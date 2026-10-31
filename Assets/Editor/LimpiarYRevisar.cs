using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class LimpiarYRevisar
{
    public static void Execute()
    {
        StringBuilder sb = new StringBuilder();
        var escena = EditorSceneManager.GetActiveScene();

        // Quitar el Portal sobrante de la escena
        foreach (var root in escena.GetRootGameObjects())
        {
            if (root.name == "Portal")
            {
                Object.DestroyImmediate(root);
                sb.AppendLine("Eliminado Portal sobrante de la escena.");
                break;
            }
        }

        // Prefabs
        foreach (var p in new[] { "Assets/Prefab/Esbirro.prefab", "Assets/Prefab/Portal.prefab" })
        {
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(p);
            sb.AppendLine(p + " -> " + (go != null ? "OK" : "FALTA"));
        }

        // Player
        var jugador = GameObject.FindGameObjectWithTag("Player");
        if (jugador != null)
        {
            var sr = jugador.GetComponent<SpriteRenderer>();
            var anim = jugador.GetComponent<Animator>();
            var col = jugador.GetComponent<BoxCollider2D>();
            var pc = jugador.GetComponent<PlayerController>();
            sb.AppendLine("PLAYER pos=" + jugador.transform.position + " sprite=" + (sr && sr.sprite ? sr.sprite.name : "null") +
                          " sort=" + (sr ? sr.sortingOrder : 0) +
                          " animator=" + (anim && anim.runtimeAnimatorController ? anim.runtimeAnimatorController.name : "null") +
                          " col size=" + (col ? col.size.ToString() : "-") + " offset=" + (col ? col.offset.ToString() : "-") +
                          " pivot=" + (pc.cabeza && pc.cabeza.pivoteCabeza ? pc.cabeza.pivoteCabeza.localPosition.ToString() : "null") +
                          " bola=" + (pc.bolaFuegoPrefab ? pc.bolaFuegoPrefab.name : "null") +
                          " cargas=" + pc.cargasMaximas + " enfriamiento=" + pc.tiempoEnfriamiento);
            sb.AppendLine("   SpriteRenderer bounds=" + (sr ? sr.bounds.ToString() : "-"));
        }

        var plataforma = GameObject.Find("Plataforma");
        if (plataforma != null)
        {
            var vis = plataforma.transform.Find("Visual");
            var sr = vis ? vis.GetComponent<SpriteRenderer>() : null;
            sb.AppendLine("PLATAFORMA pos=" + plataforma.transform.position + " visual.pos=" + (vis ? vis.localPosition.ToString() : "-") +
                          " visual.scale=" + (vis ? vis.localScale.ToString() : "-") +
                          " sprite=" + (sr && sr.sprite ? sr.sprite.name : "null") +
                          " bounds=" + (sr ? sr.bounds.ToString() : "-"));
        }

        var jefe = GameObject.Find("Boss");
        if (jefe != null)
        {
            var jf = jefe.GetComponent<JefeFlotante>();
            var sr = jefe.GetComponent<SpriteRenderer>();
            sb.AppendLine("JEFE pos=" + jefe.transform.position + " sprite=" + (sr && sr.sprite ? sr.sprite.name : "null") +
                          " vida=" + (jefe.GetComponent<Salud>() ? jefe.GetComponent<Salud>().vidaMaxima.ToString() : "-") +
                          " objetivo=" + (jf && jf.objetivo ? jf.objetivo.name : "null") +
                          " bala=" + (jf && jf.balaPrefab ? jf.balaPrefab.name : "null") +
                          " portal=" + (jf && jf.portalPrefab ? jf.portalPrefab.name : "null") +
                          " plataforma=" + (jf && jf.plataforma ? jf.plataforma.name : "null"));
        }

        var cam = Camera.main;
        if (cam != null)
        {
            var cielo = cam.transform.Find("Cielo");
            var sr = cielo ? cielo.GetComponent<SpriteRenderer>() : null;
            sb.AppendLine("CAMARA pos=" + cam.transform.position + " cielo=" + (sr && sr.sprite ? sr.sprite.name : "null") +
                          " cielo.scale=" + (cielo ? cielo.localScale.ToString() : "-") +
                          " cielo.pos=" + (cielo ? cielo.localPosition.ToString() : "-"));
        }

        var hud = Object.FindFirstObjectByType<HUDJuego>();
        if (hud != null)
        {
            sb.AppendLine("HUD saludJugador=" + (hud.saludJugador ? hud.saludJugador.name : "null") +
                          " saludJefe=" + (hud.saludJefe ? hud.saludJefe.name : "null") +
                          " barraJugador=" + (hud.barraJugador ? "OK" : "null") +
                          " barraJefe=" + (hud.barraJefe ? "OK" : "null") +
                          " textoMunicion=" + (hud.textoMunicion ? "OK" : "null") +
                          " panelFinal=" + (hud.panelFinal ? "OK" : "null"));
        }

        EditorSceneManager.MarkSceneDirty(escena);
        EditorSceneManager.SaveScene(escena);

        Debug.Log(sb.ToString());
    }
}
