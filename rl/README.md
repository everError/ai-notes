## Reinforcement Learning

에이전트(Agent)가 환경(Environment)과 상호작용하며
보상(Reward)을 최대화하는 방향으로 행동 정책(Policy)을 학습하는 방법을 정리합니다.

> RL은 특정 모델 하나를 의미하는 것이 아니라,
> 보상을 기반으로 정책을 학습하는 학습 방법(Learning Paradigm)입니다.

### Core Concepts

- Agent
- Environment
- State
- Action
- Reward
- Policy
- Value Function
- Q-Function
- Return
- Exploration / Exploitation

### 기본 구조

```text
        ┌─────────────────┐
        │   Environment   │
        └────────┬────────┘
                 │
               State
                 ↓
        ┌─────────────────┐
        │      Agent      │
        │                 │
        │     Policy      │
        └────────┬────────┘
                 │
               Action
                 ↓
        ┌─────────────────┐
        │   Environment   │
        └────────┬────────┘
                 │
              Reward
                 │
                 └──────────→ Agent
```

에이전트는 현재 상태(State)를 관찰하고 행동(Action)을 선택합니다.

환경은 행동의 결과로 다음 상태와 보상(Reward)을 반환하며,
에이전트는 반복적인 상호작용을 통해 더 나은 정책(Policy)을 학습합니다.

### Reinforcement Learning and Models

강화학습(RL)은 특정한 하나의 모델을 의미하지 않습니다.

정책(Policy)을 어떤 방식으로 표현하고 학습하느냐에 따라
다양한 모델을 사용할 수 있습니다.

예를 들어 Policy를 Neural Network로 구현할 수 있습니다.

```text
Environment
     │
     ▼
   State
     │
     ▼
 Policy / Neural Network
     │
     ▼
   Action
     │
     ▼
Environment
     │
     ├── Next State
     └── Reward
            │
            ▼
       Model Update
```

학습이 완료되면 일반적인 실행 과정에서는 다음과 같이 동작합니다.

```text
State
  │
  ▼
Learned Policy / Model
  │
  ▼
Action
```

즉, RL은 **학습 방법**이고 Neural Network는 그 정책을 구현하는 **모델**이 될 수 있습니다.

### Markov Decision Process

강화학습 문제를 수학적으로 표현하는 기본 모델인
MDP(Markov Decision Process)를 정리합니다.

- State
- Action
- Transition
- Reward
- Discount Factor

### Value-Based Methods

상태 또는 행동의 가치를 학습하여 행동을 선택하는 방법을 다룹니다.

- Q-Learning
- SARSA
- DQN
- Double DQN
- Dueling DQN

### Policy-Based Methods

행동의 가치보다 좋은 행동을 선택하는
Policy 자체를 학습하는 방법을 다룹니다.

- Policy Gradient
- REINFORCE

### Actor-Critic

Policy를 학습하는 Actor와
행동 또는 상태의 가치를 평가하는 Critic을 함께 사용하는 방법을 다룹니다.

- Actor-Critic
- A2C
- A3C
- PPO
- SAC

### Exploration

에이전트가 이미 알고 있는 좋은 행동을 선택하는 것과
새로운 행동을 탐색하는 것 사이의 균형을 다룹니다.

- Exploration
- Exploitation
- Epsilon-Greedy
- Entropy
- Intrinsic Reward

### Self-Play

에이전트가 자신 또는 다른 에이전트와 반복적으로 대결하며
학습 경험을 스스로 생성하는 방법을 정리합니다.

```text
       Agent A
          │
          ▼
    Game / Environment
          │
          ▼
       Agent B
          │
          ▼
        Reward
          │
          ▼
    Policy Update
          │
          └──────→ 반복
```

Self-Play는 게임 AI와 같은 환경에서
사람이 직접 모든 정답 데이터를 제공하지 않고도
대량의 학습 경험을 생성할 수 있다는 특징이 있습니다.

### Game AI

강화학습을 게임 AI에 적용하는 방법을 정리합니다.

```text
Game State
    │
    ▼
State Representation
    │
    ▼
Policy / Neural Network
    │
    ▼
Action
    │
    ▼
Game API
    │
    ▼
Game
    │
    └──────→ Next State
```

게임의 상태를 관찰하고 학습된 Policy를 통해 행동을 선택한 뒤,
게임 API를 통해 실제 행동을 실행하는 구조를 다룹니다.

#### StarCraft AI

- BWAPI
- Game State
- State Representation
- Action Space
- Macro / Micro
- Self-Play
- Neural Network Policy
- Pluto

### Reinforcement Learning vs Supervised Learning

#### Supervised Learning

```text
Input
  │
  ▼
Model
  │
  ▼
Prediction
  │
  ▼
Ground Truth
  │
  ▼
Loss
  │
  ▼
Model Update
```

입력과 정답(Label)이 주어진 데이터를 이용하여 학습합니다.

#### Reinforcement Learning

```text
State
  │
  ▼
Policy
  │
  ▼
Action
  │
  ▼
Environment
  │
  ├── Next State
  └── Reward
         │
         ▼
    Policy Update
```

미리 정해진 정답 행동 대신
환경과의 상호작용에서 얻은 보상을 기반으로
더 나은 행동 정책을 학습합니다.

### Experiments

강화학습 알고리즘을 직접 구현하고 실험합니다.

예정:

- Q-Learning
- DQN
- PPO
- Gymnasium
- 간단한 Game AI
- Self-Play

### Applications

강화학습이 활용될 수 있는 다양한 분야를 정리합니다.

- Game AI
- Robotics
- Physical AI
- Autonomous Driving
- Drone
- Robot Manipulation
- Industrial Automation

### References

강화학습 관련 논문, 문서 및 참고 자료를 정리합니다.
