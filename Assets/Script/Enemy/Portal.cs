using System.Collections;
using UnityEngine;

/// <summary>
/// Portal que aparece, invoca esbirros a intervalos y se cierra.
/// </summary>
public class Portal : MonoBehaviour
{
    [Header("Invocación")]
    public GameObject esbirroPrefab;
    public Transform objetivo;
    public float duracion = 5f;
    public float intervalo = 1.5f;
    public int maxEsbirros = 1;

    [Header("Aspecto")]
    public float escalaFinal = 1f;
    public float tiempoApertura = 0.35f;

    float tiempo;
    float proximo;
    int creados;
    bool cerrando;

    void Start()
    {
        transform.localScale = Vector3.zero;
        if (objetivo == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) objetivo = p.transform;
        }
    }

    void Update()
    {
        if (cerrando) return;

        tiempo += Time.deltaTime;

        float apertura = Mathf.Clamp01(tiempo / Mathf.Max(0.01f, tiempoApertura));
        float pulso = 1f + 0.06f * Mathf.Sin(tiempo * 7f);
        transform.localScale = Vector3.one * (escalaFinal * apertura * pulso);

        if (apertura >= 1f && creados < maxEsbirros && Time.time >= proximo)
        {
            proximo = Time.time + intervalo;
            CrearEsbirro();
        }

        if (tiempo >= duracion) StartCoroutine(Cerrar());
    }

    void CrearEsbirro()
    {
        if (esbirroPrefab == null) return;

        creados++;

        Vector3 pos = transform.position + (Vector3)(Random.insideUnitCircle * 0.35f);
        GameObject g = Instantiate(esbirroPrefab, pos, Quaternion.identity);

        Esbirro esbirro = g.GetComponent<Esbirro>();
        if (esbirro != null && objetivo != null) esbirro.objetivo = objetivo;

        AudioJuego.SonidoInvocacion();
        EfectosJuego.Explosion(pos, 0.9f, new Color(0.75f, 0.45f, 1f, 1f));
    }

    IEnumerator Cerrar()
    {
        cerrando = true;

        Vector3 inicial = transform.localScale;
        float t = 0f;

        while (t < 0.28f)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(inicial, Vector3.zero, t / 0.28f);
            yield return null;
        }

        Destroy(gameObject);
    }
}
