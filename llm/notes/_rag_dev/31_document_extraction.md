# 31 · 문서 추출 — PPTX·PDF·Excel

## 추출이 RAG의 1차 품질 결정 요인

원본 문서가 텍스트로 잘 추출되지 않으면 그 뒤의 임베딩·reranker가 아무리 좋아도 답이 안 나옵니다. 특히:

- **표 (table)**: 단순 cell join은 의미 손실. 헤더와 본문을 구분해야
- **이미지·다이어그램**: 텍스트 추출에 안 잡힘 → VLM 캡션 ([30_vlm_and_captioning.md](30_vlm_and_captioning.md))
- **수식·차트**: 일부 라이브러리만 잡음
- **임베드 객체**: PPT 안에 PPT/Excel을 OLE로 박은 케이스
- **레이아웃**: 다단·각주 등이 정렬 깨짐

## 라이브러리 카탈로그

### PPTX

| 라이브러리 | 라이선스 | 특성 |
|---|---|---|
| **python-pptx** | MIT | 가장 표준, 텍스트·표·picture·OLE 모두 접근 |
| **Aspose.Slides** | 상용 | 정확도 ★, 비싼 라이선스 |
| **Apache POI (Java)** | Apache 2.0 | Java 환경, 정확도 OK |

### PDF

| 라이브러리 | 라이선스 | 특성 |
|---|---|---|
| **pdfplumber** | MIT | 텍스트 + 표 추출, 한국어 OK |
| **pypdf** | BSD-3 | 가벼움, 표 추출 약함 |
| **PyMuPDF (fitz)** | AGPL/상용 | 빠름, 텍스트·이미지·렌더 모두 |
| **pdfminer.six** | MIT | 레이아웃 분석 강 |
| **Unstructured** | Apache 2.0 | 다국어 OCR + 레이아웃 분석 통합 |
| **Nougat (Meta)** | MIT | 학술 PDF + 수식, OCR 기반 |
| **MinerU** (OpenDataLab) | Apache 2.0 | PDF→markdown, 표·수식 강 |

### Excel (XLSX)

| 라이브러리 | 라이선스 | 특성 |
|---|---|---|
| **openpyxl** | MIT | 표준, 수식 평가값 / 셀 단위 접근 |
| **xlrd** (legacy) | BSD | 구 .xls 전용 |
| **pandas (read_excel)** | BSD | DataFrame으로 즉시 |

### 통합 추출

| 도구 | 라이선스 | 특성 |
|---|---|---|
| **Docling** (IBM) | MIT | PPTX·PDF·Excel·DOCX 통합, 표 정확도 97.9%, AAAI 2025 |
| **Unstructured.io** | Apache 2.0 | 다양한 포맷 + OCR + 레이아웃 |
| **LlamaParse** | SaaS | 표 추출 SOTA 수준, API |
| **Tika** (Apache) | Apache 2.0 | 다양한 포맷 메타데이터 + 텍스트 |

## PPTX 추출의 흔한 함정

### 1. 비-ASCII 디렉토리 경로

일부 환경에서 라이브러리가 내부 경로를 mbcs/cp949로 처리해 한글·중국어 폴더 경로에서 "Package not found" 에러 냅니다.

해법: `path.open("rb")` 로 바이트를 BytesIO로 넘기면 내부 경로 재구성 단계 우회.

```python
with pptx_path.open("rb") as fh:
    prs = Presentation(fh)
```

### 2. PlaceholderGraphicFrame

placeholder의 `has_text_frame`이 True여도 `text_frame` attribute가 없는 케이스가 있습니다. `getattr(shape, "text_frame", None)` 가드 필수.

### 3. 그룹 shape

일부 템플릿이 callout/콜라주를 group으로 묶어둠. `shape_type == 6` (GROUP) 일 때 자식 재귀.

### 4. 표 → markdown 변환

```
나쁜 예 (의미 손실):
  cell1 | cell2 | cell3

좋은 예 (헤더 분리):
  | cell1 | cell2 | cell3 |
  |---|---|---|
  | val1 | val2 | val3 |
```

LLM이 표 구조를 보고 인용·합성을 정확히 합니다.

### 5. OLE 임베디드 자식

PPTX는 zip이고, `ppt/embeddings/...` 안에 다른 PPTX/XLSX/DOCX를 박을 수 있습니다. 세 형태:

- **(a) 모던 OOXML** (`.pptx/.xlsx/.docx`) — 그대로 zip으로 풀어 추출
- **(b) `.bin` 안에 OOXML zip 래핑** — 내부 zip 추출
- **(c) 레거시 `.ppt/.xls/.doc`** — LibreOffice headless로 OOXML 변환 후 처리

추출된 자식은 부모와 동일 흐름으로 다시 처리되며, 메타에 부모 doc_id를 남기면 검색 결과에서 부모로 거슬러 올라갈 수 있습니다.

### 6. 슬라이드 PNG 렌더

검색 결과 카드에 슬라이드 이미지를 보여주려면 PNG 렌더가 필요합니다.

```
PPTX  ─LibreOffice headless─→  PDF  ─PyMuPDF (144 DPI)─→  slide_001.png, slide_002.png, ...
```

LibreOffice 명령:
```
soffice --headless --convert-to pdf --outdir <out_dir> <pptx_path>
```

PNG 렌더 후 PyMuPDF:
```python
import fitz
doc = fitz.open(pdf_path)
for i, page in enumerate(doc, 1):
    pix = page.get_pixmap(dpi=144)
    pix.save(f"slide_{i:03d}.png")
```

## PDF 추출의 함정

- pdfplumber가 표를 못 잡는 경우 종종 있음 (스캔 PDF, 비표준 구조)
- 표는 잡혀도 첫 행이 헤더가 아닐 수 있음
- 레이아웃이 다단(2열)이면 텍스트 순서가 섞임 (좌→우 vs 위→아래)
- 한국어 인코딩이 잘못 박힌 PDF는 "□□□" 같은 깨진 문자

스캔 PDF나 학술 PDF는 OCR 기반(Nougat, Unstructured, MinerU)이 더 정확합니다.

## Excel 추출의 함정

- `data_only=True` 옵션으로 수식 평가값 가져오기 (`=SUM(A1:A5)` 보다 `123` 이 LLM에 의미)
- `read_only=True` 로 큰 워크북 메모리 절약
- 시트 전체를 한 텍스트로 만들면 길이 폭발 → 행 그룹별 청크로 분할
- 첫 행이 항상 헤더는 아니지만 default 가정으로 충분한 경우 많음
- 병합 셀(merged cell)은 첫 셀에만 값, 나머지는 None — 명시적 처리 필요할 수 있음

## 추출 단계의 출력 모양 (예시 데이터 클래스)

```python
@dataclass
class ExtractedUnit:
    unit_type: str           # "slide" | "page" | "sheet"
    unit_no: str             # "1", "12", or sheet name
    text: str                # 본문 텍스트
    table_md: list[str]      # markdown 표들
    image_path: str          # 슬라이드/페이지 PNG 절대 경로
    embedded_image_paths: list[str]    # 임베디드 PNG 경로
    parent_doc_id: str       # OLE 임베드 자식이면 부모 doc_id
    parent_unit_no: str
```

이런 구조로 통일하면 인덱서가 포맷 무관하게 동일 흐름으로 처리합니다.

## 외부 학습 자원

- [python-pptx 문서](https://python-pptx.readthedocs.io/)
- [pdfplumber GitHub](https://github.com/jsvine/pdfplumber)
- [openpyxl 문서](https://openpyxl.readthedocs.io/)
- [Docling](https://github.com/docling-project/docling) — 통합 추출, 표 정확도 ★
- [Unstructured.io 문서](https://unstructured-io.github.io/unstructured/)
- [LibreOffice headless 가이드](https://wiki.documentfoundation.org/Documentation/UserManual/Appendix/Convert)
