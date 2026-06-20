# 🏓 Pong (Unity)

Um clássico jogo de Pong feito em **Unity 6** com **C#**, criado do zero como projeto de aprendizado de desenvolvimento de jogos.

O jogo inteiro (câmera, raquetes, bola, placar, colisões e IA) é gerado por um único script: [`Assets/Scripts/PongGame.cs`](Assets/Scripts/PongGame.cs). Não usa nenhum asset externo — basta abrir e apertar Play.

## Como rodar

1. Abra o **Unity Hub**.
2. Clique em **Add → Add project from disk** e selecione a pasta `pong-unity`.
3. Abra o projeto (versão recomendada: **Unity 6000.0.43f1**, mas qualquer Unity 6 funciona).
4. Abra a cena `Assets/Scenes/Pong.unity` (ou simplesmente aperte Play em qualquer cena — o jogo se monta sozinho).
5. Aperte o botão **▶ Play**.

## Como jogar

Você enfrenta a CPU numa **progressão de 50 níveis** de dificuldade, começando no nível 1.

**Regras de cada partida:**
- Primeiro a fazer **5 pontos** vence; ou
- Após **1 minuto**, quem tiver o maior placar vence.
- **Empate no tempo:** repete o mesmo nível.
- **Você venceu:** avança para o próximo nível.
- **A CPU venceu:** você volta para o **nível 1**.

A IA fica progressivamente mais rápida e precisa conforme o nível sobe.

| Ação | Controle |
|------|----------|
| Mover raquete | **Mouse** (ou `W` / `S`) |
| Começar a próxima partida | `Espaço` |

## Ajustando o jogo

Abra [`PongGame.cs`](Assets/Scripts/PongGame.cs) e mexa nas constantes do topo:

```csharp
const float BallStartSpeed = 7f;     // velocidade da bola
const int   PointsToWin    = 5;      // pontos para vencer a partida
const float MatchTime      = 60f;    // duração máxima de cada partida (segundos)
const int   MaxLevel       = 50;     // total de níveis de dificuldade
const bool  TwoPlayer      = false;  // true = 2 jogadores (P2 usa as setas)
```

Mude `TwoPlayer` para `true` e jogue com um amigo: o Jogador 2 controla a raquete da direita com as **setas ↑ / ↓**.

## Como funciona (resumo técnico)

- **Renderização:** as raquetes e a bola são cubos achatados (`PrimitiveType.Cube`) vistos por uma câmera ortográfica — sem precisar de sprites.
- **Física:** colisão feita "na mão" (AABB), sem o motor de física, para o comportamento ser previsível e clássico.
- **Efeito da rebatida:** o ângulo da bola muda conforme o ponto da raquete onde ela bate.
- **Auto-inicialização:** `[RuntimeInitializeOnLoadMethod]` monta a cena automaticamente ao dar Play.

---
Feito como primeiro projeto de games 🎮
