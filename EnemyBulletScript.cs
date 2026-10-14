using UnityEngine;

public class EnemyBulletScript : MonoBehaviour
{
    public Vector2 velocidad;

    public static void Crear(Vector3 posicion, Vector2 velocidad, Sprite sprite, float altura = 0.5f)
    {
        GameObject go = new GameObject("EnemyBullet");
        go.transform.position = posicion;

        float angulo = Mathf.Atan2(velocidad.y, velocidad.x) * Mathf.Rad2Deg - 90f;
        go.transform.rotation = Quaternion.Euler(0, 0, angulo);
        float escala = altura / sprite.bounds.size.y;
        go.transform.localScale = new Vector3(escala, escala, 1f);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 7;

        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0;
        rb.useFullKinematicContacts = true;

        CircleCollider2D col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = sprite.bounds.extents.x * 0.9f;

        go.AddComponent<EnemyBulletScript>().velocidad = velocidad;
    }

    void Update()
    {
        transform.position += (Vector3)(velocidad * Time.deltaTime);

        Vector3 p = transform.position;
        if (Mathf.Abs(p.x) > GameManagerScript.limiteX + 1f || Mathf.Abs(p.y) > GameManagerScript.limiteY + 1f)
            Destroy(gameObject);
    }
}
