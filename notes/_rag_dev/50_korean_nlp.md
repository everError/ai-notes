# 50 · 한국어 NLP — 토크나이저, 임베딩, 검색

## 한국어 NLP의 특수성

영어는 단어 사이에 공백이 있어 토큰화가 단순. 한국어는:

1. **교착어** — 어간에 어미가 붙어 한 단어가 길어짐
   ```
   가다 (어간) + ㄴ다(현재) → 간다
   가다 + 았었습니다(과거) → 갔었습니다
   ```
2. **조사 결합** — 명사 뒤에 조사(이/가/은/는/을/를...)가 붙음
   ```
   "회의실" + "에서" + "는" → "회의실에서는"
   ```
3. **합성·복합** — 명사 여러 개가 띄어쓰기 없이 결합
   ```
   "프로젝트관리자급여명세서"
   ```

영어 위주 토크나이저(BPE, WordPiece)는 한국어를 잘 못 자릅니다. 서브워드 단위로 토큰이 잘려도 의미 단위와 일치하지 않아 검색 정확도 ↓.

## 형태소 분석 (Morphological Analysis)

한국어를 의미 단위(형태소)로 분리:

```
"회의실에서는 마이크 문제가 발생했다"
   ↓ 형태소 분석
[("회의", NNG), ("실", XSN), ("에서", JKB), ("는", JX),
 ("마이크", NNG), ("문제", NNG), ("가", JKS),
 ("발생", NNG), ("하", XSV), ("었", EP), ("다", EF)]
```

NNG=일반명사, JKB=부사격조사, JX=보조사, EP=선어말어미, EF=종결어미 등.

검색에 의미 있는 건 **명사·동사·형용사·외래어** 같은 content words. 조사·어미는 제외.

## 한국어 형태소 분석기 카탈로그

| 도구 | 라이선스 | 특성 |
|---|---|---|
| **Kiwi** (kiwipiepy) | LGPL-2.1+ | C++ 빠른 분석기, 활발한 메인테넌스, Python 친화 |
| **Mecab-ko** | BSD/GPL | 오래됨, 빠름, 사전 관리 까다로움 |
| **Khaiii** (카카오, 2018) | Apache 2.0 | 정확하지만 최근 유지보수 부족 |
| **KoNLPy** | GPL | 여러 분석기(Mecab/Hannanum/Kkma/Komoran) wrapper, Java 의존 |
| **Komoran** | Apache 2.0 | KoNLPy 묶음 중 하나, 오타 강건 |
| **soynlp** | LGPL-3.0 | 통계 기반 (학습 코퍼스로 자동) |

선택 가이드:
- **Python 친화 + 빠름 + 활발 메인테넌스 → Kiwi**
- 사전 커스터마이징 필요 → Mecab-ko
- 라이브 데이터 학습 기반 → soynlp

## Kiwi 사용 예

```python
from kiwipiepy import Kiwi
kiwi = Kiwi()
tokens = kiwi.tokenize("프로젝트 일정 변경 요청")
# → [Token('프로젝트', 'NNG'), Token('일정', 'NNG'),
#    Token('변경', 'NNG'), Token('요청', 'NNG')]

# 명사·동사·형용사만 필터
content_words = [t.form for t in tokens
                 if t.tag.startswith(('NN', 'VV', 'VA', 'SL'))]
```

## 형태소 분석을 어디에 쓰나

### 1. BM25 sparse vector 입력

대부분의 벡터 DB의 standard analyzer는 한국어를 못 자릅니다. "회의실에서는" 같은 띄어쓰기 없는 한국어는 분석기 입장에선 통째로 한 토큰.

해법: **인덱싱 측에서 미리 형태소 분석 → 공백 join → 분석기는 그냥 공백으로 자름**:

```
원본:           "회의실에서는 마이크 문제가 발생했다"
Kiwi:          [회의, 실, 마이크, 문제, 발생, 하다]   (조사·어미 제거)
공백 join:      "회의 실 마이크 문제 발생 하다"
   ↓
DB의 standard analyzer는 공백으로 자르기만 → BM25 인덱스
```

검색 시에도 동일한 형태소 분석으로 query를 처리해야 분포가 맞습니다.

### 2. POS 필터로 노이즈 제거

검색에 의미 있는 토큰만 남기기:

```python
KEEP_TAGS_PREFIX = ("NN", "NR", "NP",   # 명사·수사·대명사
                    "VV", "VA", "VX",   # 동사·형용사
                    "MM", "MA",         # 관형사·부사
                    "XR",               # 어근
                    "SL", "SH", "SN")   # 외래어·한자·숫자
```

도메인에 따라:
- 제품·코드가 중요 → `SN`(숫자), `SL`(외래어/영문) 포함
- 고유명사 중요 → `NNP` 명시 (Kiwi의 NNP 태그)

## 한국어 임베딩 모델

| 모델 | 특성 |
|---|---|
| **bge-m3** (BAAI) | 다국어, 한국어 양호, MIT |
| **multilingual-e5-large** (Microsoft) | 100여개 언어 |
| **jina-embeddings-v3** | 다국어, 긴 컨텍스트 |
| **KR-SBERT** 계열 | 한국어 특화, 영어 약함 |
| **ko-sroberta-multitask** | 한국어 baseline |
| **upskyy/bge-m3-korean** | bge-m3 한국어 추가 fine-tune |

가이드:
- 한국어 + 영문 혼재 (제품명·약어) → 다국어(bge-m3, e5, jina)
- 순수 한국어 → KR-SBERT 같은 한국어 특화도 OK

## 한국어 reranker

| 모델 | 특성 |
|---|---|
| **bge-reranker-v2-m3** (BAAI) | 다국어, 한국어 양호 |
| **dragonkue/bge-reranker-v2-m3-ko** | 한국어 fine-tune ★ |
| **Cohere rerank-3.5** | API, 다국어 ★ |

벤치마크 (한국어 IR):
- baseline 다국어 reranker nDCG ≈ 0.69
- 한국어 fine-tune 변형 nDCG ≈ 0.81

큰 차이. 도메인이 한국어 위주면 fine-tune 변형 검토 가치 ★.

## 흔한 함정

### 1. 띄어쓰기 일관성

같은 단어가 띄어쓰기 다르게 적힌 경우:
- "프로젝트관리" vs "프로젝트 관리"
- "T 1" vs "T1"

해법:
- 임베딩(다국어 모델)은 어느 정도 동일 매칭
- BM25는 토큰 다르면 매칭 실패 → 형태소 분석으로 동일 형태로 정규화

### 2. 한자·약어·기호

"(주)", "ε", "α", "β" 같은 특수 기호가 일부 분석기에 무시되거나 다르게 잘림. POS 필터에서 빠지므로 BM25에선 손실. 임베딩이 보강.

### 3. OOV (Out-of-Vocabulary)

전문 용어·코드가 학습 데이터에 등장 안 함:
- 임베딩: 흐릿하게 인식 (점수 ↓)
- BM25: 정확 매칭 (강점)
- 그래서 hybrid 중요

### 4. 인코딩 오염

한국어 PDF 일부가 깨진 인코딩으로 텍스트 추출 시 "□□□" 또는 cp949 mojibake. 추출 단계에서 detect → 폴백 (OCR 또는 다른 라이브러리).

## 외부 학습 자원

- [Kiwi GitHub](https://github.com/bab2min/Kiwi)
- [KoNLPy 공식 문서](https://konlpy.org/)
- [세종 코퍼스 POS 태그셋](https://kkma.snu.ac.kr/documents/?doc=postag)
- [BGE-M3 multilingual paper](https://arxiv.org/abs/2402.03216)
- [한국어 embedding 리더보드 (Ko-MTEB)](https://huggingface.co/spaces/upskyy/ko-mteb)
- [HAERAE-Bench](https://huggingface.co/datasets/HAERAE-HUB/HAE_RAE_BENCH) — 한국어 LLM 평가
