using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HudScript : MonoBehaviour
{
    public static HudScript Instance;

    Canvas canvas;
    TMP_FontAsset fuente;
    Image[] corazones;
    TextMeshProUGUI txtArma, txtTitulo, txtSubtitulo, txtMeta;
    float ocultarMetaEn;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        canvas = FindFirstObjectByType<Canvas>();
        TextMeshProUGUI existente = FindFirstObjectByType<TextMeshProUGUI>();
        if (existente != null) fuente = existente.font;

        corazones = new Image[GameManagerScript.MaxVidas];
        for (int i = 0; i < corazones.Length; i++)
        {
            GameObject go = new GameObject("Vida_" + (i + 1), typeof(RectTransform));
            go.transform.SetParent(canvas.transform, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0, 0);
            rt.anchoredPosition = new Vector2(15 + i * 34, 15);
            rt.sizeDelta = new Vector2(30, 30);
            Image img = go.AddComponent<Image>();
            img.sprite = GameAssets.Instance.corazon;
            img.preserveAspect = true;
            img.raycastTarget = false;
            corazones[i] = img;
        }

        txtArma = CrearTexto("ArmaSeleccionada", new Vector2(1, 0), new Vector2(1, 0), new Vector2(-15, 15), new Vector2(320, 34), 22, TextAlignmentOptions.BottomRight);

        txtTitulo = CrearTexto("GameOverTitulo", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 30), new Vector2(700, 90), 56, TextAlignmentOptions.Center);
        txtSubtitulo = CrearTexto("GameOverSubtitulo", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -30), new Vector2(700, 80), 22, TextAlignmentOptions.Center);
        txtMeta = CrearTexto("MetaAlcanzada", new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -80), new Vector2(700, 40), 26, TextAlignmentOptions.Center);
        txtTitulo.gameObject.SetActive(false);
        txtSubtitulo.gameObject.SetActive(false);
        txtMeta.gameObject.SetActive(false);
    }

    TextMeshProUGUI CrearTexto(string nombre, Vector2 ancla, Vector2 pivote, Vector2 pos, Vector2 tam, float tamLetra, TextAlignmentOptions alineacion)
    {
        GameObject go = new GameObject(nombre, typeof(RectTransform));
        go.transform.SetParent(canvas.transform, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = ancla;
        rt.pivot = pivote;
        rt.anchoredPosition = pos;
        rt.sizeDelta = tam;

        TextMeshProUGUI t = go.AddComponent<TextMeshProUGUI>();
        if (fuente != null) t.font = fuente;
        t.fontSize = tamLetra;
        t.alignment = alineacion;
        t.color = Color.white;
        t.raycastTarget = false;
        return t;
    }

    void Update()
    {
        if (corazones == null) return;

        for (int i = 0; i < corazones.Length; i++)
            corazones[i].color = i < GameManagerScript.vidas ? Color.white : new Color(0.2f, 0.2f, 0.2f, 0.6f);

        switch (GameManagerScript.arma)
        {
            case TipoArma.Normal: txtArma.text = "ARMA: NORMAL"; txtArma.color = new Color(0.6f, 0.85f, 1f); break;
            case TipoArma.Multiple: txtArma.text = "ARMA: MULTIPLE"; txtArma.color = new Color(0.4f, 1f, 0.5f); break;
            case TipoArma.Adicional: txtArma.text = "ARMA: ADICIONAL"; txtArma.color = new Color(1f, 0.8f, 0.2f); break;
        }

        if (txtMeta.gameObject.activeSelf && Time.time > ocultarMetaEn) txtMeta.gameObject.SetActive(false);
    }

    public void MostrarMeta()
    {
        if (GameManagerScript.gameOver) return;
        txtMeta.text = "META ALCANZADA: " + GameManagerScript.PuntajeMeta + " PUNTOS";
        txtMeta.color = new Color(1f, 0.85f, 0.2f);
        txtMeta.gameObject.SetActive(true);
        ocultarMetaEn = Time.time + 5f;
    }

    public void MostrarGameOver()
    {
        txtMeta.gameObject.SetActive(false);
        txtTitulo.text = "GAME OVER";
        txtTitulo.color = new Color(1f, 0.3f, 0.3f);
        txtSubtitulo.text = "PUNTOS: " + GameManagerScript.score + "   MAX: " + PlayerPrefs.GetInt("max", 0) + "\nPRESIONA R PARA REINICIAR";
        txtTitulo.gameObject.SetActive(true);
        txtSubtitulo.gameObject.SetActive(true);
    }
}
