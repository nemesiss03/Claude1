using UnityEngine;

/// <summary>
/// Arma del dragón: pistola que gira hacia el punto de apuntado. El cañón (punta)
/// es el origen real de las bolas de fuego.
/// El volteo se hace con la escala X del pivote (nunca con flipX) para que la
/// dirección del cañón siga siendo correcta al mirar a la izquierda.
/// </summary>
public class ArmaDragon : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Pivote que gira con el jugador. Su escala X se invierte al mirar a la izquierda.")]
    public Transform pivote;

    [Tooltip("Sprite de la pistola (se le invierte la escala Y al apuntar hacia abajo).")]
    public Transform pistola;

    [Tooltip("Punto de la boca del cañón.")]
    public Transform punta;

    [Header("Movimiento")]
    public float anguloMaximo = 89f;
    public float velocidadGiro = 1500f;

    /// <summary>Dirección del cañón en coordenadas de mundo.</summary>
    public Vector2 DireccionCanon
    {
        get
        {
            if (punta == null) return Vector2.right;

            // Transform.right ignora la escala negativa del pivote, así que al mirar
            // a la izquierda devolvía la dirección CONTRARIA al cañón (el disparo
            // salía por detrás del arma). La matriz sí incluye el espejo.
            return DireccionDeEje(punta);
        }
    }

    /// <summary>Dirección del eje +X de un nodo, con la escala negativa incluida.</summary>
    public static Vector2 DireccionDeEje(Transform nodo)
    {
        if (nodo == null) return Vector2.right;

        Vector3 d = nodo.localToWorldMatrix.MultiplyVector(Vector3.right);
        d.z = 0f;

        return d.sqrMagnitude > 0.000001f ? (Vector2)d.normalized : Vector2.right;
    }

    public Vector3 PosicionPunta
    {
        get { return punta != null ? punta.position : transform.position; }
    }

    float anguloActual;
    bool mirandoIzquierda;

    void Awake()
    {
        if (pivote == null) pivote = transform;
    }

    /// <summary>El cuerpo se ha volteado: el arma se refleja con él.</summary>
    public void FijarLado(bool izquierda)
    {
        if (mirandoIzquierda == izquierda) return;

        mirandoIzquierda = izquierda;

        if (pivote != null)
        {
            Vector3 s = pivote.localScale;
            s.x = izquierda ? -Mathf.Abs(s.x) : Mathf.Abs(s.x);
            pivote.localScale = s;
        }
    }

    /// <summary>Gira el arma hacia la dirección indicada (mundo).</summary>
    public void Apuntar(Vector2 direccionMundo)
    {
        if (pivote == null) return;

        if (direccionMundo.sqrMagnitude < 0.0001f) direccionMundo = Vector2.right;

        float lado = mirandoIzquierda ? -1f : 1f;
        float objetivo = Mathf.Atan2(direccionMundo.y, direccionMundo.x * lado) * Mathf.Rad2Deg;

        objetivo = Mathf.Clamp(objetivo, -anguloMaximo, anguloMaximo);

        anguloActual = Mathf.MoveTowardsAngle(anguloActual, objetivo, velocidadGiro * Time.deltaTime);

        float local = mirandoIzquierda ? -anguloActual : anguloActual;
        pivote.localRotation = Quaternion.Euler(0f, 0f, local);

        // Al apuntar muy hacia abajo la pistola se refleja en vertical para no quedar boca abajo
        if (pistola != null)
        {
            Vector3 s = pistola.localScale;
            float deseado = Mathf.Abs(s.y) * (Mathf.Abs(anguloActual) > 90f ? -1f : 1f);
            if (!Mathf.Approximately(s.y, deseado))
            {
                s.y = deseado;
                pistola.localScale = s;
            }
        }
    }

    /// <summary>Deja el arma apuntando al frente (muerte, cinemáticas).</summary>
    public void Relajar()
    {
        if (pivote == null) return;

        anguloActual = Mathf.MoveTowards(anguloActual, 0f, velocidadGiro * Time.deltaTime);
        float local = mirandoIzquierda ? -anguloActual : anguloActual;
        pivote.localRotation = Quaternion.Euler(0f, 0f, local);
    }
}
