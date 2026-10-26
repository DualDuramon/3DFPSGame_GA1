# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 프로젝트 개요

Unity **6000.3.22f1** (URP 17) 기반 3D FPS 학습용 게임. 플레이어가 총·수류탄으로 좀비를 처치한다. 씬은 `Assets/01_Scenes/GameScene.unity` 하나뿐이며, 직접 작성한 게임 코드는 전부 `Assets/02_Scripts/`에 있다. `Assets/Plugins/`(Lean Pool, MeshSlicer 등), `Assets/10_Effects/`, `TextMesh Pro`는 외부 에셋이므로 수정하지 않는다.

## 명령어

빌드·실행·테스트는 Unity 에디터에서 한다(CLI 빌드 스크립트나 테스트 코드는 없음). CLI에서 쓰는 명령은 아래 두 가지다.

```bash
dotnet tool restore                       # Husky.Net 설치 (최초 1회)
dotnet husky install                      # git hook 연결

# pre-commit hook과 같은 포맷 검사 (Assets/02_Scripts만 대상)
dotnet format 3DFPSGame_GA1.slnx --verify-no-changes --severity warn --include Assets/02_Scripts/
# 자동 수정: 위 명령에서 --verify-no-changes 를 빼고 실행
```

`.csproj`/`.slnx`는 Unity가 생성하는 파일이다. 새 스크립트를 추가한 직후에는 에디터가 다시 생성하기 전까지 `dotnet format` 대상에 포함되지 않을 수 있다.

### 커밋 규칙 (commit-msg hook에서 검사)

`type: 설명` 형식만 허용한다. type은 `asset|build|chore|ci|docs|feat|fix|perf|refactor|revert|style|test` 중 하나이고, 설명은 한국어로 쓴다(예: `feat: 줌기능 구현`, `fix: 좀비 Healthbar 버그 수정`).

## 아키텍처

### 전역 게임 상태 게이트
`GameManager`(지연 탐색 싱글턴)가 `GameState`(Ready → Playing → GameOver)를 관리한다. 시작 시 "준비/시작!" 코루틴이 끝나야 `Playing`이 된다. **입력이나 AI를 처리하는 모든 `Update()`는 첫 줄에서 `GameManager.Instance.GameState != GameState.Playing`이면 return한다.** 새로운 동작 컴포넌트를 만들 때도 이 패턴을 따른다. 플레이어 체력이 0이 되면 `PlayerCommonStatus`가 `GameManager.GameOver()`를 호출한다.

### 데미지 시스템
`IDamageable.TakeDamage(DamageInfo)`가 공통 계약이다. `DamageInfo`는 데미지 양, 맞은 지점, 맞은 면의 법선을 담는다. 총(Raycast), 수류탄 폭발, 좀비 공격 모두 `TryGetComponent(out IDamageable)`로 대상을 찾아 호출한다. 구현체는 `Zombie`, `PlayerCommonStatus`, `Box`, `Drum`이다.

### 플레이어 = 역할별 컴포넌트 + 공용 스탯
`PlayerCommonStatus`가 체력·스태미나·이동·공격 수치를 모두 들고 있고(`[SerializeField]` 필드 + 읽기 전용 프로퍼티), `PlayerMove`/`PlayerRotate`/`PlayerGunFire`/`PlayerBombFire`/`PlayerZooming`이 `[RequireComponent]`나 `GetComponent`로 이를 참조한다. 스탯은 개별 컴포넌트가 아니라 여기에 추가한다. UI(`Player/UI/`)는 `OnTakeHit` 이벤트를 구독하거나 Ratio 프로퍼티를 읽는다.

### 카메라 시점: 전략 패턴
`CameraView` 네임스페이스에 있다. `CameraViewSwitcher`가 `ICameraViewStrategy` 구현체 6개(1인칭/3인칭/숄더/쿼터/아이소메트릭/탑다운)를 V키로 순환한다. 회전은 `Update`에서, 위치는 `LateUpdate`에서 적용한다. 전략은 switcher의 `internal` 프로퍼티와 `ApplyPose`/`SetPerspective`/`SetOrthographic`을 통해서만 카메라를 조작한다. 새 시점은 전략 클래스를 추가하고 `Awake`의 배열에 등록하면 된다.

### 좀비 AI: enum 기반 FSM
`Zombie.Update()`의 `switch(_state)`로 Idle/Roaming/Trace/Attack/Comback/Hit/Death를 처리한다. 이동은 `CharacterController.SimpleMove`를 쓴다. 실제 공격 판정은 애니메이션 이벤트에서 일어난다. `ZombieAnimationModule.OnAttackEvent()`가 `Zombie.HandleAttackPlayer()`를 호출하고, 여기서 Raycast로 판정한다. 그래서 공격 타이밍을 바꾸려면 애니메이션 클립의 이벤트도 함께 확인해야 한다.

### 오브젝트 풀링
이펙트는 `LeanPool.Spawn`으로 생성한다. 풀링 대상 파티클 프리팹에는 `PooledParticleEffect`(`IPoolable`)를 붙여 재생이 끝나면 자동으로 Despawn되게 한다. 좀비 피 이펙트는 아직 `Instantiate`를 쓰고 있다.

### 입력 / 커서
Input System 패키지가 설치되어 있지만 코드는 **구 Input Manager**(`Input.GetButton("Fire1")`, `"Reload"`, `Input.GetAxis("Mouse X")` 등)를 사용한다(Active Input Handling = Both). `CursorManager`는 기본적으로 커서를 잠그고 Left Alt를 누르는 동안만 해제한다. 사격 컴포넌트는 `Cursor.visible`이면 발사하지 않는다.

## 코드 컨벤션

- 로그는 `Debug.Log` 대신 `GameLogger.Log/LogWarning/LogError`를 사용한다. `[Conditional("ENABLE_LOG")]`가 붙어 있어서, `ENABLE_LOG` 심볼이 없는 빌드에서는 호출 자체가 제거된다(현재 Standalone에 정의됨).
- private 필드는 `_camelCase`, 상수는 `PascalCase`를 쓴다. Inspector 노출은 `[SerializeField] private`와 `[Header]` 그룹으로 하고, 외부에는 get-only 프로퍼티로 공개한다.
- Animator 파라미터는 `Animator.StringToHash`로 캐싱해서 사용한다.
- 주석과 문서는 한국어로 작성한다.

## 코드 리뷰

리뷰를 요청받으면 `Docs/CodeReviewGuide.md`의 기준을 따른다. 요점은 다음과 같다.
- Critical / High / Medium / Low 중요도로 분류한다.
- 결과는 **총평 → 발견된 문제(중요도·위치·설명·결과·수정 제안) → 설계 개선 제안(명확할 때만) → 잘 작성된 점** 순서로 쓴다.
- 확실하지 않은 내용은 단정하지 않는다. 문제가 없으면 억지로 만들지 않는다. 프로젝트 규모에 비해 과한 추상화는 제안하지 않는다.
