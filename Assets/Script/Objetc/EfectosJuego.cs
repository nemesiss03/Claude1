using UnityEngine;

/// <summary>
/// Efectos visuales reutilizables: explosiones de esbirros y del jefe, e impactos.
/// Los prefabs viven en Assets/Resources/Prefabs para poder cargarlos en tiempo de ejecución.
/// </summary>
public static class EfectosJuego
{
    static GameObject prefabExplosion;
    static GameObject prefabFogonazo;
    static bool buscados;

    public static readonly Color TinteEsbirro = new Color(1f, 0.72f, 0.30f, 1f);
    public static readonly Color TinteJefe = new Color(1f, 0.48f, 0.22f, 1f);
    public static readonly Color TinteImpacto = new Color(1f, 0.92f, 0.65f, 1f);

    /// <summary>Explosión de un esbirro (o de un proyectil que impacta).</summary>
    public static void Explosion(Vector3 posicion, float escala, Color tinte)
    {
        GameObject prefab = PrefabExplosion();
        if (prefab == null) return;

        GameObject g = Object.Instantiate(prefab, posicion, Quaternion.identity);

        ExplosionSimple e = g.GetComponent<ExplosionSimple>();
        if (e != null) e.Aplicar(escala, tinte);
    }

    /// <summary>Destrucción de un esbirro.</summary>
    public static void ExplosionEsbirro(Vector3 posicion)
    {
        Explosion(posicion, 1.1f, TinteEsbirro);
    }

    /// <summary>Estallido grande del jefe.</summary>
    public static void ExplosionJefe(Vector3 posicion, float escala)
    {
        Explosion(posicion, escala, TinteJefe);
    }

    /// <summary>Chispa corta en el punto de impacto de un proyectil.</summary>
    public static void Impacto(Vector3 posicion, float escala)
    {
        Explosion(posicion, escala, TinteImpacto);
    }

    /// <summary>Fogonazo de la boca del arma al disparar.</summary>
    public static void Fogonazo(Vector3 posicion, Vector2 direccion, float escala)
    {
        GameObject prefab = PrefabFogonazo();
        if (prefab == null) return;

        float angulo = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;
        GameObject g = Object.Instantiate(prefab, posicion, Quaternion.Euler(0f, 0f, angulo));

        FogonazoSimple f = g.GetComponent<FogonazoSimple>();
        if (f != null) f.Aplicar(escala, 0.07f);
    }

    static GameObject PrefabExplosion()
    {
        Cargar();
        return prefabExplosion;
    }

    static GameObject PrefabFogonazo()
    {
        Cargar();
        return prefabFogonazo;
    }

    static void Cargar()
    {
        if (buscados) return;

        buscados = true;
        prefabExplosion = Resources.Load<GameObject>("Prefabs/Explosion");
        prefabFogonazo = Resources.Load<GameObject>("Prefabs/Fogonazo");
    }
}
