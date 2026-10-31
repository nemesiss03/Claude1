using System.Collections;
using UnityEngine;

/// <summary>
/// Servicio de audio de "Sky Wars". Se crea solo al arrancar la partida
/// (no hay que colocarlo en la escena) y sobrevive al cambio de escena.
/// Todos los clips viven en Assets/Resources/Audio.
/// </summary>
public class AudioJuego : MonoBehaviour
{
    public static AudioJuego Instancia { get; private set; }

    [Header("Volumen")]
    [Range(0f, 1f)] public float volumenEfectos = 0.85f;
    [Range(0f, 1f)] public float volumenMusica = 0.30f;

    AudioClip disparo;
    AudioClip disparoJefe;
    AudioClip invocacion;
    AudioClip impactoJefe;
    AudioClip impactoEsbirro;
    AudioClip danioJugador;
    AudioClip dash;
    AudioClip curar;
    AudioClip explosionEsbirro;
    AudioClip explosionJefe;
    AudioClip musica;
    [Tooltip("Tema de batalla: Assets/Resources/Audio/musica_batalla.mp3")]
    AudioClip musicaBatalla;

    AudioSource[] fuentes;
    AudioSource fuenteMusica;
    Coroutine fundido;
    int siguiente;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Arrancar()
    {
        if (Instancia != null) return;

        GameObject go = new GameObject("AudioJuego");
        go.AddComponent<AudioJuego>();
    }

    void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }

        Instancia = this;
        DontDestroyOnLoad(gameObject);

        fuentes = new AudioSource[10];
        for (int i = 0; i < fuentes.Length; i++)
        {
            AudioSource s = gameObject.AddComponent<AudioSource>();
            s.playOnAwake = false;
            s.spatialBlend = 0f;
            s.volume = 1f;
            fuentes[i] = s;
        }

        fuenteMusica = gameObject.AddComponent<AudioSource>();
        fuenteMusica.playOnAwake = false;
        fuenteMusica.loop = true;
        fuenteMusica.spatialBlend = 0f;
        fuenteMusica.volume = volumenMusica;

        disparo = Resources.Load<AudioClip>("Audio/disparo");
        disparoJefe = Resources.Load<AudioClip>("Audio/disparo_jefe");
        invocacion = Resources.Load<AudioClip>("Audio/invocacion");
        impactoJefe = Resources.Load<AudioClip>("Audio/impacto_jefe");
        impactoEsbirro = Resources.Load<AudioClip>("Audio/impacto_esbirro");
        danioJugador = Resources.Load<AudioClip>("Audio/danio_jugador");
        dash = Resources.Load<AudioClip>("Audio/dash");
        curar = Resources.Load<AudioClip>("Audio/curar");
        explosionEsbirro = Resources.Load<AudioClip>("Audio/explosion_esbirro");
        explosionJefe = Resources.Load<AudioClip>("Audio/explosion_jefe");
        musica = Resources.Load<AudioClip>("Audio/musica_fondo");
        musicaBatalla = Resources.Load<AudioClip>("Audio/musica_batalla");

        if (musica != null)
        {
            fuenteMusica.clip = musica;
            fuenteMusica.Play();
        }
    }

    void OnDestroy()
    {
        if (Instancia == this) Instancia = null;
    }

    // ------------------------------------------------------------------ API
    public static void SonidoDisparo() { Tocar(Instancia != null ? Instancia.disparo : null, 0.55f, Variacion(0.94f, 1.08f)); }
    public static void SonidoDisparoJefe() { Tocar(Instancia != null ? Instancia.disparoJefe : null, 0.75f, Variacion(0.92f, 1.06f)); }
    public static void SonidoInvocacion() { Tocar(Instancia != null ? Instancia.invocacion : null, 0.70f, Variacion(0.96f, 1.06f)); }
    public static void SonidoImpactoJefe() { Tocar(Instancia != null ? Instancia.impactoJefe : null, 0.65f, Variacion(0.92f, 1.10f)); }
    public static void SonidoImpactoEsbirro() { Tocar(Instancia != null ? Instancia.impactoEsbirro : null, 0.55f, Variacion(0.90f, 1.14f)); }
    public static void SonidoDanioJugador() { Tocar(Instancia != null ? Instancia.danioJugador : null, 0.80f, Variacion(0.95f, 1.05f)); }
    public static void SonidoDash() { Tocar(Instancia != null ? Instancia.dash : null, 0.65f, Variacion(0.95f, 1.08f)); }
    public static void SonidoCuracion() { Tocar(Instancia != null ? Instancia.curar : null, 0.70f, Variacion(0.98f, 1.06f)); }
    public static void SonidoExplosionEsbirro() { Tocar(Instancia != null ? Instancia.explosionEsbirro : null, 0.75f, Variacion(0.90f, 1.12f)); }
    public static void SonidoExplosionJefe() { Tocar(Instancia != null ? Instancia.explosionJefe : null, 1.00f, Variacion(0.98f, 1.02f)); }

    public static void AjustarMusica(float volumen)
    {
        if (Instancia == null || Instancia.fuenteMusica == null) return;
        Instancia.volumenMusica = Mathf.Clamp01(volumen);
        Instancia.fuenteMusica.volume = Instancia.volumenMusica;
    }

    /// <summary>Tema de batalla (mp3 de Assets/Sound/Sountrack).</summary>
    public static void MusicaBatalla()
    {
        if (Instancia != null) Instancia.CambiarMusica(Instancia.musicaBatalla, 0.45f);
    }

    /// <summary>Vuelve al tema suave de fondo.</summary>
    public static void MusicaFondo()
    {
        if (Instancia != null) Instancia.CambiarMusica(Instancia.musica, 0.30f);
    }

    // -------------------------------------------------------------- interno
    void CambiarMusica(AudioClip clip, float volumen)
    {
        if (clip == null || fuenteMusica == null) return;

        if (fuenteMusica.clip == clip && fuenteMusica.isPlaying)
        {
            volumenMusica = volumen;
            fuenteMusica.volume = volumen;
            return;
        }

        if (fundido != null) StopCoroutine(fundido);
        fundido = StartCoroutine(FundirMusica(clip, volumen));
    }

    IEnumerator FundirMusica(AudioClip clip, float volumen)
    {
        const float bajo = 0.6f;
        const float sube = 0.9f;

        float inicial = fuenteMusica.volume;

        for (float t = 0f; t < bajo; t += Time.unscaledDeltaTime)
        {
            fuenteMusica.volume = Mathf.Lerp(inicial, 0f, t / bajo);
            yield return null;
        }

        fuenteMusica.volume = 0f;
        fuenteMusica.clip = clip;
        fuenteMusica.Play();

        for (float t = 0f; t < sube; t += Time.unscaledDeltaTime)
        {
            fuenteMusica.volume = Mathf.Lerp(0f, volumen, t / sube);
            yield return null;
        }

        fuenteMusica.volume = volumen;
        volumenMusica = volumen;
        fundido = null;
    }
    static float Variacion(float minimo, float maximo)
    {
        return Random.Range(minimo, maximo);
    }

    static void Tocar(AudioClip clip, float volumen, float tono)
    {
        if (Instancia == null || clip == null) return;
        Instancia.TocarInterno(clip, volumen, tono);
    }

    void TocarInterno(AudioClip clip, float volumen, float tono)
    {
        if (fuentes == null || fuentes.Length == 0) return;

        AudioSource s = fuentes[siguiente];
        siguiente = (siguiente + 1) % fuentes.Length;

        s.pitch = tono;
        s.PlayOneShot(clip, Mathf.Clamp01(volumen * volumenEfectos));
    }
}
