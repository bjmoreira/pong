# 🏓 Pong (Unity 6 + C#)

**Idiomas / Languages / Idiomas:** [🇧🇷 Português](#-português) · [🇺🇸 English](#-english) · [🇪🇸 Español](#-español)

O jogo inteiro (câmera, raquetes, bola, placar, colisões e IA) é gerado por um único script: [`Assets/Scripts/PongGame.cs`](Assets/Scripts/PongGame.cs). Sem assets externos — é só abrir e apertar **Play**.

---

## 🇧🇷 Português

### Descrição
Um clássico **Pong** feito em **Unity 6** com **C#**, criado do zero como projeto de aprendizado. Você enfrenta a CPU numa **progressão de 50 níveis**, cada um mais difícil que o anterior.

### Como rodar
1. Abra o **Unity Hub** → **Add → Add project from disk** e selecione a pasta `pong-unity`.
2. Abra o projeto (recomendado: **Unity 6000.0.43f1**).
3. Abra a cena `Assets/Scenes/Pong.unity` (ou aperte Play em qualquer cena — o jogo se monta sozinho).
4. Aperte o botão **▶ Play**.

### Comandos (controles)
| Ação | Comando |
|------|---------|
| Mover a raquete | **Mouse** (move o cursor para cima/baixo) |
| Mover a raquete (alternativa) | Teclas `W` / `S` |
| Avançar de nível | **Automático** (contagem regressiva 3-2-1) |

### Regras
- 🏆 Vence a partida quem fizer **5 pontos** primeiro.
- ⏱️ Cada partida dura no máximo **1 minuto**; se ninguém chegar a 5, vence quem tiver o **maior placar**.
- 🤝 **Empate** quando o tempo acaba: **repete o mesmo nível**.
- ⬆️ **Você venceu:** avança para o **próximo nível**.
- ⬇️ **A CPU venceu:** você **volta para o nível 1**.
- 🎯 São **50 níveis**: a IA fica cada vez mais rápida e precisa.

### Ajustes (no topo de `PongGame.cs`)
```csharp
const int   PointsToWin = 5;     // pontos para vencer a partida
const float MatchTime   = 60f;   // duração máxima de cada partida (segundos)
const int   MaxLevel    = 50;    // total de níveis
const bool  TwoPlayer   = false; // true = 2 jogadores (P2 usa as setas ↑/↓)
```

---

## 🇺🇸 English

### Description
A classic **Pong** built in **Unity 6** with **C#**, made from scratch as a learning project. You face the CPU through a **50-level progression**, each one harder than the last.

### How to run
1. Open **Unity Hub** → **Add → Add project from disk** and select the `pong-unity` folder.
2. Open the project (recommended: **Unity 6000.0.43f1**).
3. Open the scene `Assets/Scenes/Pong.unity` (or hit Play in any scene — the game builds itself).
4. Press the **▶ Play** button.

### Controls
| Action | Control |
|--------|---------|
| Move the paddle | **Mouse** (move the cursor up/down) |
| Move the paddle (alternative) | `W` / `S` keys |
| Advance to next level | **Automatic** (3-2-1 countdown) |

### Rules
- 🏆 First to score **5 points** wins the match.
- ⏱️ Each match lasts at most **1 minute**; if no one reaches 5, the **higher score** wins.
- 🤝 **Tie** when time runs out: **replay the same level**.
- ⬆️ **You win:** advance to the **next level**.
- ⬇️ **The CPU wins:** you go **back to level 1**.
- 🎯 There are **50 levels**: the AI gets faster and more accurate each time.

### Tuning (top of `PongGame.cs`)
```csharp
const int   PointsToWin = 5;     // points to win a match
const float MatchTime   = 60f;   // max duration of each match (seconds)
const int   MaxLevel    = 50;    // total number of levels
const bool  TwoPlayer   = false; // true = 2 players (P2 uses arrow keys ↑/↓)
```

---

## 🇪🇸 Español

### Descripción
Un clásico **Pong** hecho en **Unity 6** con **C#**, creado desde cero como proyecto de aprendizaje. Te enfrentas a la CPU en una **progresión de 50 niveles**, cada uno más difícil que el anterior.

### Cómo ejecutar
1. Abre **Unity Hub** → **Add → Add project from disk** y selecciona la carpeta `pong-unity`.
2. Abre el proyecto (recomendado: **Unity 6000.0.43f1**).
3. Abre la escena `Assets/Scenes/Pong.unity` (o pulsa Play en cualquier escena — el juego se construye solo).
4. Pulsa el botón **▶ Play**.

### Controles
| Acción | Control |
|--------|---------|
| Mover la pala | **Ratón** (mueve el cursor arriba/abajo) |
| Mover la pala (alternativa) | Teclas `W` / `S` |
| Avanzar de nivel | **Automático** (cuenta atrás 3-2-1) |

### Reglas
- 🏆 Gana la partida quien anote **5 puntos** primero.
- ⏱️ Cada partida dura como máximo **1 minuto**; si nadie llega a 5, gana quien tenga la **mayor puntuación**.
- 🤝 **Empate** al acabar el tiempo: **se repite el mismo nivel**.
- ⬆️ **Ganaste:** avanzas al **siguiente nivel**.
- ⬇️ **Ganó la CPU:** vuelves al **nivel 1**.
- 🎯 Hay **50 niveles**: la IA se vuelve más rápida y precisa cada vez.

### Ajustes (al inicio de `PongGame.cs`)
```csharp
const int   PointsToWin = 5;     // puntos para ganar la partida
const float MatchTime   = 60f;   // duración máxima de cada partida (segundos)
const int   MaxLevel    = 50;    // total de niveles
const bool  TwoPlayer   = false; // true = 2 jugadores (P2 usa las flechas ↑/↓)
```
