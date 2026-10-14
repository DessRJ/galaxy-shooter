using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    public static int Vivos;

    public int tipo = 1;
    public int direccion = -1;

    int salud;
    float velocidad, x0, nacio, siguienteDisparo;
    SpriteRenderer sr;

    void Awake() { Vivos++; }
    void OnDestroy() { Vivos--; }

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        salud = GameManagerScript.DisparosEnemigo;
        x0 = transform.position.x;
        nacio = Time.time;
        velocidad = tipo == 1 ? 1.6f : 2.2f;
        siguienteDisparo = Time.time + Random.Range(1f, 2f);

        CircleCollider2D col = GetComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = sr.sprite.bounds.extents.x * 0.7f;
    }

    void Update()
    {
        float t = Time.time - nacio;
        Vector3 p = transform.position;
        p.y += direccion * velocidad * Time.deltaTime;

        if (tipo == 1)
            p.x = x0 + Mathf.PingPong(t * 1.8f + 2.5f, 5f) - 2.5f;
        else
            p.x = x0 + Mathf.Sin(t * 2.2f) * 3f;

        float tope = GameManagerScript.limiteX - 0.8f;
        p.x = Mathf.Clamp(p.x, -tope, tope);
        transform.position = p;

        if (!GameManagerScript.gameOver && Mathf.Abs(p.y) < GameManagerScript.limiteY - 0.3f && Time.time >= siguienteDisparo)
        {
            Disparar();
            siguienteDisparo = Time.time + Random.Range(2f, 3.5f);
        }

        float limite = GameManagerScript.limiteY + 2f;
        if ((direccion < 0 && p.y < -limite) || (direccion > 0 && p.y > limite))
            Destroy(gameObject);
    }

    void Disparar()
    {
        GameAssets a = GameAssets.Instance;
        if (tipo == 1)
        {
            EnemyBulletScript.Crear(transform.position, new Vector2(0, direccion * 5f), a.balaEnemigo1, 0.55f);
        }
        else
        {
            Vector2 hacia = (Vector2)(PlayerScript.Instance.transform.position - transform.position);
            EnemyBulletScript.Crear(transform.position, hacia.normalized * 4.5f, a.balaEnemigo2, 0.45f);
        }
    }

    public void RecibirDisparo()
    {
        salud--;
        if (salud <= 0)
        {
            GameManagerScript.SumarPuntos(GameManagerScript.PuntosEnemigo);
            Explotar();
            return;
        }

        SoundScript.Instance.PlayImpacto();

        float t = (float)salud / GameManagerScript.DisparosEnemigo;
        sr.color = Color.Lerp(new Color(1f, 0.25f, 0.25f), Color.white, t);
    }

    public void Explotar()
    {
        Instantiate(GameAssets.Instance.explosion, transform.position, Quaternion.identity);
        SoundScript.Instance.PlaySonidoExplosion();
        Destroy(gameObject);
    }
}
