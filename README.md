# CardBattle

모바일 판타지 카드 배틀 게임 프로젝트입니다. 플레이어가 보유 덱에서 전투에 사용할 카드를 선택하고, 상대 AI와 턴제로 전투를 진행합니다.

동영상 주소 : https://youtu.be/5CchH0N-6kU
Git Hub : https://github.com/robertobbaik/CardBattle

## 사용한 Unity 버전

- Unity `6000.3.11f1`

## 구현한 기능 목록

### 전투 시스템

- 턴 진행 시스템
  - 플레이어 턴과 적 턴을 번갈아 진행합니다.
  - 턴 전환 시 현재 턴 표시 UI를 출력합니다.
- 카드 선택 시스템
  - 전투 시작 전 플레이어 덱에서 사용할 카드를 선택합니다.
  - 선택된 카드로 플레이어 전장 카드를 생성합니다.
- 공격 및 스킬 처리
  - 플레이어는 공격 가능한 공개 카드를 선택하고 적 카드를 타겟팅해 공격합니다.
  - 적 AI는 공개된 카드 중 공격 가능한 카드를 골라 행동합니다.
  - 공격 후 카드별 반격/피격/파괴 처리를 수행합니다.
- 카드별 효과 처리
  - `Normal`, `Ranged`, `Peerless`, `Healer`, `Guardian`, `Assassin`, `Berserker`, `Shaman`, `Commander`, `Bomber` 카드 타입을 구현했습니다.
  - 카드 공개 시 발동 효과, 턴 시작 효과, 공격 효과, 반사 피해 등 카드별 로직을 분리했습니다.
- HP 계산
  - 카드의 현재 HP를 공격력으로 사용합니다.
  - 피해, 회복, 최대 HP, 초기 HP 기준 보정 효과를 처리합니다.
- 카드 제거 처리
  - HP가 0 이하가 된 카드는 파괴 애니메이션/FX 후 전장에서 제거됩니다.
- 신규 카드 자동 배치
  - 각 진영은 대기 카드 큐를 보유하며, 전장 카드가 제거되면 빈 슬롯에 다음 카드가 자동 배치됩니다.
- 승리 및 패배 판정
  - 플레이어/적 전장에 생존 카드가 남아 있는지 검사해 승리, 패배, 무승부를 판정합니다.
  - 결과에 따라 게임오버 UI를 표시하고 로비로 복귀합니다.
- 간단한 상대 AI
  - 적 덱을 무작위 구성/셔플합니다.
  - 공격 가능한 카드와 타겟 후보를 평가해 행동을 선택합니다.
  - 일정 대기 시간 후 공격 시퀀스를 실행합니다.

### UI

- 전투 화면
  - 플레이어/적 카드 슬롯, 액션 패널, 공격 타겟 하이라이트, 카드 공개 연출을 구성했습니다.
- 카드 선택 UI
  - 전투 시작 전 사용할 카드 선택 화면을 구현했습니다.
- 현재 턴 표시 UI
  - `YourTurn`, `EnemyTurn` 이미지 기반 턴 전환 패널을 사용합니다.
- 카드 HP 표시
  - 카드 UI에 현재 HP/초기 HP를 표시하고, HP 변경 시 갱신합니다.
- 승리 및 패배 결과 화면
  - `Victory`, `Draw`, `Defeat` 이미지 기반 결과 패널을 표시합니다.
- 로비 UI
  - 메인, 상점, 인벤토리, 컬렉션 화면 구조를 구성했습니다.
- 덱 편집 UI
  - 인벤토리 화면에서 컬렉션 카드를 선택하고 현재 덱 슬롯에 장착할 수 있습니다.
- 엑셀 기반 데이터 관리
  - 카드 능력치, 카드 타입, 카드 텍스트를 엑셀에서 관리하고 Unity용 JSON/클래스 데이터로 변환해 유지 보수를 쉽게 했습니다.

### 가산점 구현 사항

- 새로운 종류 카드 추가
  - 기본 카드 외에 `Ranged`, `Peerless`, `Healer`, `Guardian`, `Assassin`, `Berserker`, `Shaman`, `Commander`, `Bomber`를 추가했습니다.
- 카드 일러스트 적용
  - 카드 타입별 아이콘 리소스를 적용했습니다.
- 공격 / 피격 FX 연출
  - 피해, 회복, 파괴, 공격 라인, 더블 슬래시, 실드, 디버프 FX를 구현했습니다.
- 간단한 애니메이션 적용
  - 카드 뒤집기, 피해 흔들림, 파괴 흔들림, 게임 시작/FX 애니메이션을 적용했습니다.
- 카드 덱 구성 및 편집 기능
  - 인벤토리에서 현재 덱 카드를 교체할 수 있습니다.
- 카드 도감/컬렉션
  - 컬렉션 아이템 UI와 카드 목록 표시 구조를 구현했습니다.

## 주요 코드 구조

### 전투 흐름

- `GameManager`
  - 전투 전체 흐름을 제어합니다.
  - 카드 선택, 초기 카드 공개, 턴 전환, 공격 시퀀스, 승패 판정, 게임오버 처리를 담당합니다.
- `TurnManager`
  - 현재 턴 소유자와 턴 카운트를 관리합니다.
  - 플레이어/적 턴 전환과 턴 전환 UI 표시를 담당합니다.
- `PlayerController`
  - 플레이어 덱 카드 생성, 초기 공개, 턴 시작 처리, 카드 자동 보충을 담당합니다.
- `EnemyController`
  - 적 덱 생성, 카드 생성, 초기 공개, 자동 보충, AI 행동 선택 및 실행을 담당합니다.

### 카드 로직

- `BaseCard`
  - 모든 카드의 공통 상태와 동작을 정의합니다.
  - 카드 초기화, HP 처리, 공격 가능 여부, 공개 상태, 파괴 처리, 하이라이트, 카드 클릭 이벤트를 관리합니다.
- 카드별 클래스
  - `NormalCard`, `RangedCard`, `PeerlessCard`, `HealerCard`, `GuardianCard`, `AssassinCard`, `BerserkerCard`, `ShamanCard`, `CommanderCard`, `BomberCard`
  - 각 카드 타입은 `Attack`, `ReflectDamage`, `OnEnterField`, `OnTurnStart` 등을 오버라이드해 고유 효과를 구현합니다.

### 데이터

- `DataManager`
  - `Resources/Data`의 카드 데이터와 카드 텍스트 데이터를 로드합니다.
  - 카드 이름, 설명, 아이콘 조회를 제공합니다.
- `GlobalData`
  - 엑셀/JSON 기반 카드 데이터 구조를 정의하는 생성 코드입니다.
- `ExcelDataImporterWindow`
  - 엑셀 데이터를 Unity에서 사용할 JSON/클래스 형태로 변환하는 에디터 도구입니다.
  - 카드 수치나 설명을 코드에서 직접 수정하지 않고 엑셀에서 일괄 관리할 수 있어 카드 추가, 밸런스 조정, 텍스트 수정 작업을 쉽게 유지 관리할 수 있습니다.

### UI

- `CardSelectPanel`
  - 전투 시작 전 카드 선택 UI를 담당합니다.
- `ActionPanel`
  - 공격 등 전투 액션 버튼 UI를 담당합니다.
- `GameOverPanel`
  - 승리/무승부/패배 결과 이미지를 표시합니다.
- `LobbyManager`, `BaseScreen`, `MainScreen`, `InventoryScreen`, `ShopScreen`, `CollectionScreen`
  - 로비 화면 전환과 인벤토리/컬렉션 UI를 관리합니다.
  - `BaseScreen`은 모든 로비 화면의 공통 부모 클래스로, `CanvasGroup`의 `alpha`, `interactable`, `blocksRaycasts` 값을 제어해 화면 표시 여부와 입력 가능 여부를 통일해서 관리합니다.
  - `LobbyManager`는 `LobbyScreenType`과 `BaseScreen`, 버튼을 Dictionary로 연결하고, 버튼 클릭 시 선택한 화면만 `Show()`하고 나머지 화면은 `Hide()`하는 방식으로 로비 탭 전환을 처리합니다.
  - 각 화면은 `Initialize()`를 통해 최초 UI 데이터를 구성하고, 필요한 경우 `Show()`를 오버라이드해 화면이 다시 열릴 때 최신 덱/카드 정보를 갱신합니다.
- `CardItem`, `InventoryItem`, `CollectionItem`
  - 카드 목록 및 덱 편집 UI 항목을 담당합니다.

### FX

- `FXManager`
  - `Resources/FX`의 FX 프리팹을 로드하고 풀링합니다.
  - Damage, Heal, Lining, DoubleSlash, Destroy, Shield, Debuff FX를 재생합니다.
- `FX`
  - Animator 기반 FX 재생과 자동 회수를 처리합니다.

## AI 도구, 외부 에셋, AI 생성 리소스 사용 범위와 출처

### AI 도구

- OpenAI Codex/ChatGPT를 사용해 코드 구현 보조, 일부 2D VFX 스프라이트 시트 제작을 진행했습니다.
- AI 도구 사용 범위:
  - 전투/카드/UI/FX 코드 구현 및 수정 보조
  - 일부 투명 PNG 스프라이트 시트 생성 및 Unity slicing용 `.meta` 구성 보조
- 단순히 일회성 질문으로만 사용하지 않고, 프로젝트 전용 AI 작업 지침을 문서화한 뒤 해당 지침을 기준으로 구현 범위, 코드 스타일, Unity 구조, 카드 규칙, 에셋 규칙을 일관되게 관리했습니다.

### AI 협업 지침 관리

- AI에게 전달할 전체 지침을 README에 길게 직접 포함하지 않고, `Docs/CardBattle_AI_Handoff` 아래에 별도 문서로 세부 저장했습니다.
- README에는 지침의 위치와 SHA-256 해시를 남겨 어떤 기준 문서를 사용했는지 추적할 수 있게 했습니다.
- 이 방식으로 README는 제출용 요약 문서로 유지하고, 실제 세부 규칙은 별도 문서에서 관리해 AI 작업 지침이 길어져도 프로젝트 문서 구조가 무너지지 않도록 했습니다.
- 지침 파일은 작업 목적별로 분리했습니다.
  - `AGENTS.md`: AI가 처음 읽는 진입점, 작업 순서, 구현 전/후 보고 규칙
  - `ai_rules/project_overview.md`: 프로젝트 목표와 우선순위
  - `ai_rules/csharp_style.md`: C# 코드 스타일과 클래스 작성 규칙
  - `ai_rules/unity_architecture.md`: Unity 씬/매니저/MonoBehaviour 구조 규칙
  - `ai_rules/gameplay_card_battle.md`: 전투 흐름, 턴, 타겟팅, AI 행동 규칙
  - `ai_rules/CardBattle_CardRules_Final.md`: 카드별 최종 전투 규칙
  - `ai_rules/asset_scene_rules.md`: 에셋, 프리팹, 씬, UI 관리 규칙
  - `ai_rules/git_workflow.md`: 변경 파일 확인, diff, 커밋 관련 규칙

- 세부 구현 과정에서는 위 문서를 기준으로 AI가 먼저 관련 규칙을 읽고, 수정할 파일과 예상 영향 범위를 확인한 뒤 작업하도록 운영했습니다.
- 카드 규칙처럼 길고 자주 참조되는 내용은 별도 상세 문서에 저장해 프롬프트 반복 입력을 줄이고, 동일한 기준으로 구현/수정할 수 있게 했습니다.

### AI 생성 리소스

- AI/절차 생성 방식으로 제작한 VFX 리소스:
  - `Assets/05.Art/FX/CommanderBuffSheet_1024x1024.png`
  - `Assets/05.Art/FX/ShamanCurseSheet_1024x1024.png`
  - 기타 공격/피격/회복/파괴/하이라이트 계열 FX 이미지 일부
- 위 리소스는 프로젝트 내 전투 FX 연출 용도로 사용했습니다.

### 외부 패키지 및 에셋

- DOTween
  - 카드 뒤집기, 흔들림 등 Tween 애니메이션에 사용했습니다.
- AYellowpaper Serialized Collections
  - 로비 화면 매핑 등 직렬화 Dictionary 구성이 필요한 UI 구조에 사용했습니다.
- NuGetForUnity
  - 엑셀 데이터 임포트 도구에서 외부 .NET 패키지를 Unity 프로젝트에 연동하기 위해 사용했습니다.
- Unity 기본/URP 템플릿 리소스
  - 프로젝트 기본 렌더링 설정과 일부 기본 에셋 구성에 사용했습니다.
