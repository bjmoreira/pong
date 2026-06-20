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
    const int   PointsToWin   = 5;      // chegar a 5 pontos vence a partida
    const float MatchTime     = 60f;    // cada partida dura no maximo 60 segundos
    const int   MaxLevel      = 50;     // total de niveis de dificuldade
    const bool  TwoPlayer     = false;  // false = voce vs CPU | true = 2 jogadores

    // ---- Progressao ----
    const float TransitionTime = 3f;  // segundos de contagem regressiva entre niveis
    int   level = 1;        // nivel atual (1 a 50): sobe ao vencer, zera ao perder
    float timeLeft;         // segundos restantes na partida atual
    float transitionTimer;  // > 0 = mostrando a contagem antes do proximo nivel
    string banner = "";     // mensagem grande exibida durante a contagem

    // ---- Estado interno ----
    Camera cam;
    Transform left, right, ball;
    Vector2 ballVel;
    float halfWidth, halfHeight, paddleX;
    int scoreLeft, scoreRight;

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

        StartMatch();
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
        // Entre niveis: roda a contagem regressiva e libera o proximo sozinho.
        if (transitionTimer > 0f)
        {
            transitionTimer -= Time.deltaTime;
            if (transitionTimer <= 0f) StartMatch();
            return;   // jogo pausado durante a contagem
        }

        // Cronometro: quando zera, quem tiver mais pontos vence
        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0f)
        {
            timeLeft = 0f;
            DecideByTime();
            return;
        }

        // Raquete da esquerda: segue o mouse (ou teclas W / S)
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S))
        {
            float lMove = 0;
            if (Input.GetKey(KeyCode.W)) lMove += 1;
            if (Input.GetKey(KeyCode.S)) lMove -= 1;
            MovePaddle(left, lMove);
        }
        else
        {
            // Converte a posicao do mouse na tela para o mundo e segue o eixo Y
            float mouseWorldY = cam.ScreenToWorldPoint(
                new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10f)).y;
            SetPaddleY(left, mouseWorldY);
        }

        // Raquete da direita: setas (2 jogadores) ou IA com dificuldade
        if (TwoPlayer)
        {
            float rMove = 0;
            if (Input.GetKey(KeyCode.UpArrow))   rMove += 1;
            if (Input.GetKey(KeyCode.DownArrow)) rMove -= 1;
            MovePaddle(right, rMove);
        }
        else
        {
            UpdateAI();
        }

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
        if (p.x < -halfWidth)      { scoreRight++; CheckMatchPoint(); ResetBall(1);  }
        else if (p.x > halfWidth)  { scoreLeft++;  CheckMatchPoint(); ResetBall(-1); }
    }

    void MovePaddle(Transform t, float dir)
    {
        Vector3 pos = t.position;
        pos.y += dir * PaddleSpeed * Time.deltaTime;
        float limit = halfHeight - PaddleHeight / 2f;
        pos.y = Mathf.Clamp(pos.y, -limit, limit);
        t.position = pos;
    }

    // Posiciona a raquete diretamente numa altura (usado pelo controle do mouse).
    void SetPaddleY(Transform t, float y)
    {
        float limit = halfHeight - PaddleHeight / 2f;
        Vector3 pos = t.position;
        pos.y = Mathf.Clamp(y, -limit, limit);
        t.position = pos;
    }

    // Inteligencia artificial da raquete da direita.
    // A forca dela escala com o nivel (1 = facil, 50 = quase perfeita) atraves
    // de 3 valores: velocidade, "zona morta" e se so reage com a bola vindo.
    void UpdateAI()
    {
        // t vai de 0 (nivel 1) ate 1 (nivel 50). Tudo na IA escala com ele.
        float t = (level - 1) / (float)(MaxLevel - 1);

        // Mathf.Lerp(a, b, t) mistura de 'a' (t=0) ate 'b' (t=1).
        float maxSpeed = Mathf.Lerp(PaddleSpeed * 0.35f, PaddleSpeed * 1.10f, t); // lenta -> mais rapida que voce
        float deadzone = Mathf.Lerp(1.30f, 0.04f, t);                            // preguicosa -> precisa
        bool reactOnlyWhenApproaching = t < 0.5f;                                // ate o nivel ~25 ela relaxa

        // Decide para onde a IA quer ir:
        // - se a bola vem na direcao dela, persegue a altura da bola
        // - senao, relaxa e volta devagar para o centro
        float targetY;
        if (!reactOnlyWhenApproaching || ballVel.x > 0)
            targetY = ball.position.y;
        else
            targetY = 0f;

        float diff = targetY - right.position.y;

        // Perto o bastante do alvo? Entao nao mexe (e isso que a deixa "burra"
        // e permite que voce ganhe pontos no canto).
        if (Mathf.Abs(diff) <= deadzone) return;

        // Move em direcao ao alvo, sem ultrapassar e respeitando a velocidade maxima.
        float step = Mathf.Sign(diff) * maxSpeed * Time.deltaTime;
        if (Mathf.Abs(step) > Mathf.Abs(diff)) step = diff;

        float limit = halfHeight - PaddleHeight / 2f;
        Vector3 pos = right.position;
        pos.y = Mathf.Clamp(pos.y + step, -limit, limit);
        right.position = pos;
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

    // Comeca uma nova partida: zera placar, reinicia cronometro e centraliza tudo.
    void StartMatch()
    {
        scoreLeft = scoreRight = 0;
        timeLeft = MatchTime;
        transitionTimer = 0f;
        ball.gameObject.SetActive(true);                 // mostra a bola de volta
        left.position  = new Vector3(-paddleX, 0, 0);
        right.position = new Vector3(paddleX, 0, 0);
        ResetBall(Random.value < 0.5f ? 1 : -1);
    }

    // Chamado a cada ponto: alguem chegou a 5? Entao a partida acabou.
    void CheckMatchPoint()
    {
        if (scoreLeft >= PointsToWin)       PlayerWins();
        else if (scoreRight >= PointsToWin) PlayerLoses();
    }

    // Tempo esgotado: quem tiver mais pontos vence; empate repete o mesmo nivel.
    void DecideByTime()
    {
        if (scoreLeft > scoreRight)      PlayerWins();
        else if (scoreRight > scoreLeft) PlayerLoses();
        else BeginTransition("TEMPO ESGOTADO - EMPATE!");
    }

    // Jogador venceu: avanca de nivel (ou zera o jogo ao passar do nivel 50).
    void PlayerWins()
    {
        if (level >= MaxLevel)
            BeginTransition("VOCE ZEROU OS " + MaxLevel + " NIVEIS!");
        else
        {
            level++;
            BeginTransition("VOCE VENCEU!");
        }
    }

    // Jogador perdeu para a maquina: volta para o nivel 1.
    void PlayerLoses()
    {
        level = 1;
        BeginTransition("A CPU VENCEU!");
    }

    // Inicia a contagem regressiva; ao zerar, StartMatch() comeca o proximo nivel.
    void BeginTransition(string msg)
    {
        banner = msg;
        transitionTimer = TransitionTime;
        ball.gameObject.SetActive(false);                // esconde a bola na contagem
    }

    // Desenha placar, linha do meio e mensagens na tela.
    void OnGUI()
    {
        // Linha pontilhada no centro (comeca abaixo do placar)
        GUI.color = new Color(1, 1, 1, 0.15f);
        float cx = Screen.width / 2f - 2;
        for (float y = 130; y < Screen.height; y += 40)
            GUI.DrawTexture(new Rect(cx, y, 4, 24), Texture2D.whiteTexture);
        GUI.color = Color.white;

        // ---- Placar do topo ----
        // Nivel atual no CANTO SUPERIOR ESQUERDO (fonte 4x maior)
        var lvl = new GUIStyle { fontSize = 96, alignment = TextAnchor.UpperLeft, fontStyle = FontStyle.Bold };
        lvl.normal.textColor = new Color(0.55f, 0.85f, 1f);
        GUI.Label(new Rect(16, 6, 640, 130), "Level " + level, lvl);
        var lvlSub = new GUIStyle { fontSize = 24, alignment = TextAnchor.UpperLeft };
        lvlSub.normal.textColor = new Color(1, 1, 1, 0.4f);
        GUI.Label(new Rect(22, 124, 300, 32), "de " + MaxLevel, lvlSub);

        // Pontuacao grande:  3   -   1
        var big = new GUIStyle { fontSize = 44, alignment = TextAnchor.UpperCenter };
        big.normal.textColor = Color.white;
        GUI.Label(new Rect(0, 30, Screen.width, 56), scoreLeft + "    -    " + scoreRight, big);

        // Rotulos dos lados
        var side = new GUIStyle { fontSize = 16, alignment = TextAnchor.UpperCenter };
        side.normal.textColor = new Color(1, 1, 1, 0.6f);
        GUI.Label(new Rect(Screen.width / 2f - 175, 46, 110, 24), "VOCE", side);
        GUI.Label(new Rect(Screen.width / 2f + 65,  46, 110, 24), "CPU",  side);

        // Cronometro (fica vermelho nos ultimos 10 segundos)
        var clock = new GUIStyle { fontSize = 22, alignment = TextAnchor.UpperCenter };
        clock.normal.textColor = timeLeft <= 10f ? new Color(1f, 0.45f, 0.45f) : Color.white;
        GUI.Label(new Rect(0, 88, Screen.width, 28), Mathf.CeilToInt(timeLeft) + "s", clock);

        // Dica de controles no rodape
        var hint = new GUIStyle { fontSize = 15, alignment = TextAnchor.LowerCenter };
        hint.normal.textColor = new Color(1, 1, 1, 0.5f);
        GUI.Label(new Rect(0, Screen.height - 30, Screen.width, 22),
                  (TwoPlayer ? "P1: mouse ou W / S    P2: setas" : "Mova com o MOUSE (ou W / S)")
                  + "   -   primeiro a " + PointsToWin + " ou maior placar em " + (int)MatchTime + "s vence", hint);

        // Contagem regressiva entre os niveis (3, 2, 1) e libera o proximo sozinho
        if (transitionTimer > 0f)
        {
            var title = new GUIStyle { fontSize = 38, alignment = TextAnchor.MiddleCenter, wordWrap = true, fontStyle = FontStyle.Bold };
            title.normal.textColor = Color.yellow;
            GUI.Label(new Rect(0, Screen.height / 2f - 160, Screen.width, 70), banner, title);

            // Numero GIGANTE da contagem
            var count = new GUIStyle { fontSize = 140, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            count.normal.textColor = Color.white;
            GUI.Label(new Rect(0, Screen.height / 2f - 100, Screen.width, 190),
                      Mathf.CeilToInt(transitionTimer).ToString(), count);

            // Proximo nivel a comecar
            var next = new GUIStyle { fontSize = 28, alignment = TextAnchor.MiddleCenter };
            next.normal.textColor = new Color(0.6f, 0.85f, 1f);
            GUI.Label(new Rect(0, Screen.height / 2f + 110, Screen.width, 40), "Level " + level, next);
        }
    }
}
