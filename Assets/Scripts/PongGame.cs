using UnityEngine;

/// <summary>
/// Pong completo em um unico script, sem precisar de assets.
/// Ele se inicializa sozinho ao dar Play (cria camera, raquetes, bola e placar).
/// Basta abrir o projeto no Unity e apertar o botao Play.
/// </summary>
public class PongGame : MonoBehaviour
{
    // ---- Configuracoes (mude estes valores para ajustar o jogo) ----
    const float OrthoSize     = 5f;     // metade da altura visivel da tela
    const float PaddleHeight  = 2f;     // tamanho da raquete
    const float PaddleWidth   = 0.3f;
    const float PaddleSpeed   = 10f;    // velocidade das raquetes
    const float BallSize      = 0.3f;
    const float BallStartSpeed = 7f;    // velocidade inicial da bola
    const float BallSpeedup   = 0.35f;  // quanto a bola acelera a cada rebatida
    const int   ScoreToWin    = 5;      // pontos para vencer
    const bool  TwoPlayer     = false;  // false = voce vs CPU | true = 2 jogadores

    // ---- Estado interno ----
    Camera cam;
    Transform left, right, ball;
    Vector2 ballVel;
    float halfWidth, halfHeight, paddleX;
    int scoreLeft, scoreRight;
    bool gameOver;
    string message = "";

    // Faz o jogo se montar sozinho assim que a cena carrega,
    // mesmo numa cena vazia. Nao precisa arrastar nada no editor.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if (FindObjectOfType<PongGame>() == null)
            new GameObject("PongGame").AddComponent<PongGame>();
    }

    void Start()
    {
        // --- Camera ---
        cam = Camera.main;
        if (cam == null)
        {
            var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
            cam = camGo.AddComponent<Camera>();
        }
        cam.orthographic = true;
        cam.orthographicSize = OrthoSize;
        cam.transform.position = new Vector3(0, 0, -10);
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.05f, 0.06f, 0.09f);

        halfHeight = OrthoSize;
        halfWidth  = OrthoSize * cam.aspect;
        paddleX    = halfWidth - 0.7f;

        // --- Objetos do jogo ---
        left  = MakeBlock("LeftPaddle",  PaddleWidth, PaddleHeight, Color.white);
        left.position  = new Vector3(-paddleX, 0, 0);

        right = MakeBlock("RightPaddle", PaddleWidth, PaddleHeight, Color.white);
        right.position = new Vector3(paddleX, 0, 0);

        ball  = MakeBlock("Ball", BallSize, BallSize, Color.white);

        ResetBall(Random.value < 0.5f ? 1 : -1);
    }

    // Cria um retangulo (cubo achatado) com material que aparece em qualquer
    // pipeline de renderizacao (Built-in ou URP).
    Transform MakeBlock(string name, float w, float h, Color color)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        Destroy(go.GetComponent<Collider>());            // colisao e feita na mao
        go.transform.localScale = new Vector3(w, h, 1f);

        Shader s = Shader.Find("Unlit/Color");
        if (s == null) s = Shader.Find("Universal Render Pipeline/Unlit");
        if (s == null) s = Shader.Find("Sprites/Default");
        var mat = new Material(s);
        if (mat.HasProperty("_Color"))     mat.color = color;
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        go.GetComponent<Renderer>().material = mat;

        return go.transform;
    }

    void ResetBall(int dir)
    {
        ball.position = Vector3.zero;
        float vy = Random.Range(-0.5f, 0.5f);
        ballVel = new Vector2(dir, vy).normalized * BallStartSpeed;
    }

    void Update()
    {
        if (gameOver)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                scoreLeft = scoreRight = 0;
                gameOver = false;
                ResetBall(Random.value < 0.5f ? 1 : -1);
            }
            return;
        }

        // Raquete da esquerda: teclas W / S
        float lMove = 0;
        if (Input.GetKey(KeyCode.W)) lMove += 1;
        if (Input.GetKey(KeyCode.S)) lMove -= 1;
        MovePaddle(left, lMove);

        // Raquete da direita: setas (2 jogadores) ou IA simples
        float rMove;
        if (TwoPlayer)
        {
            rMove = 0;
            if (Input.GetKey(KeyCode.UpArrow))   rMove += 1;
            if (Input.GetKey(KeyCode.DownArrow)) rMove -= 1;
        }
        else
        {
            // IA: persegue a bola, mas limitada para ser vencivel
            float diff = ball.position.y - right.position.y;
            rMove = Mathf.Clamp(diff * 1.8f, -0.85f, 0.85f);
        }
        MovePaddle(right, rMove);

        // Move a bola
        Vector3 p = ball.position;
        p.x += ballVel.x * Time.deltaTime;
        p.y += ballVel.y * Time.deltaTime;

        // Quica no teto e no chao
        float ballHalf = BallSize / 2f;
        if (p.y > halfHeight - ballHalf)  { p.y = halfHeight - ballHalf;  ballVel.y = -Mathf.Abs(ballVel.y); }
        if (p.y < -halfHeight + ballHalf) { p.y = -halfHeight + ballHalf; ballVel.y =  Mathf.Abs(ballVel.y); }

        // Colisao com as raquetes
        CheckPaddle(left,  ref p, +1);
        CheckPaddle(right, ref p, -1);

        ball.position = p;

        // Ponto: bola passou de um dos lados
        if (p.x < -halfWidth)      { scoreRight++; CheckWin(); ResetBall(1);  }
        else if (p.x > halfWidth)  { scoreLeft++;  CheckWin(); ResetBall(-1); }
    }

    void MovePaddle(Transform t, float dir)
    {
        Vector3 pos = t.position;
        pos.y += dir * PaddleSpeed * Time.deltaTime;
        float limit = halfHeight - PaddleHeight / 2f;
        pos.y = Mathf.Clamp(pos.y, -limit, limit);
        t.position = pos;
    }

    // Rebate a bola se ela encostar na raquete e estiver indo em direcao a ela.
    void CheckPaddle(Transform paddle, ref Vector3 p, int reflectDir)
    {
        float ballHalf = BallSize / 2f;
        float pw = PaddleWidth / 2f;
        float ph = PaddleHeight / 2f;
        Vector3 pp = paddle.position;

        bool overlapX = Mathf.Abs(p.x - pp.x) <= pw + ballHalf;
        bool overlapY = Mathf.Abs(p.y - pp.y) <= ph + ballHalf;
        if (!overlapX || !overlapY) return;

        bool indoParaPaddle = (reflectDir > 0 && ballVel.x < 0) ||
                              (reflectDir < 0 && ballVel.x > 0);
        if (!indoParaPaddle) return;

        // Onde bateu na raquete muda o angulo (efeito classico do Pong)
        float offset = (p.y - pp.y) / ph;            // -1 (base) a +1 (topo)
        float speed = ballVel.magnitude + BallSpeedup;
        ballVel = new Vector2(reflectDir, offset).normalized * speed;

        // Empurra a bola para fora da raquete para nao "grudar"
        p.x = pp.x + reflectDir * (pw + ballHalf);
    }

    void CheckWin()
    {
        if (scoreLeft >= ScoreToWin)
        {
            gameOver = true;
            message = TwoPlayer ? "Jogador 1 venceu!" : "Voce venceu!";
        }
        else if (scoreRight >= ScoreToWin)
        {
            gameOver = true;
            message = TwoPlayer ? "Jogador 2 venceu!" : "A CPU venceu!";
        }
    }

    // Desenha placar, linha do meio e mensagens na tela.
    void OnGUI()
    {
        // Linha pontilhada no centro
        GUI.color = new Color(1, 1, 1, 0.15f);
        float cx = Screen.width / 2f - 2;
        for (float y = 10; y < Screen.height; y += 40)
            GUI.DrawTexture(new Rect(cx, y, 4, 24), Texture2D.whiteTexture);
        GUI.color = Color.white;

        // Placar
        var score = new GUIStyle { fontSize = 48, alignment = TextAnchor.UpperCenter };
        score.normal.textColor = Color.white;
        GUI.Label(new Rect(Screen.width / 2f - 200, 20, 150, 60), scoreLeft.ToString(),  score);
        GUI.Label(new Rect(Screen.width / 2f + 50,  20, 150, 60), scoreRight.ToString(), score);

        // Dica de controles
        var hint = new GUIStyle { fontSize = 16, alignment = TextAnchor.LowerCenter };
        hint.normal.textColor = new Color(1, 1, 1, 0.5f);
        GUI.Label(new Rect(0, Screen.height - 36, Screen.width, 24),
                  TwoPlayer ? "P1: W / S      P2: setas Cima / Baixo"
                            : "Mova a raquete com  W  e  S", hint);

        // Mensagem de vitoria
        if (gameOver)
        {
            var big = new GUIStyle { fontSize = 42, alignment = TextAnchor.MiddleCenter };
            big.normal.textColor = Color.yellow;
            GUI.Label(new Rect(0, Screen.height / 2f - 70, Screen.width, 60), message, big);

            var sub = new GUIStyle { fontSize = 22, alignment = TextAnchor.MiddleCenter };
            sub.normal.textColor = Color.white;
            GUI.Label(new Rect(0, Screen.height / 2f, Screen.width, 30),
                      "Pressione ESPACO para jogar de novo", sub);
        }
    }
}
