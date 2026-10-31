using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Monta la arena completa: cielo, plataforma destructible, dragón jugador con animator,
/// jefe animado, prefabs de esbirro y portal, y el HUD con las dos barras de vida.
/// </summary>
public class ConstruirArena
{
    const string RUTA_PLATAFORMA   = "Assets/Generated_Images/plataforma_cielo.png";
    const string RUTA_PORTAL       = "Assets/Generated_Images/portal_magico.png";
    const string RUTA_CIELO        = "Assets/Imagen/BackGround/6707.jpg";
    const string RUTA_HOJA_JEFE    = "Assets/Imagen/Enemy/Boss/Character_sheet.png";
    const string RUTA_ESBIRRO_WALK = "Assets/Imagen/Enemy/Enemy03_Walk_RP1 (32x36).png";
    const string RUTA_DRAGON       = "Assets/Dragon Warrior Files/Dragon Warrior PNG/";
    const string RUTA_BALA_JUG     = "Assets/Prefab/Circle (1).prefab";
    const string RUTA_BALA_JEFE    = "Assets/Prefab/Triangle.prefab";
    const string RUTA_CTRL_DRAGON  = "Assets/Animations/Dragon.controller";
    const string RUTA_CTRL_JEFE    = "Assets/Animations/Jefe.controller";
    const string RUTA_PREF_ESBIRRO = "Assets/Prefab/Esbirro.prefab";
    const string RUTA_PREF_PORTAL  = "Assets/Prefab/Portal.prefab";
    const string RUTA_BLANCO       = "Assets/UI/blanco.png";

    const float ANCHO_PLATAFORMA = 12.96f;
    const float TOPE_PLATAFORMA  = -0.72f;   // altura del suelo

    // ================================================================== ENTRADA
    public static void Execute()
    {
        ConfigurarImportadores();
        ConfigurarBalas();

        Sprite blanco = CrearSpriteBlanco();
        Sprite spritePlataforma = CargarSprite(RUTA_PLATAFORMA);
        Sprite spritePortal = CargarSprite(RUTA_PORTAL);

        GameObject prefEsbirro = CrearPrefabEsbirro();
        GameObject prefPortal = CrearPrefabPortal(prefEsbirro, spritePortal, blanco);

        ConstruirEscena(spritePlataforma, blanco, prefPortal);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[ConstruirArena] Arena construida.");
    }

    // ================================================================ IMPORT
    static void ConfigurarImportadores()
    {
        ConfigurarSprite(RUTA_PLATAFORMA, 100, false);
        ConfigurarSprite(RUTA_PORTAL, 100, false);
    }

    static void ConfigurarSprite(string ruta, int ppu, bool punto)
    {
        var ti = AssetImporter.GetAtPath(ruta) as TextureImporter;
        if (ti == null)
        {
            Debug.LogWarning("Sin importador: " + ruta);
            return;
        }

        ti.textureType = TextureImporterType.Sprite;
        ti.spriteImportMode = SpriteImportMode.Single;
        ti.spritePixelsPerUnit = ppu;
        ti.alphaIsTransparency = true;
        ti.mipmapEnabled = false;
        ti.filterMode = punto ? FilterMode.Point : FilterMode.Bilinear;
        ti.textureCompression = TextureImporterCompression.Uncompressed;
        ti.maxTextureSize = 2048;
        ti.SaveAndReimport();
    }

    // ================================================================ BALAS
    static void ConfigurarBalas()
    {
        EditarPrefab(RUTA_BALA_JUG, go =>
        {
            go.transform.localScale = Vector3.one * 0.3f;
            var b = go.GetComponent<Bala>();
            b.equipo = Equipo.Jugador;
            b.danio = 10f;
            b.velocidad = 20f;
            b.vida = 1.6f;
            var col = go.GetComponent<BoxCollider2D>();
            if (col != null) { col.isTrigger = true; col.size = new Vector2(0.9f, 0.9f); }
            var sr = go.GetComponent<SpriteRenderer>();
            if (sr != null) sr.sortingOrder = 7;
        });

        EditarPrefab(RUTA_BALA_JEFE, go =>
        {
            go.transform.localScale = Vector3.one * 0.3f;
            var b = go.GetComponent<Bala>();
            b.equipo = Equipo.Enemigo;
            b.danio = 10f;
            b.velocidad = 8.5f;
            b.vida = 5f;
            var col = go.GetComponent<BoxCollider2D>();
            if (col != null) { col.isTrigger = true; col.size = new Vector2(0.9f, 0.9f); }
            var sr = go.GetComponent<SpriteRenderer>();
            if (sr != null) sr.sortingOrder = 7;
        });
    }

    static void EditarPrefab(string ruta, System.Action<GameObject> editar)
    {
        GameObject contenido = PrefabUtility.LoadPrefabContents(ruta);
        if (contenido == null)
        {
            Debug.LogWarning("No se pudo abrir el prefab " + ruta);
            return;
        }

        editar(contenido);
        PrefabUtility.SaveAsPrefabAsset(contenido, ruta);
        PrefabUtility.UnloadPrefabContents(contenido);
    }

    // ================================================================ SPRITES
    static Sprite CargarSprite(string ruta)
    {
        Object[] todos = AssetDatabase.LoadAllAssetsAtPath(ruta);
        foreach (Object o in todos)
        {
            Sprite s = o as Sprite;
            if (s != null) return s;
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(ruta);
    }

    static Sprite CargarSubSprite(string ruta, string nombre)
    {
        Object[] todos = AssetDatabase.LoadAllAssetsAtPath(ruta);
        foreach (Object o in todos)
        {
            Sprite s = o as Sprite;
            if (s != null && s.name == nombre) return s;
        }
        Debug.LogWarning("No se encontró el sprite " + nombre + " en " + ruta);
        return CargarSprite(ruta);
    }

    static Sprite[] CargarSpritesOrdenados(string ruta)
    {
        Object[] todos = AssetDatabase.LoadAllAssetsAtPath(ruta);
        var lista = new List<Sprite>();
        foreach (Object o in todos)
        {
            Sprite s = o as Sprite;
            if (s != null) lista.Add(s);
        }

        lista.Sort((a, b) => NumeroFinal(a.name).CompareTo(NumeroFinal(b.name)));
        return lista.ToArray();
    }

    static int NumeroFinal(string nombre)
    {
        Match m = Regex.Match(nombre, @"(\d+)\s*$");
        return m.Success ? int.Parse(m.Groups[1].Value) : 0;
    }

    static Sprite CrearSpriteBlanco()
    {
        Sprite existente = AssetDatabase.LoadAssetAtPath<Sprite>(RUTA_BLANCO);
        if (existente != null) return existente;

        if (!Directory.Exists("Assets/UI")) Directory.CreateDirectory("Assets/UI");

        var tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
        var px = new Color32[16 * 16];
        for (int i = 0; i < px.Length; i++) px[i] = new Color32(255, 255, 255, 255);
        tex.SetPixels32(px);
        tex.Apply();

        File.WriteAllBytes(RUTA_BLANCO, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);

        AssetDatabase.ImportAsset(RUTA_BLANCO);

        var ti = AssetImporter.GetAtPath(RUTA_BLANCO) as TextureImporter;
        ti.textureType = TextureImporterType.Sprite;
        ti.spriteImportMode = SpriteImportMode.Single;
        ti.spritePixelsPerUnit = 100;
        ti.mipmapEnabled = false;
        ti.alphaIsTransparency = true;
        ti.textureCompression = TextureImporterCompression.Uncompressed;
        ti.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Sprite>(RUTA_BLANCO);
    }

    // ================================================================ PREFABS
    static GameObject CrearPrefabEsbirro()
    {
        GameObject existente = AssetDatabase.LoadAssetAtPath<GameObject>(RUTA_PREF_ESBIRRO);
        if (existente != null) return existente;

        Sprite[] sprites = CargarSpritesOrdenados(RUTA_ESBIRRO_WALK);

        var go = new GameObject("Esbirro");

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprites.Length > 0 ? sprites[0] : null;
        sr.sortingOrder = 4;
        go.transform.localScale = Vector3.one * 2.2f;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.32f, 0.32f);

        var salud = go.AddComponent<Salud>();
        salud.equipo = Equipo.Enemigo;
        salud.vidaMaxima = 20f;
        salud.destruirAlMorir = true;

        var esbirro = go.AddComponent<Esbirro>();
        esbirro.velocidadHorizontal = 3.6f;
        esbirro.velocidadVertical = 6f;
        esbirro.distanciaDanio = 1.05f;
        esbirro.danio = 12f;
        esbirro.frames = sprites;
        esbirro.fps = 12f;

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, RUTA_PREF_ESBIRRO);
        Object.DestroyImmediate(go);

        return prefab;
    }

    static GameObject CrearPrefabPortal(GameObject prefEsbirro, Sprite spritePortal, Sprite blanco)
    {
        // Tamaño del portal a partir de su contenido visible
        int tw, th;
        Rect caja = MedirContenido(RUTA_PORTAL, out tw, out th);
        float altoVisible = caja.height / 100f;
        float escala = altoVisible > 0.01f ? 2.1f / altoVisible : 0.22f;

        GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(RUTA_PREF_PORTAL);
        if (go == null)
        {
            GameObject temp = new GameObject("Portal");

            var sr = temp.AddComponent<SpriteRenderer>();
            sr.sprite = spritePortal;
            sr.sortingOrder = 3;

            var portal = temp.AddComponent<Portal>();
            portal.esbirroPrefab = prefEsbirro;
            portal.duracion = 6f;
            portal.intervalo = 1.15f;
            portal.maxEsbirros = 3;
            portal.escalaFinal = escala;
            portal.tiempoApertura = 0.35f;

            go = PrefabUtility.SaveAsPrefabAsset(temp, RUTA_PREF_PORTAL);
            Object.DestroyImmediate(temp);
        }

        return go;
    }

    static Rect MedirContenido(string ruta, out int ancho, out int alto)
    {
        ancho = 0;
        alto = 0;

        if (!File.Exists(ruta)) return new Rect();

        byte[] bytes = File.ReadAllBytes(ruta);
        var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);

        if (!tex.LoadImage(bytes))
        {
            Object.DestroyImmediate(tex);
            return new Rect();
        }

        ancho = tex.width;
        alto = tex.height;

        Color[] px = tex.GetPixels();
        int minX = ancho, maxX = -1, minY = alto, maxY = -1;

        for (int y = 0; y < alto; y++)
        {
            for (int x = 0; x < ancho; x++)
            {
                if (px[y * ancho + x].a > 0.1f)
                {
                    if (x < minX) minX = x;
                    if (x > maxX) maxX = x;
                    if (y < minY) minY = y;
                    if (y > maxY) maxY = y;
                }
            }
        }

        Object.DestroyImmediate(tex);

        if (maxX < 0) return new Rect(0f, 0f, ancho, alto);

        return new Rect(minX, minY, maxX - minX + 1, maxY - minY + 1);
    }

    // ================================================================ ESCENA
    static void ConstruirEscena(Sprite spritePlataforma, Sprite blanco, GameObject prefPortal)
    {
        GameObject arena = GameObject.Find("GameObject");
        if (arena == null) arena = new GameObject("Arena");
        arena.name = "Arena";

        Transform player = arena.transform.Find("Player");
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        GameObject boss = GameObject.Find("Boss");
        GameObject plataforma = GameObject.Find("Square");

        if (player == null || boss == null || plataforma == null)
        {
            Debug.LogError("[ConstruirArena] Faltan objetos base en la escena (Player / Boss / Square).");
            return;
        }

        // ------------------------------------------------ PLATAFORMA
        plataforma.name = "Plataforma";
        plataforma.transform.position = new Vector3(0f, -1.22f, 0f);
        plataforma.transform.localScale = Vector3.one;

        var srViejoPlat = plataforma.GetComponent<SpriteRenderer>();
        if (srViejoPlat != null) Object.DestroyImmediate(srViejoPlat);

        var colPlat = plataforma.GetComponent<BoxCollider2D>();
        colPlat.size = new Vector2(ANCHO_PLATAFORMA, 1f);
        colPlat.offset = Vector2.zero;
        colPlat.isTrigger = false;

        // Posicionar el visual con el borde superior justo en el tope del collider
        int tw, th;
        Rect caja = MedirContenido(RUTA_PLATAFORMA, out tw, out th);
        float escalaPlat = caja.width > 0f ? ANCHO_PLATAFORMA / (caja.width / 100f) : 0.844f;
        float topLocal = (caja.yMax - th * 0.5f) / 100f * escalaPlat;

        Transform visual = plataforma.transform.Find("Visual");
        if (visual == null)
        {
            var g = new GameObject("Visual");
            g.transform.SetParent(plataforma.transform, false);
            visual = g.transform;
        }

        var srVis = visual.GetComponent<SpriteRenderer>();
        if (srVis == null) srVis = visual.gameObject.AddComponent<SpriteRenderer>();
        srVis.sprite = spritePlataforma;
        srVis.color = Color.white;
        srVis.sortingOrder = 0;
        srVis.drawMode = SpriteDrawMode.Simple;

        visual.localPosition = new Vector3(0f, 0.5f - topLocal, 0f);
        visual.localScale = new Vector3(escalaPlat, escalaPlat, 1f);
        visual.localRotation = Quaternion.identity;

        var plataformaScript = plataforma.GetComponent<PlataformaCelestial>();
        if (plataformaScript == null) plataformaScript = plataforma.AddComponent<PlataformaCelestial>();
        plataformaScript.visual = visual;
        plataformaScript.colisionador = colPlat;
        plataformaScript.spriteEscombro = blanco;

        // ------------------------------------------------ JUGADOR (DRAGÓN)
        var srJugador = player.GetComponent<SpriteRenderer>();
        srJugador.sprite = CargarSprite(RUTA_DRAGON + "idle_01.png");
        srJugador.color = Color.white;
        srJugador.flipX = false;
        srJugador.sortingOrder = 5;

        // Ajustar collider y posición a los pies del dragón
        string rutaIdle = RUTA_DRAGON + "idle_01.png";
        int twD, thD;
        Rect cajaD = MedirContenido(rutaIdle, out twD, out thD);
        float ppuD = 100f;

        float piesLocalY = (cajaD.yMin - thD * 0.5f) / ppuD;
        float centroLocalX = ((cajaD.xMin + cajaD.xMax) * 0.5f - twD * 0.5f) / ppuD;
        float altoDragón = cajaD.height / ppuD;
        float anchoDragón = cajaD.width / ppuD;

        float altoCol = Mathf.Max(0.6f, altoDragón * 0.55f);
        float anchoCol = Mathf.Max(0.5f, anchoDragón * 0.6f);

        var colJugador = player.GetComponent<BoxCollider2D>();
        colJugador.size = new Vector2(anchoCol, altoCol);
        colJugador.offset = new Vector2(centroLocalX * 0.5f, piesLocalY + altoCol * 0.5f);
        colJugador.isTrigger = false;

        player.position = new Vector3(0.31f, TOPE_PLATAFORMA - piesLocalY, 0f);
        player.localScale = Vector3.one;
        player.localRotation = Quaternion.identity;

        var saludJugador = player.GetComponent<Salud>();
        saludJugador.equipo = Equipo.Jugador;
        saludJugador.vidaMaxima = 100f;
        saludJugador.destruirAlMorir = false;
        saludJugador.invulnerable = false;

        var rbJugador = player.GetComponent<Rigidbody2D>();
        rbJugador.bodyType = RigidbodyType2D.Dynamic;
        rbJugador.gravityScale = 1f;
        rbJugador.freezeRotation = true;
        rbJugador.sleepMode = RigidbodySleepMode2D.NeverSleep;
        rbJugador.interpolation = RigidbodyInterpolation2D.Interpolate;

        var animJugador = player.GetComponent<Animator>();
        if (animJugador == null) animJugador = player.gameObject.AddComponent<Animator>();
        animJugador.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(RUTA_CTRL_DRAGON);
        animJugador.applyRootMotion = false;
        animJugador.cullingMode = AnimatorCullingMode.AlwaysAnimate;

        Transform armaPivot = player.Find("ArmaPivot");
        Transform armaSprite = armaPivot != null ? armaPivot.Find("Pistol 1 (72 ppi)") : null;
        Transform punta = armaSprite != null ? armaSprite.Find("Punta") : null;

        if (armaPivot != null)
        {
            armaPivot.localPosition = new Vector3(0f, -0.30f, 0f);
            armaPivot.localRotation = Quaternion.identity;
        }
        if (armaSprite != null)
        {
            armaSprite.localPosition = new Vector3(0.55f, 0.10f, 0f);
            armaSprite.localScale = new Vector3(0.27f, 0.27f, 0.27f);

            var srArma = armaSprite.GetComponent<SpriteRenderer>();
            if (srArma != null) srArma.sortingOrder = 6;
        }

        Transform chequeo = player.Find("ChequeoSueli");
        if (chequeo != null) Object.DestroyImmediate(chequeo.gameObject);

        var control = player.GetComponent<PlayerController>();
        control.bolaFuegoPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/BolaDeFuego.prefab");
        control.cadencia = 0.22f;
        control.cargasMaximas = 15;
        control.tiempoEnfriamiento = 2.5f;
        control.danio = 12f;
        control.velocidadProyectil = 17f;
        control.animador = animJugador;
        control.spriteCuerpo = srJugador;
        control.velocidad = 7f;
        control.fuerzaSalto = 12f;
        control.velocidadMaximaCaida = 9f;

        // ------------------------------------------------ JEFE
        var srJefe = boss.GetComponent<SpriteRenderer>();
        srJefe.sprite = CargarSubSprite(RUTA_HOJA_JEFE, "Character_sheet_4");
        srJefe.color = Color.white;
        srJefe.sortingOrder = 5;

        boss.transform.position = new Vector3(0.6f, 3.6f, 0f);
        boss.transform.localScale = new Vector3(3.3f, 3.3f, 1f);

        var colJefe = boss.GetComponent<BoxCollider2D>();
        colJefe.isTrigger = true;
        colJefe.size = new Vector2(0.8f, 0.8f);
        colJefe.offset = Vector2.zero;

        var rbJefe = boss.GetComponent<Rigidbody2D>();
        rbJefe.bodyType = RigidbodyType2D.Dynamic;
        rbJefe.gravityScale = 0f;
        rbJefe.freezeRotation = true;
        rbJefe.interpolation = RigidbodyInterpolation2D.Interpolate;

        var saludJefe = boss.GetComponent<Salud>();
        saludJefe.equipo = Equipo.Enemigo;
        saludJefe.vidaMaxima = 400f;
        saludJefe.destruirAlMorir = false;
        saludJefe.invulnerable = false;

        var animJefe = boss.GetComponent<Animator>();
        if (animJefe == null) animJefe = boss.AddComponent<Animator>();
        animJefe.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(RUTA_CTRL_JEFE);
        animJefe.applyRootMotion = false;
        animJefe.cullingMode = AnimatorCullingMode.AlwaysAnimate;

        Transform puntoDisparo = boss.transform.Find("PuntoDeDisparo");
        if (puntoDisparo == null)
        {
            var g = new GameObject("PuntoDeDisparo");
            g.transform.SetParent(boss.transform, false);
            puntoDisparo = g.transform;
            puntoDisparo.localPosition = new Vector3(0f, -0.52f, 0f);
        }

        var jefe = boss.GetComponent<JefeFlotante>();
        jefe.objetivo = player;
        jefe.jugador = control;
        jefe.saludPropia = saludJefe;
        jefe.velocidadSeguir = 3.5f;
        jefe.alturaSobreObjetivo = 3.4f;
        jefe.rangoHorizontal = 4.6f;
        jefe.balaPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(RUTA_BALA_JEFE);
        jefe.puntoDisparo = puntoDisparo;
        jefe.cadenciaDisparo = 1.9f;
        jefe.proyectilesPorRafaga = 3;
        jefe.dispersionGrados = 13f;
        jefe.esperaEntreProyectiles = 0.12f;
        jefe.velocidadProyectil = 8.5f;
        jefe.vidaProyectil = 5f;
        jefe.danioProyectil = 10f;
        jefe.portalPrefab = prefPortal;
        jefe.cadenciaPortales = 8.5f;
        jefe.portalesPorAtaque = 2;
        jefe.separacionPortales = 3.4f;
        jefe.alturaPortales = 1.2f;
        jefe.plataforma = plataformaScript;
        jefe.umbralFase2 = 0.5f;
        jefe.animador = animJefe;

        // ------------------------------------------------ CÁMARA
        Camera cam = Camera.main;
        cam.transform.position = new Vector3(0f, 1.7f, -10f);
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.44f, 0.74f, 0.95f, 1f);

        var seguir = cam.GetComponent<CamaraSeguir>();
        if (seguir == null) seguir = cam.gameObject.AddComponent<CamaraSeguir>();
        seguir.objetivo = player;
        seguir.desplazamiento = new Vector3(0f, 1.2f, -10f);
        seguir.suavizado = 7f;

        Transform cielo = cam.transform.Find("Cielo");
        if (cielo == null)
        {
            var g = new GameObject("Cielo");
            g.transform.SetParent(cam.transform, false);
            cielo = g.transform;
        }

        var srCielo = cielo.GetComponent<SpriteRenderer>();
        if (srCielo == null) srCielo = cielo.gameObject.AddComponent<SpriteRenderer>();
        srCielo.sprite = CargarSprite(RUTA_CIELO);
        srCielo.color = Color.white;
        srCielo.sortingOrder = -1000;

        cielo.localPosition = new Vector3(0f, 0f, 15f);
        cielo.localScale = Vector3.one * 1.25f;
        cielo.localRotation = Quaternion.identity;

        // ------------------------------------------------ HUD
        CrearHUD(player.GetComponent<Salud>(), saludJefe, control, blanco);

        // ------------------------------------------------ BUILD SETTINGS
        RegistrarEscenaEnBuild();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
    }

    // ================================================================ HUD
    static Font FuenteUI()
    {
        Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (f == null) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return f;
    }

    static RectTransform CrearRect(string nombre, Transform padre, Vector2 aMin, Vector2 aMax,
                                   Vector2 pivot, Vector2 pos, Vector2 tam)
    {
        var go = new GameObject(nombre, typeof(RectTransform));
        var rt = go.GetComponent<RectTransform>();
        rt.SetParent(padre, false);
        rt.anchorMin = aMin;
        rt.anchorMax = aMax;
        rt.pivot = pivot;
        rt.anchoredPosition = pos;
        rt.sizeDelta = tam;
        rt.localScale = Vector3.one;
        return rt;
    }

    static Text CrearTexto(string nombre, Transform padre, Vector2 aMin, Vector2 aMax, Vector2 pivot,
                           Vector2 pos, Vector2 tam, int tamFuente, TextAnchor anclaje, Color color, string contenido)
    {
        var rt = CrearRect(nombre, padre, aMin, aMax, pivot, pos, tam);
        var t = rt.gameObject.AddComponent<Text>();
        t.font = FuenteUI();
        t.fontSize = tamFuente;
        t.alignment = anclaje;
        t.color = color;
        t.text = contenido;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        return t;
    }

    static Image CrearBarra(string nombre, Transform padre, Vector2 aMin, Vector2 aMax, Vector2 pivot,
                            Vector2 pos, Vector2 tam, Color colorFondo, out Image relleno,
                            Color colorRelleno, Sprite blanco)
    {
        var rt = CrearRect(nombre, padre, aMin, aMax, pivot, pos, tam);

        var fondo = rt.gameObject.AddComponent<Image>();
        fondo.sprite = blanco;
        fondo.color = colorFondo;
        fondo.raycastTarget = false;

        var rtRelleno = CrearRect("Relleno", rt, Vector2.zero, Vector2.one, new Vector2(0f, 0.5f),
                                  Vector2.zero, new Vector2(-6f, -6f));
        relleno = rtRelleno.gameObject.AddComponent<Image>();
        relleno.sprite = blanco;
        relleno.color = colorRelleno;
        relleno.type = Image.Type.Filled;
        relleno.fillMethod = Image.FillMethod.Horizontal;
        relleno.fillOrigin = 0;
        relleno.fillAmount = 1f;
        relleno.raycastTarget = false;

        return fondo;
    }

    static void CrearHUD(Salud saludJugador, Salud saludJefe, PlayerController control, Sprite blanco)
    {
        GameObject canvasGO = GameObject.Find("CanvasHUD");
        if (canvasGO == null)
        {
            canvasGO = new GameObject("CanvasHUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        }

        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        Transform raiz = canvasGO.transform;

        // ---------------- Barra del JEFE (arriba, centrada)
        Image rellenoJefe;
        CrearBarra("BarraJefe", raiz, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                   new Vector2(0f, -46f), new Vector2(820f, 34f),
                   new Color(0.05f, 0.05f, 0.09f, 0.9f), out rellenoJefe,
                   new Color(0.85f, 0.19f, 0.25f, 1f), blanco);

        CrearTexto("NombreJefe", raiz, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                   new Vector2(0f, -14f), new Vector2(820f, 30f), 26, TextAnchor.MiddleCenter,
                   new Color(1f, 0.93f, 0.72f, 1f), "SEÑOR DEL ABISMO");

        Text textoVidaJefe = CrearTexto("TextoVidaJefe", raiz, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                   new Vector2(0.5f, 1f), new Vector2(0f, -84f), new Vector2(820f, 24f), 18,
                   TextAnchor.MiddleCenter, Color.white, "400 / 400");

        // ---------------- Barra del JUGADOR (abajo, izquierda)
        Image rellenoJugador;
        CrearBarra("BarraJugador", raiz, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f),
                   new Vector2(32f, 66f), new Vector2(460f, 32f),
                   new Color(0.05f, 0.05f, 0.09f, 0.9f), out rellenoJugador,
                   new Color(0.35f, 0.95f, 0.45f, 1f), blanco);

        Text textoVidaJugador = CrearTexto("TextoVidaJugador", raiz, new Vector2(0f, 0f), new Vector2(0f, 0f),
                   new Vector2(0f, 0f), new Vector2(32f, 104f), new Vector2(460f, 28f), 20,
                   TextAnchor.MiddleLeft, Color.white, "100 / 100");

        // ---------------- Munición (abajo, derecha)
        Text textoMunicion = CrearTexto("TextoMunicion", raiz, new Vector2(1f, 0f), new Vector2(1f, 0f),
                   new Vector2(1f, 0f), new Vector2(-32f, 66f), new Vector2(360f, 46f), 38,
                   TextAnchor.MiddleRight, new Color(1f, 0.88f, 0.55f, 1f), "15 / 15");

        Text textoEstadoArma = CrearTexto("TextoEstadoArma", raiz, new Vector2(1f, 0f), new Vector2(1f, 0f),
                   new Vector2(1f, 0f), new Vector2(-32f, 114f), new Vector2(420f, 30f), 20,
                   TextAnchor.MiddleRight, new Color(1f, 0.55f, 0.35f, 1f), "");

        // ---------------- Aviso central
        Text textoAviso = CrearTexto("TextoAviso", raiz, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                   new Vector2(0.5f, 0.5f), new Vector2(0f, 150f), new Vector2(1400f, 140f), 46,
                   TextAnchor.MiddleCenter, new Color(1f, 0.85f, 0.25f, 1f), "");

        // ---------------- Panel final
        var rtPanel = CrearRect("PanelFinal", raiz, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
                                Vector2.zero, Vector2.zero);
        var imgPanel = rtPanel.gameObject.AddComponent<Image>();
        imgPanel.sprite = blanco;
        imgPanel.color = new Color(0f, 0f, 0f, 0.72f);
        imgPanel.raycastTarget = false;

        Text textoFinal = CrearTexto("TextoFinal", rtPanel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                   new Vector2(0.5f, 0.5f), new Vector2(0f, 40f), new Vector2(1400f, 160f), 96,
                   TextAnchor.MiddleCenter, Color.white, "");

        Text textoReinicio = CrearTexto("TextoReinicio", rtPanel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                   new Vector2(0.5f, 0.5f), new Vector2(0f, -80f), new Vector2(1400f, 50f), 28,
                   TextAnchor.MiddleCenter, new Color(0.9f, 0.9f, 0.95f, 1f), "");

        rtPanel.gameObject.SetActive(false);

        // ---------------- Componente HUD
        var hud = canvasGO.GetComponent<HUDJuego>();
        if (hud == null) hud = canvasGO.AddComponent<HUDJuego>();

        hud.saludJugador = saludJugador;
        hud.jugador = control;
        hud.barraJugador = rellenoJugador;
        hud.textoVidaJugador = textoVidaJugador;

        hud.saludJefe = saludJefe;
        hud.barraJefe = rellenoJefe;
        hud.textoVidaJefe = textoVidaJefe;
        hud.textoNombreJefe = null;

        hud.textoMunicion = textoMunicion;
        hud.textoEstadoArma = textoEstadoArma;

        hud.textoAviso = textoAviso;
        hud.panelFinal = rtPanel.gameObject;
        hud.textoFinal = textoFinal;
        hud.textoReinicio = textoReinicio;

        // Referencias del jefe al HUD ya están por singleton
    }

    static void RegistrarEscenaEnBuild()
    {
        string ruta = "Assets/Scenes/SampleScene.unity";
        var lista = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

        foreach (var s in lista)
            if (s.path == ruta) return;

        lista.Insert(0, new EditorBuildSettingsScene(ruta, true));
        EditorBuildSettings.scenes = lista.ToArray();
    }
}
