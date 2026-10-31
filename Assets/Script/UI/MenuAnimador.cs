using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Animación decorativa del menú: nubes que cruzan la pantalla, el título que
/// respira y destellos que se encienden y apagan.
/// </summary>
public class MenuAnimador : MonoBehaviour
{
    [Header("Nubes")]
    public RectTransform[] nubes;
    public float velocidadNubes = 18f;
    public float anchoRecorrido = 1900f;

    [Header("Título")]
    public RectTransform titulo;
    public Graphic[] destellos;
    public float amplitudTitulo = 6f;
    public float velocidadTitulo = 1.4f;
    public float amplitudBrillo = 0.35f;

    [Header("Dragón decorativo")]
    public RectTransform dragon;
    public float amplitudDragon = 14f;

    float tiempo;
    Vector2 tituloBase;
    Vector2 dragonBase;
    float[] faseNubes;

    void Start()
    {
        if (titulo != null) tituloBase = titulo.anchoredPosition;
        if (dragon != null) dragonBase = dragon.anchoredPosition;

        faseNubes = new float[nubes != null ? nubes.Length : 0];
        for (int i = 0; i < faseNubes.Length; i++) faseNubes[i] = Random.Range(0f, Mathf.PI * 2f);
    }

    void Update()
    {
        tiempo += Time.deltaTime;

        if (nubes != null)
        {
            for (int i = 0; i < nubes.Length; i++)
            {
                RectTransform n = nubes[i];
                if (n == null) continue;

                Vector2 p = n.anchoredPosition;
                p.x += velocidadNubes * (0.55f + i * 0.22f) * Time.deltaTime;

                if (p.x > anchoRecorrido * 0.5f) p.x = -anchoRecorrido * 0.5f;

                p.y += Mathf.Sin(tiempo * 0.7f + faseNubes[i]) * 2f * Time.deltaTime;
                n.anchoredPosition = p;
            }
        }

        if (titulo != null)
        {
            titulo.anchoredPosition = tituloBase + new Vector2(0f, Mathf.Sin(tiempo * velocidadTitulo) * amplitudTitulo);
        }

        if (dragon != null)
        {
            dragon.anchoredPosition = dragonBase + new Vector2(0f, Mathf.Sin(tiempo * 1.1f) * amplitudDragon);
            dragon.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(tiempo * 0.9f) * 3f);
        }

        if (destellos != null)
        {
            for (int i = 0; i < destellos.Length; i++)
            {
                if (destellos[i] == null) continue;

                Color c = destellos[i].color;
                c.a = 0.45f + Mathf.Abs(Mathf.Sin(tiempo * 1.6f + i)) * amplitudBrillo;
                destellos[i].color = c;
            }
        }
    }
}
