using UnityEngine;

public class GameAssets : MonoBehaviour
{
    public static GameAssets Instance;

    public GameObject explosion;

    public Sprite balaNormal;
    public Sprite balaAdicional;

    public Sprite[] asteroide;

    public Sprite enemigo1;
    public Sprite enemigo2;
    public Sprite balaEnemigo1;
    public Sprite balaEnemigo2;

    public Sprite powerupMultiple;
    public Sprite powerupAdicional;
    public Sprite powerupVida;

    public Sprite corazon;

    void Awake()
    {
        Instance = this;
    }
}
