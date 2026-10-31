using UnityEngine;

/// <summary>
/// Fogonazo de boca de cañón: aparece, se estira un poco y desaparece en unas centésimas.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class FogonazoSimple : MonoBehaviour
{
    public Sprite sprite;
    public float duracion = 0.07f;
    public float escala = 1f;
    public int ordenDibujo = 25;

    SpriteRenderer sr;
    float tiempo;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();

        sr.sprite = sprite;
        sr.sortingOrder = ordenDibujo;
        sr.color = Color.white;
        transform.localScale = Vector3.one * escala;
    }

    public void Aplicar(float escalaNueva, float duracionNueva)
    {
        escala = Mathf.Max(0.01f, escalaNueva);
        duracion = Mathf.Max(0.01f, duracionNueva);
        if (sr != null) sr.color = Color.white;
        transform.localScale = Vector3.one * escala;
    }

    void Update()
    {
        tiempo += Time.deltaTime;

        float k = Mathf.Clamp01(tiempo / duracion);
        if (k >= 1f)
        {
            Destroy(gameObject);
            return;
        }

        transform.localScale = Vector3.one * (escala * Mathf.Lerp(1f, 1.5f, k));

        if (sr != null)
        {
            Color c = Color.white;
            c.a = 1f - k;
            sr.color = c;
        }
    }
}
