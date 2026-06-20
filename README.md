# 🏓 Pong (Unity)

Um clássico jogo de Pong feito em **Unity 6** com **C#**, criado do zero como projeto de aprendizado de desenvolvimento de jogos.

O jogo inteiro (câmera, raquetes, bola, placar, colisões e IA) é gerado por um único script: [`Assets/Scripts/PongGame.cs`](Assets/Scripts/PongGame.cs). Não usa nenhum asset externo — basta abrir e apertar Play.

## Como rodar

1. Abra o **Unity Hub**.
2. Clique em **Add → Add project from disk** e selecione a pasta `pong-unity`.
3. Abra o projeto (versão recomendada: **Unity 6000.0.43f1**, mas qualquer Unity 6 funciona).
4. Abra a cena `Assets/Scenes/Pong.unity` (ou simplesmente aperte Play em qualquer cena — o jogo se monta sozinho).
5. Aperte o botão **▶ Play**.

## Controles

| Ação | Tecla |
|------|-------|
| Mover raquete para cima | `W` |
| Mover raquete para baixo | `S` |
| Jogar de novo (após fim de jogo) | `Espaço` |

Por padrão você joga contra uma **CPU**. O primeiro a fazer **5 pontos** vence.

## Ajustando o jogo

Abra [`PongGame.cs`](Assets/Scripts/PongGame.cs) e mexa nas constantes do topo:

```csharp
const float BallStartSpeed = 7f;     // velocidade da bola
const int   ScoreToWin     = 5;      // pontos para vencer
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
