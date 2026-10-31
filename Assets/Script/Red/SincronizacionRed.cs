using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Online cooperativo: el host simula el mundo y el cliente reproduce el estado.
/// Envía a 30 Hz la posición de los dos dragones, la del jefe y las bolas de fuego.
/// </summary>
public class SincronizacionRed : MonoBehaviour
{
    [Header("Referencias")]
    public PlayerController jugadorLocal;    // el dragón de esta escena
    public PlayerController jugadorRemoto;   // el dragón que llega por red
    public JefeFlotante jefe;

    [Header("Bolas de fuego (solo cliente)")]
    public Sprite spriteBala;
    public float escalaBala = 0.5f;

    [Header("Ritmo")]
    public float frecuencia = 1f / 30f;

    EntradaRed entradaRemota;
    EntradaJugador entradaPropia;
    float proximoEnvio;
    readonly List<SpriteRenderer> balasCliente = new List<SpriteRenderer>();

    bool activo;

    void Start()
    {
        if (!ConfiguracionPartida.EsOnline)
        {
            enabled = false;
            return;
        }

        activo = true;

        if (jefe == null) jefe = JefeFlotante.Activo;

        // Online sin portales: mantiene el mundo sincronizado y simple
        if (jefe != null) jefe.portalPrefab = null;

        if (ConfiguracionPartida.EsHost)
        {
            if (jugadorRemoto != null)
            {
                entradaRemota = new EntradaRed();
                jugadorRemoto.Configurar(TipoDispositivo.TecladoRaton, 0, 2);
                jugadorRemoto.AsignarEntrada(entradaRemota);
            }

            NetJuego.Estado = "Servidor: " + NetJuego.IpLocal() + ":" + ConfiguracionPartida.Puerto;
        }
        else
        {
            // El cliente no simula: solo pinta lo que manda el host
            if (jugadorLocal != null) jugadorLocal.PrepararComoRemoto();
            if (jugadorRemoto != null) jugadorRemoto.PrepararComoRemoto();

            if (jefe != null)
            {
                jefe.enabled = false;
                Rigidbody2D rb = jefe.GetComponent<Rigidbody2D>();
                if (rb != null) rb.bodyType = RigidbodyType2D.Kinematic;
            }

            entradaPropia = EntradaJugador.Crear(ConfiguracionPartida.Dispositivo1, 0);
            NetJuego.Estado = "Conectado a " + ConfiguracionPartida.Ip;
        }
    }

    void Update()
    {
        if (!activo) return;

        if (ConfiguracionPartida.EsHost)
        {
            string msg;
            while ((msg = NetJuego.Recibir()) != null) LeerEntradaCliente(msg);

            if (Time.time >= proximoEnvio)
            {
                proximoEnvio = Time.time + frecuencia;
                EnviarEstado();
            }
        }
        else
        {
            if (Time.time >= proximoEnvio)
            {
                proximoEnvio = Time.time + frecuencia;
                EnviarEntrada();
            }

            string msg = NetJuego.RecibirUltimo();
            if (msg != null) AplicarEstado(msg);
        }
    }

    void OnDestroy()
    {
        activo = false;
        if (ConfiguracionPartida.EsOnline) NetJuego.Detener();
    }

    // ------------------------------------------------------------- host
    void LeerEntradaCliente(string msg)
    {
        string[] p = msg.Split(';');
        if (p.Length < 8 || p[0] != "I" || entradaRemota == null) return;

        entradaRemota.h = NetJuego.Leer(p, 1);
        entradaRemota.salto = NetJuego.LeerBool(p, 2);
        entradaRemota.dash = NetJuego.LeerBool(p, 3);
        entradaRemota.fuego = NetJuego.LeerBool(p, 4);
        entradaRemota.aim = new Vector2(NetJuego.Leer(p, 5), NetJuego.Leer(p, 6));
    }

    void EnviarEstado()
    {
        if (!NetJuego.Conectado) return;

        Vector2 p1 = Vector2.zero, p2 = Vector2.zero, a1 = Vector2.right, a2 = Vector2.left;
        float v1 = 0f, v2 = 0f, hp1 = 0f, hp2 = 0f, f1 = 0f, f2 = 0f;
        float bx = 0f, by = 0f, bhp = 0f;

        if (jugadorLocal != null)
        {
            p1 = jugadorLocal.transform.position;
            v1 = jugadorLocal.VelocidadHorizontal;
            a1 = jugadorLocal.DireccionActual;
            f1 = jugadorLocal.spriteCuerpo != null && jugadorLocal.spriteCuerpo.flipX ? 1f : 0f;

            Salud s = jugadorLocal.GetComponent<Salud>();
            if (s != null) hp1 = s.VidaActual;
        }

        if (jugadorRemoto != null)
        {
            p2 = jugadorRemoto.transform.position;
            v2 = jugadorRemoto.VelocidadHorizontal;
            a2 = jugadorRemoto.DireccionActual;
            f2 = jugadorRemoto.spriteCuerpo != null && jugadorRemoto.spriteCuerpo.flipX ? 1f : 0f;

            Salud s = jugadorRemoto.GetComponent<Salud>();
            if (s != null) hp2 = s.VidaActual;
        }

        if (jefe != null)
        {
            bx = jefe.transform.position.x;
            by = jefe.transform.position.y;

            Salud sj = jefe.saludPropia != null ? jefe.saludPropia : jefe.GetComponent<Salud>();
            if (sj != null) bhp = sj.VidaActual;
        }

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append("W;");
        sb.Append(NetJuego.Num(p1.x)).Append(';').Append(NetJuego.Num(p1.y)).Append(';');
        sb.Append(NetJuego.Num(v1)).Append(';').Append(NetJuego.Num(hp1)).Append(';').Append(NetJuego.Num(f1)).Append(';');
        sb.Append(NetJuego.Num(a1.x)).Append(';').Append(NetJuego.Num(a1.y)).Append(';');
        sb.Append(NetJuego.Num(p2.x)).Append(';').Append(NetJuego.Num(p2.y)).Append(';');
        sb.Append(NetJuego.Num(v2)).Append(';').Append(NetJuego.Num(hp2)).Append(';').Append(NetJuego.Num(f2)).Append(';');
        sb.Append(NetJuego.Num(a2.x)).Append(';').Append(NetJuego.Num(a2.y)).Append(';');
        sb.Append(NetJuego.Num(bx)).Append(';').Append(NetJuego.Num(by)).Append(';').Append(NetJuego.Num(bhp));

        // Bolas de fuego vivas
        Bala[] balas = Object.FindObjectsByType<Bala>(FindObjectsSortMode.None);
        sb.Append(';').Append(balas.Length);

        for (int i = 0; i < balas.Length; i++)
        {
            Vector2 pos = balas[i].transform.position;
            float ang = balas[i].transform.eulerAngles.z;
            sb.Append(';').Append(NetJuego.Num(pos.x)).Append(';').Append(NetJuego.Num(pos.y)).Append(';').Append(NetJuego.Num(ang));
        }

        NetJuego.Enviar(sb.ToString());
    }

    // ------------------------------------------------------------- cliente
    void EnviarEntrada()
    {
        if (!NetJuego.Conectado || entradaPropia == null) return;

        float h = entradaPropia.Horizontal();
        float salto = entradaPropia.SaltoPulsado() ? 1f : 0f;
        float dash = entradaPropia.DashPulsado() ? 1f : 0f;
        float fuego = entradaPropia.DisparoMantenido() ? 1f : 0f;

        Vector3 origen = jugadorLocal != null ? jugadorLocal.transform.position : Vector3.zero;
        Vector2 facing = jugadorLocal != null && jugadorLocal.spriteCuerpo != null && jugadorLocal.spriteCuerpo.flipX
            ? Vector2.left : Vector2.right;

        Vector2 aim = entradaPropia.DireccionAim(origen, facing, jefe != null ? jefe.transform : null);

        string msg = "I;" + NetJuego.Num(h) + ";" + NetJuego.Num(salto) + ";" + NetJuego.Num(dash) + ";" +
                     NetJuego.Num(fuego) + ";" + NetJuego.Num(aim.x) + ";" + NetJuego.Num(aim.y);

        NetJuego.Enviar(msg);
    }

    void AplicarEstado(string msg)
    {
        string[] p = msg.Split(';');
        if (p.Length < 18 || p[0] != "W") return;

        // En el cliente el dragón de la escena es el jugador 2 del host
        if (jugadorLocal != null)
            jugadorLocal.AplicarEstadoRemoto(
                new Vector2(NetJuego.Leer(p, 8), NetJuego.Leer(p, 9)),
                NetJuego.Leer(p, 10),
                NetJuego.LeerBool(p, 12),
                new Vector2(NetJuego.Leer(p, 13), NetJuego.Leer(p, 14)));

        if (jugadorRemoto != null)
            jugadorRemoto.AplicarEstadoRemoto(
                new Vector2(NetJuego.Leer(p, 1), NetJuego.Leer(p, 2)),
                NetJuego.Leer(p, 3),
                NetJuego.LeerBool(p, 5),
                new Vector2(NetJuego.Leer(p, 6), NetJuego.Leer(p, 7)));

        if (jugadorLocal != null)
        {
            Salud s = jugadorLocal.GetComponent<Salud>();
            if (s != null) s.FijarVidaRemota(NetJuego.Leer(p, 11));
        }

        if (jugadorRemoto != null)
        {
            Salud s = jugadorRemoto.GetComponent<Salud>();
            if (s != null) s.FijarVidaRemota(NetJuego.Leer(p, 4));
        }

        if (jefe != null)
        {
            jefe.transform.position = new Vector3(NetJuego.Leer(p, 15), NetJuego.Leer(p, 16), jefe.transform.position.z);

            Salud sj = jefe.GetComponent<Salud>();
            if (sj != null) sj.FijarVidaRemota(NetJuego.Leer(p, 17));
        }

        AplicarBalas(p, 18);
    }

    void AplicarBalas(string[] p, int desde)
    {
        if (p.Length <= desde) return;

        int total = Mathf.RoundToInt(NetJuego.Leer(p, desde));

        while (balasCliente.Count < total)
            balasCliente.Add(CrearBalaVisual());

        for (int i = 0; i < balasCliente.Count; i++)
        {
            bool visible = i < total;
            balasCliente[i].gameObject.SetActive(visible);

            if (!visible) continue;

            int b = desde + 1 + i * 3;
            balasCliente[i].transform.position = new Vector3(NetJuego.Leer(p, b), NetJuego.Leer(p, b + 1), 0f);
            balasCliente[i].transform.rotation = Quaternion.Euler(0f, 0f, NetJuego.Leer(p, b + 2));
        }
    }

    SpriteRenderer CrearBalaVisual()
    {
        GameObject g = new GameObject("BalaRemota");

        SpriteRenderer sr = g.AddComponent<SpriteRenderer>();
        sr.sprite = spriteBala;
        sr.sortingOrder = 7;
        g.transform.localScale = Vector3.one * escalaBala;

        return sr;
    }
}
