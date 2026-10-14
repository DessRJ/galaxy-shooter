using UnityEngine;

public class BulletScript : MonoBehaviour
{
    public float velocidad = 10f;
    bool usada;

    public static void Crear(Vector3 posicion, Quaternion rotacionNave, float anguloExtra, Sprite sprite, float altura)
    {
        GameObject go = new GameObject("Bullet");
        go.transform.position = posicion;
        go.transform.rotation = rotacionNave * Quaternion.Euler(0, 0, anguloExtra);
        float escala = altura / sprite.bounds.size.y;
        go.transform.localScale = new Vector3(escala, escala, 1f);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 8;

        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0;
        rb.useFullKinematicContacts = true;

        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(sprite.bounds.size.x * 0.8f, sprite.bounds.size.y * 0.9f);

        go.AddComponent<BulletScript>();
    }

    void Start()
    {
        GetComponent<Rigidbody2D>().linearVelocity = transform.up * velocidad;
    }

    void Update()
    {
        Vector3 p = transform.position;
        if (Mathf.Abs(p.x) > GameManagerScript.limiteX + 1f || Mathf.Abs(p.y) > GameManagerScript.limiteY + 1f)
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (usada) return;

        AsteroidScript asteroide = other.GetComponent<AsteroidScript>();
        if (asteroide != null)
        {
            usada = true;
            asteroide.RecibirDisparo();
            Destroy(gameObject);
            return;
        }

        EnemyScript enemigo = other.GetComponent<EnemyScript>();
        if (enemigo != null)
        {
            usada = true;
            enemigo.RecibirDisparo();
            Destroy(gameObject);
            return;
        }

        EnemyBulletScript balaEnemiga = other.GetComponent<EnemyBulletScript>();
        if (balaEnemiga != null)
        {
            usada = true;
            SoundScript.Instance.PlayCancelacion();
            Destroy(balaEnemiga.gameObject);
            Destroy(gameObject);
        }
    }
}
