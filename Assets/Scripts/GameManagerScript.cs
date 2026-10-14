using UnityEngine;
using UnityEngine.SceneManagement;

public enum TipoArma { Normal, Multiple, Adicional }

public enum EstadoJuego { Menu, Cuenta, Jugando }

public class GameManagerScript : MonoBehaviour
{
    public const int MaxVidas = 5;
    public const int PuntosAsteroide = 2;
    public const int PuntosEnemigo = 5;
    public const int DisparosAsteroide = 3;
    public const int DisparosEnemigo = 5;
    public const int PuntajeMeta = 200;
    const string ClaveMax = "max";

    public static int score;
    public static int vidas;
    public static TipoArma arma;
    public static bool gameOver;
    public static bool metaAlcanzada;
    public static EstadoJuego estado;
    public static bool saltarMenu;
    static float inicioPartida;

    public static float limiteX, limiteY;

    float siguienteAsteroide, siguienteEnemigo, siguientePowerup;

    static Camera camara;
    public static Camera Cam
    {
        get
        {
            if (camara == null)
            {
                camara = Camera.main;
                if (camara == null) camara = FindFirstObjectByType<Camera>();
            }
            return camara;
        }
    }

    void Awake()
    {
        camara = null;
        Camera cam = Cam;
        if (cam == null) { Debug.LogError("No hay ninguna camara en la escena."); return; }
        limiteY = cam.orthographicSize;
        limiteX = limiteY * cam.aspect;
    }

    void Start()
    {
        score = 0;
        vidas = MaxVidas;
        arma = TipoArma.Normal;
        gameOver = false;
        metaAlcanzada = false;
        estado = EstadoJuego.Menu;
        Time.timeScale = 1f;

        if (!PlayerPrefs.HasKey(ClaveMax))
        {
            PlayerPrefs.SetInt(ClaveMax, 0);
        }

        if (GetComponent<HudScript>() == null) gameObject.AddComponent<HudScript>();
        if (GetComponent<MenuScript>() == null) gameObject.AddComponent<MenuScript>();
    }

    public static bool EnJuego
    {
        get { return estado == EstadoJuego.Jugando && !gameOver; }
    }

    public static void Comenzar()
    {
        score = 0;
        vidas = MaxVidas;
        arma = TipoArma.Normal;
        gameOver = false;
        metaAlcanzada = false;
        inicioPartida = Time.time;
        estado = EstadoJuego.Jugando;
        if (PlayerScript.Instance != null) PlayerScript.Instance.Preparar();
        GameManagerScript gm = FindFirstObjectByType<GameManagerScript>();
        if (gm != null) gm.ProgramarSpawns();
    }

    void ProgramarSpawns()
    {
        siguienteAsteroide = Time.time + 1f;
        siguienteEnemigo = Time.time + 5f;
        siguientePowerup = Time.time + 8f;
    }

    void Update()
    {
        if (Cam != null) limiteX = limiteY * Cam.aspect;

        if (estado != EstadoJuego.Jugando) return;

        if (gameOver)
        {
            if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Return)) Reiniciar(true);
            else if (Input.GetKeyDown(KeyCode.M) || Input.GetKeyDown(KeyCode.Escape)) Reiniciar(false);
            return;
        }

        float dificultad = Mathf.Clamp01(score / (float)PuntajeMeta);

        if (Time.time >= siguienteAsteroide)
        {
            SpawnAsteroide();
            siguienteAsteroide = Time.time + Mathf.Lerp(1.8f, 0.9f, dificultad);
        }

        if (Time.time >= siguienteEnemigo)
        {
            int maxEnemigos = dificultad < 0.5f ? 2 : 3;
            if (EnemyScript.Vivos < maxEnemigos) SpawnEnemigo();
            siguienteEnemigo = Time.time + Mathf.Lerp(6f, 3.5f, dificultad);
        }

        if (Time.time >= siguientePowerup)
        {
            if (PowerupScript.Activos < 2) SpawnPowerup();
            siguientePowerup = Time.time + Random.Range(9f, 14f);
        }
    }

    public static void SumarPuntos(int puntos)
    {
        score += puntos;

        if (score > PlayerPrefs.GetInt(ClaveMax, 0))
        {
            PlayerPrefs.SetInt(ClaveMax, score);
            PlayerPrefs.Save();
        }

        if (!metaAlcanzada && score >= PuntajeMeta)
        {
            metaAlcanzada = true;
            if (HudScript.Instance != null) HudScript.Instance.MostrarMeta();
        }
    }

    public static void TerminarJuego()
    {
        gameOver = true;
        RegistroPartidas.Guardar(score, Time.time - inicioPartida, metaAlcanzada);
        PlayerPrefs.Save();
        if (SoundScript.Instance != null)
        {
            SoundScript.Instance.BajarMusica();
            SoundScript.Instance.PlayGameOver();
        }
        if (HudScript.Instance != null) HudScript.Instance.MostrarGameOver();
    }

    void Reiniciar(bool directo)
    {
        saltarMenu = directo;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void OnApplicationQuit()
    {
        PlayerPrefs.Save();
    }

    void SpawnAsteroide()
    {
        bool arriba = Random.value < 0.5f;
        float x = Random.Range(-limiteX + 0.5f, limiteX - 0.5f);
        float y = (arriba ? 1 : -1) * (limiteY + 1.5f);

        GameObject go = new GameObject("Asteroid");
        go.transform.position = new Vector3(x, y, 0f);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = GameAssets.Instance.asteroide[0];
        sr.sortingOrder = 5;

        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0;
        rb.useFullKinematicContacts = true;

        go.AddComponent<CircleCollider2D>();
        go.AddComponent<AsteroidScript>().direccion = arriba ? -1 : 1;
    }

    void SpawnEnemigo()
    {
        int tipo = Random.value < 0.5f ? 1 : 2;
        bool arriba = Random.value < 0.5f;
        float x = Random.Range(-limiteX + 2f, limiteX - 2f);
        float y = (arriba ? 1 : -1) * (limiteY + 1.5f);

        GameObject go = new GameObject("Enemy" + tipo);
        go.transform.position = new Vector3(x, y, 0f);

        go.transform.rotation = Quaternion.Euler(0, 0, arriba ? 180f : 0f);
        go.transform.localScale = new Vector3(2.4f, 2.4f, 1f);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = tipo == 1 ? GameAssets.Instance.enemigo1 : GameAssets.Instance.enemigo2;
        sr.sortingOrder = 6;

        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0;
        rb.useFullKinematicContacts = true;

        go.AddComponent<CircleCollider2D>();
        EnemyScript enemigo = go.AddComponent<EnemyScript>();
        enemigo.tipo = tipo;
        enemigo.direccion = arriba ? -1 : 1;
    }

    void SpawnPowerup()
    {
        TipoPowerup tipo = (TipoPowerup)Random.Range(0, 3);
        if (tipo == TipoPowerup.Vida && vidas >= MaxVidas) tipo = TipoPowerup.Multiple;

        Vector2 pos = Vector2.zero;
        for (int i = 0; i < 10; i++)
        {
            pos = new Vector2(Random.Range(-limiteX + 1.5f, limiteX - 1.5f), Random.Range(-limiteY + 1.5f, limiteY - 1.5f));
            if (PlayerScript.Instance == null) break;
            if (Vector2.Distance(pos, PlayerScript.Instance.transform.position) > 2.5f) break;
        }

        GameAssets a = GameAssets.Instance;
        Sprite sprite = tipo == TipoPowerup.Multiple ? a.powerupMultiple : tipo == TipoPowerup.Adicional ? a.powerupAdicional : a.powerupVida;

        GameObject go = new GameObject("Powerup_" + tipo);
        go.transform.position = pos;
        go.transform.localScale = new Vector3(0.6f, 0.6f, 1f);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 4;

        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0;
        rb.useFullKinematicContacts = true;

        CircleCollider2D col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = sprite.bounds.extents.x * 0.8f;

        go.AddComponent<PowerupScript>().tipo = tipo;
    }
}
