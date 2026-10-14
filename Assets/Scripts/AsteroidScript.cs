using UnityEngine;

public class AsteroidScript : MonoBehaviour
{
    public int direccion = -1;

    int salud;
    float velocidad, velocidadRotacion, derivaX;
    SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        salud = GameManagerScript.DisparosAsteroide;

        float escala = Random.Range(.08f, .3f);
        transform.localScale = new Vector3(escala, escala, 1f);
        velocidad = Random.Range(1.5f, 4f);
        derivaX = Random.Range(-0.6f, 0.6f);
        velocidadRotacion = Random.Range(-120f, 120f);

        CircleCollider2D col = GetComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = sr.sprite.bounds.extents.x * 0.75f;
    }

    void Update()
    {
        transform.position += new Vector3(derivaX, direccion * velocidad, 0f) * Time.deltaTime;
        transform.Rotate(0, 0, velocidadRotacion * Time.deltaTime);

        float y = transform.position.y;
        float limite = GameManagerScript.limiteY + 3f;
        if ((direccion < 0 && y < -limite) || (direccion > 0 && y > limite))
            Destroy(gameObject);
    }

    public void RecibirDisparo()
    {
        salud--;
        if (salud <= 0)
        {
            GameManagerScript.SumarPuntos(GameManagerScript.PuntosAsteroide);
            Explotar();
            return;
        }

        SoundScript.Instance.PlayImpacto();

        Sprite[] etapas = GameAssets.Instance.asteroide;
        int etapa = Mathf.Clamp(GameManagerScript.DisparosAsteroide - salud, 0, etapas.Length - 1);
        sr.sprite = etapas[etapa];
    }

    public void Explotar()
    {
        Instantiate(GameAssets.Instance.explosion, transform.position, Quaternion.identity);
        SoundScript.Instance.PlaySonidoExplosion();
        Destroy(gameObject);
    }
}
