using System.Collections.Generic;
using UnityEngine;

/// <summary>Registro de todos los dragones vivos de la partida (1 o 2 jugadores).</summary>
public static class Jugadores
{
    public static readonly List<PlayerController> Todos = new List<PlayerController>();

    public static void Registrar(PlayerController j)
    {
        if (j != null && !Todos.Contains(j)) Todos.Add(j);
    }

    public static void Quitar(PlayerController j)
    {
        if (j != null) Todos.Remove(j);
    }

    /// <summary>Jugador vivo más cercano a un punto (el jefe y los esbirros lo usan).</summary>
    public static Transform MasCercano(Vector3 desde)
    {
        Transform mejor = null;
        float mejorDist = float.MaxValue;

        for (int i = 0; i < Todos.Count; i++)
        {
            PlayerController j = Todos[i];
            if (j == null || j.Muerto) continue;

            float d = (j.transform.position - desde).sqrMagnitude;
            if (d < mejorDist)
            {
                mejorDist = d;
                mejor = j.transform;
            }
        }

        return mejor;
    }

    /// <summary>Todos los jugadores siguen vivos.</summary>
    public static bool AlgunoVivo
    {
        get
        {
            for (int i = 0; i < Todos.Count; i++)
                if (Todos[i] != null && !Todos[i].Muerto) return true;
            return false;
        }
    }
}
