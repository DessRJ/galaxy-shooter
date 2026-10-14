using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class MenuScript : MonoBehaviour
{
    Canvas canvas;
    TMP_FontAsset fuente;
    Image fondo;
    GameObject panelMenu, panelInstrucciones, panelRegistro, panelCuenta;
    TextMeshProUGUI txtRecord, txtCuenta, txtResumen, txtVacio;
    TextMeshProUGUI[,] celdas;
    List<TextMeshProUGUI> marcadores = new List<TextMeshProUGUI>();
    SpriteRenderer naveSr;
    Coroutine cuenta;

    static readonly Color Oscuro = new Color(0.02f, 0.03f, 0.1f, 0.88f);
    static readonly Color Amarillo = new Color(1f, 0.85f, 0.25f);
    static readonly Color Celeste = new Color(0.6f, 0.85f, 1f);

    void Start()
    {
        canvas = FindFirstObjectByType<Canvas>();
        if (canvas.GetComponent<GraphicRaycaster>() == null) canvas.gameObject.AddComponent<GraphicRaycaster>();
        if (FindFirstObjectByType<EventSystem>() == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

        TextMeshProUGUI existente = FindFirstObjectByType<TextMeshProUGUI>();
        if (existente != null) fuente = existente.font;

        string[] propios = { "ArmaSeleccionada", "GameOverTitulo", "GameOverSubtitulo", "MetaAlcanzada" };
        foreach (TextMeshProUGUI t in canvas.GetComponentsInChildren<TextMeshProUGUI>(false))
            if (System.Array.IndexOf(propios, t.gameObject.name) < 0) marcadores.Add(t);

        if (PlayerScript.Instance != null) naveSr = PlayerScript.Instance.GetComponent<SpriteRenderer>();

        fondo = CrearImagen("MenuFondo", canvas.transform, Oscuro);
        Estirar(fondo.rectTransform);

        ConstruirMenu();
        ConstruirInstrucciones();
        ConstruirRegistro();
        ConstruirCuenta();

        if (GameManagerScript.saltarMenu)
        {
            GameManagerScript.saltarMenu = false;
            Jugar();
        }
        else
        {
            MostrarMarcadores(false);
            Mostrar(panelMenu);
        }
    }

    void Update()
    {
        if (GameManagerScript.estado != EstadoJuego.Menu) return;

        bool enMenu = panelMenu.activeSelf;
        if (enMenu && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))) Jugar();
        if (!enMenu && Input.GetKeyDown(KeyCode.Escape)) Mostrar(panelMenu);
    }

    void ConstruirMenu()
    {
        panelMenu = CrearPanel("PanelMenu");
        Sprite logo = Resources.Load<Sprite>("titulo");
        if (logo != null)
        {
            Image img = CrearImagen("Logo", panelMenu.transform, Color.white);
            img.sprite = logo;
            img.preserveAspect = true;
            Fijar(img.rectTransform, new Vector2(0, 148), new Vector2(420, 150));
        }
        else
        {
            CrearTexto(panelMenu.transform, "Titulo", "GALAXY SHOOTERS", 48, Amarillo, new Vector2(0, 148), new Vector2(760, 70));
        }
        CrearTexto(panelMenu.transform, "Subtitulo", "SOBREVIVE Y LLEGA A " + GameManagerScript.PuntajeMeta + " PUNTOS", 20, Celeste, new Vector2(0, 62), new Vector2(760, 30));
        CrearBoton(panelMenu.transform, "JUGAR", new Vector2(0, 20), Jugar);
        CrearBoton(panelMenu.transform, "INSTRUCCIONES", new Vector2(0, -34), () => Mostrar(panelInstrucciones));
        CrearBoton(panelMenu.transform, "REGISTRO", new Vector2(0, -88), AbrirRegistro);
        CrearBoton(panelMenu.transform, "SALIR", new Vector2(0, -142), Salir);
        txtRecord = CrearTexto(panelMenu.transform, "Record", "", 20, Color.white, new Vector2(0, -192), new Vector2(760, 30));
    }

    void ConstruirInstrucciones()
    {
        panelInstrucciones = CrearPanel("PanelInstrucciones");
        CrearTexto(panelInstrucciones.transform, "Titulo", "COMO JUGAR", 40, Amarillo, new Vector2(0, 192), new Vector2(760, 50));

        Sprite ilustracion = Resources.Load<Sprite>("instructivo");
        if (ilustracion == null)
        {
            Texture2D tex = Resources.Load<Texture2D>("instructivo");
            if (tex != null) ilustracion = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
        }
        if (ilustracion != null)
        {
            Image img = CrearImagen("Ilustracion", panelInstrucciones.transform, Color.white);
            img.sprite = ilustracion;
            img.preserveAspect = true;
            Fijar(img.rectTransform, new Vector2(0, 62), new Vector2(600, 200));
        }

        string[] lineas =
        {
            "EL PUNTERO DEL MOUSE DIRIGE LA NAVE",
            "FLECHAS O TECLAS WASD PARA MOVERLA",
            "CLICK IZQUIERDO O ESPACIO PARA DISPARAR"
        };
        for (int i = 0; i < lineas.Length; i++)
            CrearTexto(panelInstrucciones.transform, "Linea" + i, lineas[i], 22, Color.white, new Vector2(0, -58 - i * 28), new Vector2(760, 28));

        CrearTexto(panelInstrucciones.transform, "Consejo", "RECOGE POWERUPS Y CUIDA TUS 5 VIDAS", 18, Celeste, new Vector2(0, -146), new Vector2(760, 26));
        CrearBoton(panelInstrucciones.transform, "VOLVER", new Vector2(0, -190), () => Mostrar(panelMenu));
    }

    void ConstruirRegistro()
    {
        panelRegistro = CrearPanel("PanelRegistro");
        CrearTexto(panelRegistro.transform, "Titulo", "REGISTRO DE PARTIDAS", 38, Amarillo, new Vector2(0, 195), new Vector2(760, 50));
        txtResumen = CrearTexto(panelRegistro.transform, "Resumen", "", 18, Celeste, new Vector2(0, 163), new Vector2(760, 26));

        float[] xs = { -310, -250, -90, 50, 190 };
        string[] cabeceras = { "#", "FECHA", "PUNTOS", "TIEMPO", "META" };
        for (int c = 0; c < xs.Length; c++)
            CrearColumna(panelRegistro.transform, "Cabecera" + c, cabeceras[c], new Vector2(xs[c], 136), Amarillo);

        celdas = new TextMeshProUGUI[RegistroPartidas.Maximo, xs.Length];
        for (int f = 0; f < RegistroPartidas.Maximo; f++)
            for (int c = 0; c < xs.Length; c++)
                celdas[f, c] = CrearColumna(panelRegistro.transform, "Celda" + f + "_" + c, "", new Vector2(xs[c], 112 - f * 22), Color.white);

        txtVacio = CrearTexto(panelRegistro.transform, "Vacio", "AUN NO HAY PARTIDAS", 26, Color.white, new Vector2(0, 20), new Vector2(760, 40));
        CrearBoton(panelRegistro.transform, "VOLVER", new Vector2(-150, -190), () => Mostrar(panelMenu), new Vector2(240, 44));
        CrearBoton(panelRegistro.transform, "BORRAR", new Vector2(150, -190), BorrarRegistro, new Vector2(240, 44));
    }

    void ConstruirCuenta()
    {
        panelCuenta = CrearPanel("PanelCuenta");
        CrearTexto(panelCuenta.transform, "Preparate", "PREPARATE", 36, Celeste, new Vector2(0, 105), new Vector2(760, 50));
        txtCuenta = CrearTexto(panelCuenta.transform, "Numero", "3", 150, Color.white, new Vector2(0, 0), new Vector2(500, 180));
    }

    void Mostrar(GameObject panel)
    {
        panelMenu.SetActive(panel == panelMenu);
        panelInstrucciones.SetActive(panel == panelInstrucciones);
        panelRegistro.SetActive(panel == panelRegistro);
        panelCuenta.SetActive(panel == panelCuenta);
        fondo.enabled = panel != null;
        fondo.color = panel == panelCuenta ? new Color(0.02f, 0.03f, 0.1f, 0.35f) : Oscuro;

        if (panel == panelMenu)
            txtRecord.text = "RECORD: " + PlayerPrefs.GetInt("max", 0) + "     PARTIDAS: " + RegistroPartidas.Total;
    }

    void AbrirRegistro()
    {
        List<Partida> lista = RegistroPartidas.Obtener();
        for (int f = 0; f < RegistroPartidas.Maximo; f++)
        {
            bool hay = f < lista.Count;
            for (int c = 0; c < celdas.GetLength(1); c++) celdas[f, c].gameObject.SetActive(hay);
            if (!hay) continue;
            Partida p = lista[f];
            celdas[f, 0].text = (f + 1).ToString();
            celdas[f, 1].text = p.fecha;
            celdas[f, 2].text = p.puntos.ToString();
            celdas[f, 3].text = RegistroPartidas.FormatoTiempo(p.segundos);
            celdas[f, 4].text = p.meta ? "SI" : "NO";
            celdas[f, 4].color = p.meta ? new Color(0.4f, 1f, 0.5f) : new Color(1f, 0.5f, 0.5f);
        }
        txtVacio.gameObject.SetActive(lista.Count == 0);
        txtResumen.text = "PARTIDAS: " + RegistroPartidas.Total + "     MEJOR PUNTAJE: " + PlayerPrefs.GetInt("max", 0);
        Mostrar(panelRegistro);
    }

    void BorrarRegistro()
    {
        RegistroPartidas.Borrar();
        AbrirRegistro();
    }

    void Salir()
    {
        PlayerPrefs.Save();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void Jugar()
    {
        if (cuenta != null) return;
        cuenta = StartCoroutine(CuentaRegresiva());
    }

    IEnumerator CuentaRegresiva()
    {
        GameManagerScript.estado = EstadoJuego.Cuenta;
        MostrarMarcadores(true);
        Mostrar(panelCuenta);
        if (SoundScript.Instance != null) SoundScript.Instance.SubirMusica();

        Color[] colores = { new Color(0.4f, 1f, 0.5f), Amarillo, new Color(1f, 0.4f, 0.4f) };
        RectTransform rt = txtCuenta.rectTransform;

        for (int n = 3; n >= 1; n--)
        {
            txtCuenta.text = n.ToString();
            if (SoundScript.Instance != null) SoundScript.Instance.PlayBip(440f, 0.15f);
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t);
                rt.localScale = Vector3.one * Mathf.Lerp(1.5f, 1f, k);
                Color c = colores[n - 1];
                c.a = Mathf.Lerp(1f, 0.5f, k);
                txtCuenta.color = c;
                yield return null;
            }
        }

        txtCuenta.text = "YA!";
        txtCuenta.color = Color.white;
        rt.localScale = Vector3.one * 0.7f;
        if (SoundScript.Instance != null) SoundScript.Instance.PlayBip(880f, 0.3f);
        yield return new WaitForSeconds(0.5f);

        Mostrar(null);
        panelCuenta.SetActive(false);
        cuenta = null;
        GameManagerScript.Comenzar();
    }

    void MostrarMarcadores(bool visible)
    {
        foreach (TextMeshProUGUI t in marcadores)
            if (t != null) t.enabled = visible;
        if (naveSr != null) naveSr.enabled = visible;
    }

    GameObject CrearPanel(string nombre)
    {
        GameObject go = new GameObject(nombre, typeof(RectTransform));
        go.transform.SetParent(canvas.transform, false);
        Estirar(go.GetComponent<RectTransform>());
        go.SetActive(false);
        return go;
    }

    Image CrearImagen(string nombre, Transform padre, Color color)
    {
        GameObject go = new GameObject(nombre, typeof(RectTransform));
        go.transform.SetParent(padre, false);
        Image img = go.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
        return img;
    }

    TextMeshProUGUI CrearTexto(Transform padre, string nombre, string texto, float tamano, Color color, Vector2 pos, Vector2 tam)
    {
        GameObject go = new GameObject(nombre, typeof(RectTransform));
        go.transform.SetParent(padre, false);
        Fijar(go.GetComponent<RectTransform>(), pos, tam);

        TextMeshProUGUI t = go.AddComponent<TextMeshProUGUI>();
        if (fuente != null) t.font = fuente;
        t.text = texto;
        t.fontSize = tamano;
        t.color = color;
        t.alignment = TextAlignmentOptions.Center;
        t.raycastTarget = false;
        return t;
    }

    TextMeshProUGUI CrearColumna(Transform padre, string nombre, string texto, Vector2 pos, Color color)
    {
        TextMeshProUGUI t = CrearTexto(padre, nombre, texto, 20, color, pos, new Vector2(150, 24));
        t.alignment = TextAlignmentOptions.Left;
        t.rectTransform.pivot = new Vector2(0, 0.5f);
        return t;
    }

    void CrearBoton(Transform padre, string texto, Vector2 pos, UnityEngine.Events.UnityAction accion, Vector2? tam = null)
    {
        Image img = CrearImagen("Boton" + texto, padre, new Color(0.15f, 0.2f, 0.5f, 1f));
        img.raycastTarget = true;
        Fijar(img.rectTransform, pos, tam ?? new Vector2(290, 44));

        Button b = img.gameObject.AddComponent<Button>();
        b.targetGraphic = img;
        ColorBlock cb = b.colors;
        cb.normalColor = Color.white;
        cb.highlightedColor = new Color(1f, 0.9f, 0.45f);
        cb.pressedColor = new Color(0.7f, 0.7f, 0.7f);
        cb.selectedColor = Color.white;
        b.colors = cb;
        b.onClick.AddListener(accion);

        Outline borde = img.gameObject.AddComponent<Outline>();
        borde.effectColor = new Color(0.65f, 0.75f, 1f, 1f);
        borde.effectDistance = new Vector2(2, -2);

        TextMeshProUGUI t = CrearTexto(img.transform, "Texto", texto, 26, Color.white, Vector2.zero, img.rectTransform.sizeDelta);
        Estirar(t.rectTransform);
    }

    static void Estirar(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    static void Fijar(RectTransform rt, Vector2 pos, Vector2 tam)
    {
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = tam;
    }
}
