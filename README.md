# Bug-Fighter-1v1_Action_Game-built-with-Unity
Bug Fighter : Unity C# 기반 1:1 대전 액션 게임

Unity 엔진과 C#을 사용하여 개발한 2D 로컬 멀티플레이 1:1 대전 액션 게임입니다.
생태계속의 벌레인 개미와 벌로 플레이를 진행하게 되며 다양한 기술들을 활용하여 상대방을 제압하는 게임입니다. 

프로젝트 기간: 2024.09 ~ 2024.12 (상명대학교 팀프로젝트)

<br>

## 🐜 게임 개요
<img width="600" height="300" alt="Image" src="https://github.com/user-attachments/assets/85193567-3862-4ec2-a510-f4bf7f78e0b4" />
<br>

## 🎮플레이
### 첫번 째 스테이지
<img width="600" height="300" alt="Image" src="https://github.com/user-attachments/assets/109bc75b-58da-41ce-963e-dcdb26db7a90" />
<br>

### 두번 째 스테이지
<img width="600" height="300" alt="Image" src="https://github.com/user-attachments/assets/01d99af8-edc2-407a-a4c5-fe1875f70919" />
<br>

### 스킬
| 개미 근거리 | 벌 근거리 | 원거리 | 쉴드 |
|---|---|---|---|
| <img width="170" height="150" alt="Image" src="https://github.com/user-attachments/assets/e384569a-7c31-4e39-9d5f-057162d5bd95" /> | <img width="200" height="110" alt="Image" src="https://github.com/user-attachments/assets/47e72685-1767-46a0-bcbd-37623743c1fe" /> | <img width="200" height="130" alt="Image" src="https://github.com/user-attachments/assets/32d4d030-f841-4d65-9058-dc8ee134b964" /> | <img width="300" height="150" alt="Image" src="https://github.com/user-attachments/assets/d1f2477c-1a8a-4302-af08-5bf9b87a106d" /> |
<br>

| 개미 궁극기1 | 개미 궁극기2 | 개미 궁극기 3 |
|---|---|---|
| <img width="100" height="100" alt="Image" src="https://github.com/user-attachments/assets/9b361166-749d-4581-ad5f-a2e97ade550b" /> | <img width="250" height="130" alt="Image" src="https://github.com/user-attachments/assets/0eb275eb-6046-4931-927a-85de5b533a07" /> | <img width="250" height="130" alt="Image" src="https://github.com/user-attachments/assets/d561e7aa-d9f5-4b95-8a12-b8ecfe786391" /> |
<br>

| 벌 궁극기1 | 벌 궁극기2 |
|---|---|
| <img width="250" height="130" alt="Image" src="https://github.com/user-attachments/assets/9d9becb8-b7c7-434d-8006-2d77b2c770c4" /> | <img width="250" height="130" alt="Image" src="https://github.com/user-attachments/assets/db9395f2-3f9b-418c-9075-7759a68146ea" /> |
<br>

### 게임 종료
<img width="600" height="300" alt="Image" src="https://github.com/user-attachments/assets/046cf19a-78c2-4920-b0fa-7e03019af9cc" />
<img width="600" height="300" alt="Image" src="https://github.com/user-attachments/assets/8fc9931f-2829-4a23-b253-91ef3d6422f4" />
<br>

## ✨ 담당 역할
캐릭터 공격 및 이동: 각 캐릭터(개미, 벌)의 공격(원거리, 근거리) 스프라이트와 콜라이더를 연결하여 정교한 공격 범위 및 피격 판정을 구현했습니다.

2층 플로어 구현 : Platform Effector 2D를 활용하여 아래에서 위로만 통과할 수 있는 발판을 구현하여 맵의 전략적인 깊이를 더했습니다.

물리 효과 제어: 캐릭터가 점프 후 바닥에 튕기는 현상을 막기 위해, Physics Material 2D의 Bounciness 값을 0으로 설정하여 안정적인 조작감을 제공합니다.

회복 아이템: 플레이어가 HP를 회복할 수 있는 '젤리' 오브젝트와 일정한 시간마다 재생성되는 리스폰 타이머를 구현했습니다.

궁극기 시스템: 시전 시 캐릭터 위치를 기반으로 이펙트와 별도의 콜라이더 박스를 생성하여, 상대방과 오버랩될 시 강력한 피해를 주는 궁극기 스킬을 구현했습니다.


## 😱 문제 발생

궁극기 스킬을 구현하는 과정에서, 처음에는 간단하게 화려한 이펙트 스프라이트 자체에 Box Collider 2D를 크게 추가하여 공격 판정을 만들려고 했습니다. 가장 직관적인 방법이었지만, 테스트 과정에서 다음과 같은 두 가지 문제점을 발견했습니다.

- 유령 타격(Ghost Hit) 현상: 이펙트가 사라진 후에도 콜라이더가 미세하게 남아있어, 스킬이 끝났음에도 상대방이 피해를 입는 현상이 발생

- 딜레이 차이: 이펙트의 특정 모션은 아직 상대플레이어에 닿지도 않았는데 피해를 입히는 현상 발생
  
## 😀 문제 해결
해당 문제를 해결하기 위해 스킬 입력 시, 스프라이트를 플레이어 근처에 생성이 되게끔 구현 후 해당 스프라이트의 Position을 참조하여 콜리전을 생성하였습니다. 

- 판정 객체의 동적 생성: 스킬 시전 시, 이펙트 객체와는 별개로 OnTriggerEnter2D를 감지할 보이지 않는 판정용 오브젝트를 동적으로 생성

- 적절한 생명주기 관리: 이 판정용 오브젝트는 **정확히 0.2초(이펙트의 핵심 지속 시간)만 활성화된 후 즉시 스스로를 파괴(Destroy)**하도록 설계.
  
이를 통해 '유령 타격' 문제를 원천적으로 차단하고, 원하는 순간에만 공격 판정이 일어나도록 만들 수 있었습니다.
