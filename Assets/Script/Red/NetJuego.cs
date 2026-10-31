using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

/// <summary>
/// Sesión online sencilla sobre TCP. El host abre el servidor y simula el mundo;
/// el cliente envía su entrada y recibe el estado del mundo.
/// Los mensajes son líneas de texto separadas por ';'.
/// </summary>
public static class NetJuego
{
    public const int PuertoPorDefecto = 7777;

    public static bool EsHost { get; private set; }
    public static bool Conectado { get; private set; }
    public static string Estado = "Sin conexión";
    public static string UltimoError = string.Empty;

    static TcpListener servidor;
    static TcpClient cliente;
    static NetworkStream flujo;
    static StreamWriter escritor;
    static Thread hiloEscucha;
    static Thread hiloAceptar;

    static volatile bool activo;

    static readonly Queue<string> entrada = new Queue<string>();
    static readonly object candadoEntrada = new object();
    static readonly object candadoEscritura = new object();

    // ---------------------------------------------------------------- arranque
    public static void IniciarHost(int puerto)
    {
        Detener();

        EsHost = true;
        Conectado = false;
        UltimoError = string.Empty;
        Estado = "Abriendo servidor...";
        activo = true;

        hiloAceptar = new Thread(() => AceptarHilo(puerto));
        hiloAceptar.IsBackground = true;
        hiloAceptar.Start();
    }

    public static void IniciarCliente(string ip, int puerto)
    {
        Detener();

        EsHost = false;
        Conectado = false;
        UltimoError = string.Empty;
        Estado = "Conectando a " + ip + "...";
        activo = true;

        hiloAceptar = new Thread(() => ConectarHilo(ip, puerto));
        hiloAceptar.IsBackground = true;
        hiloAceptar.Start();
    }

    public static void Detener()
    {
        activo = false;
        Conectado = false;

        try { if (escritor != null) escritor.Dispose(); } catch { }
        try { if (flujo != null) flujo.Dispose(); } catch { }
        try { if (cliente != null) cliente.Close(); } catch { }
        try { if (servidor != null) servidor.Stop(); } catch { }

        escritor = null;
        flujo = null;
        cliente = null;
        servidor = null;

        lock (candadoEntrada) entrada.Clear();

        Estado = "Sin conexión";
        EsHost = false;
    }

    // ---------------------------------------------------------------- hilos
    static void AceptarHilo(int puerto)
    {
        try
        {
            servidor = new TcpListener(IPAddress.Any, puerto);
            servidor.Start();
            Estado = "Servidor abierto · esperando jugador";

            TcpClient c = servidor.AcceptTcpClient();
            cliente = c;
            cliente.NoDelay = true;
            flujo = cliente.GetStream();
            escritor = new StreamWriter(flujo) { AutoFlush = true };

            Conectado = true;
            Estado = "Jugador conectado";

            hiloEscucha = new Thread(LeerHilo) { IsBackground = true };
            hiloEscucha.Start();
        }
        catch (Exception e)
        {
            UltimoError = e.Message;
            Estado = "Error: " + e.Message;
            activo = false;
        }
    }

    static void ConectarHilo(string ip, int puerto)
    {
        try
        {
            cliente = new TcpClient();
            cliente.Connect(ip, puerto);
            cliente.NoDelay = true;
            flujo = cliente.GetStream();
            escritor = new StreamWriter(flujo) { AutoFlush = true };

            Conectado = true;
            Estado = "Conectado a " + ip;

            hiloEscucha = new Thread(LeerHilo) { IsBackground = true };
            hiloEscucha.Start();
        }
        catch (Exception e)
        {
            UltimoError = e.Message;
            Estado = "No se pudo conectar";
            activo = false;
        }
    }

    static void LeerHilo()
    {
        try
        {
            using (var lector = new StreamReader(flujo))
            {
                string linea;
                while (activo && (linea = lector.ReadLine()) != null)
                {
                    lock (candadoEntrada) entrada.Enqueue(linea);
                }
            }
        }
        catch { }

        if (activo)
        {
            Conectado = false;
            Estado = "Conexión perdida";
        }
    }

    // ---------------------------------------------------------------- mensajes
    public static void Enviar(string linea)
    {
        if (!Conectado || escritor == null) return;

        lock (candadoEscritura)
        {
            try { escritor.WriteLine(linea); }
            catch
            {
                Conectado = false;
                Estado = "Conexión perdida";
            }
        }
    }

    /// <summary>Saca el último mensaje recibido (o null).</summary>
    public static string Recibir()
    {
        lock (candadoEntrada)
        {
            return entrada.Count > 0 ? entrada.Dequeue() : null;
        }
    }

    /// <summary>Descarta los mensajes pendientes y devuelve solo el último.</summary>
    public static string RecibirUltimo()
    {
        string ultimo = null;

        lock (candadoEntrada)
        {
            while (entrada.Count > 0) ultimo = entrada.Dequeue();
        }

        return ultimo;
    }

    public static string IpLocal()
    {
        try
        {
            IPAddress[] todas = Dns.GetHostAddresses(Dns.GetHostName());
            for (int i = 0; i < todas.Length; i++)
            {
                if (todas[i].AddressFamily == AddressFamily.InterNetwork) return todas[i].ToString();
            }
        }
        catch { }

        return "127.0.0.1";
    }

    public static string Num(float v)
    {
        return v.ToString("0.###", CultureInfo.InvariantCulture);
    }

    public static float Leer(string[] partes, int indice)
    {
        if (indice < 0 || indice >= partes.Length) return 0f;

        float v;
        if (float.TryParse(partes[indice], NumberStyles.Float, CultureInfo.InvariantCulture, out v)) return v;
        return 0f;
    }

    public static bool LeerBool(string[] partes, int indice)
    {
        return Leer(partes, indice) > 0.5f;
    }
}

/// <summary>Entrada de un jugador remoto: la rellena la red en el host.</summary>
public class EntradaRed : EntradaJugador
{
    public float h;
    public bool salto;
    public bool dash;
    public bool fuego;
    public Vector2 aim = Vector2.right;

    public override float Horizontal() { return h; }

    public override bool SaltoPulsado()
    {
        bool s = salto;
        salto = false;
        return s;
    }

    public override bool DashPulsado()
    {
        bool d = dash;
        dash = false;
        return d;
    }

    public override bool DisparoMantenido() { return fuego; }

    public override bool EnfriamientoPulsado() { return false; }

    public override bool TieneAimManual { get { return true; } }

    public override Vector2 DireccionAim(Vector3 origen, Vector2 facing, Transform objetivoAuto)
    {
        return aim.sqrMagnitude < 0.0001f ? facing : aim;
    }
}
