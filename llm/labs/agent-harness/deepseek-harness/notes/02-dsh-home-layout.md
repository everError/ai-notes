# DSH_HOME 구조

문서로만 알던 부분을 실제 실행 후 확인한 기록. 관찰 기준은 developer preview
단계의 `npx @deepseek-ai/dsh web` 실행이며, Windows 환경이다.
판올림에 따라 달라질 수 있다.

## 실행 디렉터리에는 아무것도 생기지 않는다

`dsh web` 을 실행해도 실행 디렉터리에는 파일이 만들어지지 않는다. 상태는 전부
`$DSH_HOME`(기본값 `~/.dsh`)에 모인다. 실행 디렉터리는 에이전트가 작업 결과로
파일을 만들 때만 채워진다.

따라서 실행 위치를 나누는 목적은 설정 격리가 아니라 에이전트의 읽기와 쓰기 범위를
제한하는 데 있다. 설정은 실행 위치와 무관하게 공유된다.

## 디렉터리 구성

```
~/.dsh/
├── settings.yaml          플러그인별 설정
├── .credentials.yaml      API 키 실체
├── .anonymous-user-id     익명 식별자
├── profiles/
│   ├── web/               프로필 하나가 플러그인 트리 하나에 대응
│   │   ├── package.json   dsh.profile.bundles 로 번들 지정
│   │   ├── cordis.yml     트리 루트. 빈 배열이며 편집 대상이 아니다
│   │   ├── cordis.patch.yml  실제 편집 지점
│   │   └── pnpm-workspace.yaml
│   └── node_modules/      플러그인이 npm 패키지로 설치된다
├── sessions/              세션 기록
└── storages/              플러그인 저장소
```

## settings.yaml 의 최상위 키는 플러그인 이름이다

초기 상태는 다음과 같다.

```yaml
ui-onboarding:
  welcomeNoticeVersion: 2026-08-13.1
```

`ui-onboarding` 은 플러그인 이름이다. 모델 프로바이더 설정이 `llm-pi-ai` 아래에
들어가는 것도 같은 규칙이며, 설정 파일이 플러그인 단위로 분할되어 있음을 뜻한다.
프로바이더를 등록하기 전에는 이 파일에 프로바이더 항목이 존재하지 않는다.

## 플러그인은 npm 패키지다

`profiles/web/package.json` 은 다음과 같다.

```json
{
  "name": "dsh-profile-web",
  "private": true,
  "dependencies": {},
  "dsh": {
    "profile": {
      "bundles": ["@deepseek-ai/dsh-base", "@deepseek-ai/dsh-web-app"]
    }
  }
}
```

번들을 지정하면 그에 딸린 플러그인이 `node_modules` 로 설치된다. 설치된 목록에는
`@modelcontextprotocol`, `@anthropic-ai`, `@google`, `@mistralai`, `@aws-sdk` 가 포함된다.
프로바이더 지원이 하네스 본체가 아니라 개별 패키지로 분리되어 있다는 뜻이며,
MCP 서버 연결도 지원됨을 알 수 있다.

## 플러그인 트리를 고칠 때

`cordis.yml` 첫 줄에 편집하지 말라고 명시되어 있다. 트리는 다음 순서로 합성된다.

1. `package.json` 의 `dsh.profile.bundles`
2. `cordis.patch.yml`
3. `--patch` 로 넘긴 덮어쓰기 파일

따라서 구성을 바꿀 때는 `cordis.patch.yml` 을 고친다.
