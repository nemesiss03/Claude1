using System.Collections;
using UnityEngine;

/// <summary>
/// Plataforma del cielo. Cuando el jefe baja al 50 % de vida se agrieta, tiembla
/// y se destruye: el combate pasa a caída libre.
/// </summary>
public class PlataformaCelestial : MonoBehaviour
{
    [Header("Referencias")]
    public Transform visual;
    public Collider2D colisionador;

    [Header("Destrucción")]
    public bool destruida;
    public float tiempoTemblor = 0.6f;
    public float fuerzaTemblor = 0.12f;
    public int numeroEscombros = 8;
    public float escalaEscombro = 0.45f;
    public Sprite spriteEscombro;

    public void Destruir()
    {
        if (destruida) return;

        destruida = true;

        if (colisionador != null) colisionador.enabled = false;

        StartCoroutine(Desmoronar());
    }

    IEnumerator Desmoronar()
    {
        Vector3 baseLocal = visual != null ? visual.localPosition : Vector3.zero;

        float t = 0f;
        while (t < tiempoTemblor)
        {
            t += Time.deltaTime;

            if (visual != null)
            {
                Vector3 ruido = (Vector3)(Random.insideUnitCircle * fuerzaTemblor);
                visual.localPosition = baseLocal + ruido;
            }

            yield return null;
        }

        if (visual != null) visual.localPosition = baseLocal;

        CrearEscombros();

        if (visual != null) visual.gameObject.SetActive(false);
    }

    void CrearEscombros()
    {
        if (spriteEscombro == null) return;

        float ancho = 12f;
        var srPropio = GetComponent<SpriteRenderer>();
        if (srPropio != null && srPropio.sprite != null) ancho = srPropio.sprite.bounds.size.x * transform.localScale.x;

        for (int i = 0; i < numeroEscombros; i++)
        {
            GameObject g = new GameObject("Escombro");
            g.transform.position = new Vector3(
                transform.position.x + Random.Range(-ancho * 0.45f, ancho * 0.45f),
                transform.position.y + Random.Range(-0.2f, 0.6f),
                0f);
            g.transform.localScale = Vector3.one * escalaEscombro * Random.Range(0.6f, 1.4f);

            var sr = g.AddComponent<SpriteRenderer>();
            sr.sprite = spriteEscombro;
            sr.color = new Color(0.42f, 0.46f, 0.56f, 1f);
            sr.sortingOrder = -1;

            var rb = g.AddComponent<Rigidbody2D>();
            rb.gravityScale = 1.6f;
            rb.linearVelocity = new Vector2(Random.Range(-2.5f, 2.5f), Random.Range(0.5f, 3f));
            rb.angularVelocity = Random.Range(-220f, 220f);

            var col = g.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1f, 1f);

            Destroy(g, 4f);
        }
    }
}
