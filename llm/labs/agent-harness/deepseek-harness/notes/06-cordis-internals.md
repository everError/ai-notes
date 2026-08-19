# Cordis 구현 방식

설치본에 소스가 함께 들어 있어(`@deepseek-ai/cordis/src`) 직접 읽고 정리했다.
전체 2,700줄 규모이며 핵심은 `context.ts`, `service.ts`, `reflect.ts`, `fiber.ts` 네 개다.

## 컨텍스트는 프록시다

`Context` 생성자는 자기 자신이 아니라 프록시를 반환한다.

```ts
constructor() {
  const self = new Proxy<this>(this, ReflectService.handler)
  this.root = self
  ...
  return self
}
```

그래서 `ctx.foo` 라는 평범한 속성 읽기가 전부 서비스 해석기를 지난다.
의존성 주입을 위해 별도 문법을 두지 않고 속성 접근을 가로챈 것이다.

선언하지 않은 서비스를 읽으면 다음 오류가 난다.

```
cannot get property "foo" without inject
```

주목할 점은 오류 객체를 **접근 시점에 미리 만들어 둔다**는 것이다. 해석에 실패한
뒤에 만들면 그 지점의 호출 스택이 사라지므로, 필요해지기 전에 생성해 두었다가
던진다.

## 서비스 해석은 fiber 사슬을 거슬러 올라간다

```ts
let fiber = ctx.fiber
while (true) {
  const impl = fiber.store?.[prop]
  if (impl) return getTraceable(ctx, impl.value)
  if (!fiber.runtime) throw error
  if (fiber.parent[symbols.isolate][prop] !== key) throw error
  fiber = fiber.parent.fiber
}
```

fiber 는 플러그인 하나의 실행 단위다. 자기 저장소에 없으면 부모 fiber 로 올라가며
찾는다. 중간에 격리 표지가 달라지면 거기서 멈춘다. 이것이 세션마다 플러그인이
격리되는 실제 원리다.

## 격리와 자식 컨텍스트는 프로토타입 상속

새 객체를 만들지 않고 프로토타입 체인을 쓴다.

```ts
extend(meta = {}): this {
  const self = Object.create(getTraceable(this, this))
  ...
}

isolate(name: string, label?: symbol) {
  const shadow = Object.create(this[symbols.isolate])
  shadow[name] = label ?? Symbol(name)
  return this.extend({ [symbols.isolate]: shadow })
}
```

`isolate` 는 격리 표를 프로토타입으로 상속한 뒤 해당 이름 하나만 새 심볼로 덮는다.
나머지 이름은 부모 것을 그대로 본다. 부모는 변경되지 않는다.

`intercept` 도 같은 구조이며, 설정 병합은 프로토타입 체인을 거슬러 올라가면서
루트에 가까운 것부터 적용한다.

```ts
while (this.name in intercept) {
  if (Object.hasOwn(intercept, this.name)) configs.unshift(intercept[this.name])
  intercept = Object.getPrototypeOf(intercept)
}
```

## 되돌리기의 실체

핵심 기능이라고 내세우는 정리 기능은 `effect` 와 처분 목록으로 구현된다.

```ts
effect(execute: () => Effect, label = 'anonymous') {
  const disposables: Disposable[] = []
  let disposing = false
  const dispose = () => {
    if (disposing) return disposalTask
    disposing = true
    for (const disposable of disposables.splice(0).reverse()) { ... }
  }
  ...
}
```

정리 규칙이 세 가지다.

- 효과 실행 중에 만들어진 처분자를 모아 둔다
- 해제할 때 **역순**으로 실행한다
- 두 번 호출하면 아무 일도 하지 않는다

플러그인이 내려갈 때는 모아 둔 것을 전부 비우고 기다린다.

```ts
private async _unload() {
  await Promise.all(this._disposables.clear().map(async (dispose) => {
    try { await runDisposable(dispose) }
    catch (reason) { this.ctx.logger.error(reason) }
  }))
  this.store = undefined
  ...
}
```

하나가 실패해도 나머지는 계속 정리한다. 이벤트 구독, 열린 연결, 타이머가 남지
않는 이유가 여기에 있다.

## 정리 상태를 조회할 수 있다

효과마다 이름표와 자식 목록을 갖는 메타 정보가 붙는다.

```ts
const meta: EffectMeta = { label, children: [] }
```

`getEffects()` 로 살아 있는 효과를 트리로 조회한다. 어떤 플러그인이 무엇을 붙들고
있는지 볼 수 있으므로, 정리가 안 된 자원을 추적하는 진단 수단이 된다.

## 서비스는 생성만 하면 등록된다

`Service` 를 상속하고 `super(ctx, name)` 을 부르면 등록까지 끝난다.

```ts
constructor(protected ctx: Context, name: string) {
  ...
  self.ctx.reflect.provide(name, self, this[symbols.check])
  return self
}
```

주석에 명시된 대로 소유한 fiber 가 내려가면 자동으로 해제된다. 등록과 해제를
따로 쓰지 않아도 되는 구조다.

## 판별을 instanceof 로 하지 않는다

```ts
static is(value: any): value is Context {
  return !!value?.[Context.is as any]
}

static {
  Context.is[Symbol.toPrimitive] = () => Symbol.for('cordis.is')
  Context.prototype[Context.is as any] = true
}
```

전역 심볼로 표식을 남긴다. 그래서 실행 영역이 다르거나 cordis 사본이 여러 벌
있어도 판별된다. dsh 가 모델이 쓴 코드를 별도 실행 영역에서 돌리는 구조이므로
이 성질이 필요하다.

`Service` 는 한 걸음 더 나아가 `Symbol.hasInstance` 를 직접 구현해 프로토타입
체인을 훑는다. 생성자가 프록시일 수 있어 기본 `instanceof` 가 통하지 않기 때문이다.

## 정리

| 기능 | 구현 수단 |
| --- | --- |
| 의존성 주입 | Proxy 의 get 가로채기 |
| 서비스 조회 | fiber 부모 사슬 순회 |
| 범위 격리 | 프로토타입 체인 + 심볼 표지 |
| 설정 가로채기 | 프로토타입 체인 순회 병합 |
| 되돌리기 | 처분자 목록과 역순 실행 |
| 진단 | 효과 메타 트리 |
| 영역 간 판별 | 전역 심볼 표식 |

특별한 런타임 기능을 쓰지 않는다. Proxy, 프로토타입, 심볼이라는 표준 요소만으로
구성했다. 웹어셈블리나 별도 실행기가 필요 없는 이유다.
