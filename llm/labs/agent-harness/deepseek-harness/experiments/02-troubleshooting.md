# 실행 중 겪은 문제

## 실행 디렉터리에 파일이 생기지 않는다

증상만 보면 실행이 안 된 것처럼 보이나 정상이다. 상태는 전부 `$DSH_HOME` 으로 간다.
자세한 내용은 `../notes/02-dsh-home-layout.md` 를 참고한다.

## duplicate loader entry id

```
plugin tree failed to load: failed to apply loader entry include (cordis:include):
duplicate loader entry id: cordis-host-runner
```

원인은 이미 존재하는 id 를 프로필 패치에서 `insert` 로 다시 넣은 것이다.
`insert` 는 새 행을 넣는 조작이므로 중복을 허용하지 않는다.

기존 행을 고칠 때는 `insert` 없이 최상위에 `id` 를 적어 덮어쓴다. 어떤 id 가 이미
있는지는 번들의 `cordis.patch.yml` 을 확인한다. 이때 `dsh-base` 뿐 아니라 프로필이
쓰는 모드 번들도 함께 봐야 한다. `web` 프로필은 `dsh-web-app` 번들을 통해
`cordis-host-runner`, `cordis-client-runner`, `ui-cordis` 를 이미 갖고 있다.

## Host Cordis inspect provider "Service" is already registered

```
agent-presets: preset "cordis" failed to mount:
failed to apply loader entry tool-cordis (@deepseek-ai/dsh-tool-cordis):
Host Cordis inspect provider "Service" is already registered
```

원인은 자기 참조 도구를 프로필 패치로 직접 넣은 것이다. 이 도구는 `cordis` 에이전트
프리셋이 이미 올리므로 같은 서비스가 두 번 등록되어 세션이 열리지 않는다.

프로필 패치를 빈 배열로 되돌리고, 화면의 모드 선택기에서 프리셋을 고르는 방식으로
쓴다. 새 기능을 넣기 전에 프리셋이 이미 제공하는지 먼저 확인한다.

## 정리

세 건 모두 원인이 같다. 어떤 계층이 무엇을 이미 제공하는지 확인하지 않고 추가한 것이다.
확인 순서는 다음과 같다.

1. 에이전트 프리셋이 제공하는가
2. 프로필의 모드 번들이 제공하는가
3. `dsh-base` 가 제공하는가
4. 그래도 없으면 프로필 패치로 추가한다
