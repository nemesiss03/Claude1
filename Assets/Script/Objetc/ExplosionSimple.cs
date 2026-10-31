using UnityEngine;

/// <summary>
/// Reproduce una explosión fotograma a fotograma y se destruye al terminar.
/// Se usa para la destrucción de esbirros, del jefe y para los impactos.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class ExplosionSimple : MonoBehaviour
{
    public Sprite[] fotogramas;
    public float fps = 32f;
    public float escala = 1f;
    public Color tinte = Color.white;
    public int ordenDibujo = 30;

    SpriteRenderer sr;
    float tiempo;
    int actual = -1;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();

        sr.sortingOrder = ordenDibujo;
        sr.color = tinte;
        transform.localScale = Vector3.one * escala;
    }

    /// <summary>Se llama justo después de instanciar para fijar tamaño y color.</summary>
    public void Aplicar(float escalaNueva, Color tinteNuevo)
    {
        escala = Mathf.Max(0.01f, escalaNueva);
        tinte = tinteNuevo;
        transform.localScale = Vector3.one * escala;

        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = tinte;
    }

    void Update()
    {
        if (fotogramas == null || fotogramas.Length == 0)
        {
            Destroy(gameObject);
            return;
        }

        tiempo += Time.deltaTime;

        int indice = Mathf.FloorToInt(tiempo * fps);
        if (indice >= fotogramas.Length)
        {
            Destroy(gameObject);
            return;
        }

        if (indice != actual)
        {
            actual = indice;

            if (sr == null) sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.sprite = fotogramas[indice];
        }

        float avance = (float)indice / fotogramas.Length;

        transform.localScale = Vector3.one * (escala * Mathf.Lerp(0.65f, 1.35f, avance));

        if (sr != null)
        {
            Color c = tinte;
            c.a = tinte.a * Mathf.Lerp(1f, 0f, Mathf.InverseLerp(0.55f, 1f, avance));
            sr.color = c;
        }
    }
}
