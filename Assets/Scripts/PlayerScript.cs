using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    public static PlayerScript Instance;

    public GameObject bullet;
    public SoundScript soundScript;

    public float velocidad = 6f;
    public float cadencia = 0.22f;
    public float anguloMultiple = 15f;

    Rigidbody2D rb;
    SpriteRenderer sr;
    Vector2 movimiento;
    float siguienteDisparo;
    float rojoHasta;
    bool ratonMovido;
    Vector3 ratonInicial;

    void Awake()
    {
        Instance = this;
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        rb.useFullKinematicContacts = true;

        CircleCollider2D col = GetComponent<CircleCollider2D>();
        if (col == null) col = gameObject.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = sr.sprite.bounds.extents.x * 0.6f;
    }

    void Start()
    {
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        ratonInicial = Input.mousePosition;
    }

    public void Preparar()
    {
        gameObject.SetActive(true);
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        ratonMovido = false;
        ratonInicial = Input.mousePosition;
        movimiento = Vector2.zero;
        siguienteDisparo = Time.time + 0.3f;
        rojoHasta = 0f;
        sr.color = Color.white;
    }

    void Update()
    {
        if (!GameManagerScript.EnJuego) return;

        if (!ratonMovido && (Input.mousePosition - ratonInicial).sqrMagnitude > 4f) ratonMovido = true;
        if (ratonMovido && GameManagerScript.Cam != null)
        {
            Vector3 mouse = GameManagerScript.Cam.ScreenToWorldPoint(Input.mousePosition);
            Vector2 dir = mouse - transform.position;
            float angulo = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0, 0, angulo);
        }

        float x = 0, y = 0;
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) x += 1;
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) x -= 1;
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) y += 1;
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) y -= 1;
        movimiento = new Vector2(x, y).normalized * velocidad;

        if ((Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0)) && Time.time >= siguienteDisparo)
        {
            Disparar();
            siguienteDisparo = Time.time + cadencia;
        }

        sr.color = Time.time < rojoHasta ? new Color(1f, 0.3f, 0.3f) : Color.white;
    }

    void FixedUpdate()
    {
        if (!GameManagerScript.EnJuego) return;

        Vector2 destino = rb.position + movimiento * Time.fixedDeltaTime;
        destino.x = Mathf.Clamp(destino.x, -GameManagerScript.limiteX + 0.6f, GameManagerScript.limiteX - 0.6f);
        destino.y = Mathf.Clamp(destino.y, -GameManagerScript.limiteY + 0.6f, GameManagerScript.limiteY - 0.6f);
        rb.MovePosition(destino);
    }

    void Disparar()
    {
        GameAssets a = GameAssets.Instance;
        Vector3 origen = transform.position + transform.up * 0.7f;

        switch (GameManagerScript.arma)
        {
            case TipoArma.Normal:
                BulletScript.Crear(origen, transform.rotation, 0f, a.balaNormal, 0.5f);
                SoundScript.Instance.PlaySonidoDisparo();
                break;

            case TipoArma.Multiple:
                BulletScript.Crear(origen, transform.rotation, -anguloMultiple, a.balaNormal, 0.5f);
                BulletScript.Crear(origen, transform.rotation, 0f, a.balaNormal, 0.5f);
                BulletScript.Crear(origen, transform.rotation, anguloMultiple, a.balaNormal, 0.5f);
                SoundScript.Instance.PlaySonidoDisparo();
                break;

            case TipoArma.Adicional:
                BulletScript.Crear(origen, transform.rotation, 0f, a.balaAdicional, 0.9f);
                SoundScript.Instance.PlayDisparoAdicional();
                break;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!GameManagerScript.EnJuego) return;

        PowerupScript powerup = other.GetComponent<PowerupScript>();
        if (powerup != null)
        {
            powerup.Aplicar();
            SoundScript.Instance.PlayPowerup();
            Destroy(powerup.gameObject);
            return;
        }

        AsteroidScript asteroide = other.GetComponent<AsteroidScript>();
        if (asteroide != null) { asteroide.Explotar(); RecibirDanio(); return; }

        EnemyScript enemigo = other.GetComponent<EnemyScript>();
        if (enemigo != null) { enemigo.Explotar(); RecibirDanio(); return; }

        EnemyBulletScript balaEnemiga = other.GetComponent<EnemyBulletScript>();
        if (balaEnemiga != null) { Destroy(balaEnemiga.gameObject); RecibirDanio(); }
    }

    void RecibirDanio()
    {
        GameManagerScript.vidas--;
        GameManagerScript.arma = TipoArma.Normal;
        SoundScript.Instance.PlayGolpeNave();
        rojoHasta = Time.time + 0.25f;

        if (GameManagerScript.vidas <= 0)
        {
            Instantiate(GameAssets.Instance.explosion, transform.position, Quaternion.identity);
            SoundScript.Instance.PlaySonidoExplosion();
            GameManagerScript.TerminarJuego();
            gameObject.SetActive(false);
        }
    }
}
