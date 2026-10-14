using UnityEngine;

public enum TipoPowerup { Multiple, Adicional, Vida }

public class PowerupScript : MonoBehaviour
{
    public static int Activos;

    public TipoPowerup tipo;
    public float duracion = 10f;

    float nacio;
    SpriteRenderer sr;

    void Awake() { Activos++; }
    void OnDestroy() { Activos--; }

    void Start()
    {
        nacio = Time.time;
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        float resta = duracion - (Time.time - nacio);
        if (resta <= 0f)
        {
            Destroy(gameObject);
            return;
        }
        if (resta < 3f) sr.enabled = Mathf.FloorToInt(Time.time * 6f) % 2 == 0;
    }

    public void Aplicar()
    {
        switch (tipo)
        {
            case TipoPowerup.Multiple: GameManagerScript.arma = TipoArma.Multiple; break;
            case TipoPowerup.Adicional: GameManagerScript.arma = TipoArma.Adicional; break;
            case TipoPowerup.Vida: GameManagerScript.vidas = GameManagerScript.MaxVidas; break;
        }
    }
}
