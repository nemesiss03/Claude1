using UnityEngine;
using UnityEditor;
using System.Text;

public class InspectAssets2
{
    public static void Execute()
    {
        StringBuilder sb = new StringBuilder();

        // Boss sheet: dump index -> grid cell
        sb.AppendLine("=== BOSS SHEET ===");
        var objs = AssetDatabase.LoadAllAssetsAtPath("Assets/Imagen/Enemy/Boss/Character_sheet.png");
        foreach (var a in objs)
        {
            if (a is Sprite s)
            {
                int col = Mathf.RoundToInt(s.rect.x / 100f);
                int row = Mathf.RoundToInt((1000f - s.rect.y - s.rect.height) / 100f);
                sb.AppendLine(s.name + " row=" + row + " col=" + col + " size=" + s.rect.width + "x" + s.rect.height);
            }
        }

        sb.AppendLine("=== OTHER SHEETS ===");
        string[] paths = {
            "Assets/Imagen/Enemy/Enemy01_RP1 (32x16).png",
            "Assets/Imagen/Enemy/Enemy03_Idle_RP1 (32x36).png",
            "Assets/Imagen/Enemy/Enemy03_Walk_RP1 (32x36).png",
            "Assets/Imagen/Bullet.png",
            "Assets/Imagen/BulletBoss.png",
        };
        foreach (var p in paths)
        {
            var ti = AssetImporter.GetAtPath(p) as TextureImporter;
            if (ti == null) { sb.AppendLine(p + " NO IMPORTER"); continue; }
            sb.AppendLine(p + " type=" + ti.textureType + " mode=" + ti.spriteImportMode + " ppu=" + ti.spritePixelsPerUnit);
            var sp = AssetDatabase.LoadAllAssetsAtPath(p);
            int n = 0;
            foreach (var a in sp) if (a is Sprite s) { n++; if (n <= 20) sb.AppendLine("   " + s.name + " " + s.rect); }
            sb.AppendLine("   count=" + n);
        }

        // Prefabs
        sb.AppendLine("=== PREFABS ===");
        foreach (var p in new[] { "Assets/Prefab/Circle (1).prefab", "Assets/Prefab/Triangle.prefab" })
        {
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(p);
            if (go == null) { sb.AppendLine(p + " NULL"); continue; }
            sb.AppendLine(p + " scale=" + go.transform.localScale);
            foreach (var c in go.GetComponents<Component>())
                sb.AppendLine("   " + c.GetType().Name);
            var sr = go.GetComponent<SpriteRenderer>();
            if (sr != null) sb.AppendLine("   sprite=" + (sr.sprite ? sr.sprite.name : "null"));
        }

        // Scene objects of interest
        sb.AppendLine("=== SCENE ===");
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        foreach (var root in scene.GetRootGameObjects())
        {
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
            {
                var sr = t.GetComponent<SpriteRenderer>();
                string sprite = sr && sr.sprite ? sr.sprite.name : "-";
                var mg = t.GetComponent<SpriteRenderer>();
                sb.AppendLine(t.name + " | " + GetPath(t) + " | sprite=" + sprite +
                    (mg ? " sort=" + mg.sortingOrder : "") +
                    " | scale=" + t.localScale + " | pos=" + t.localPosition);
            }
        }

        // Pistol child
        var pistol = scene.GetRootGameObjects()[0].transform.Find("Player/ArmaPivot/Pistol 1 (72 ppi)");
        if (pistol != null) sb.AppendLine("PISTOL scale=" + pistol.localScale + " pos=" + pistol.localPosition);
        var punta = scene.GetRootGameObjects()[0].transform.Find("Player/ArmaPivot/Pistol 1 (72 ppi)/Punta");
        if (punta != null) sb.AppendLine("PUNTA pos=" + punta.localPosition);
        var arma = scene.GetRootGameObjects()[0].transform.Find("Player/ArmaPivot");
        if (arma != null) sb.AppendLine("ARMA pos=" + arma.localPosition);

        Debug.Log(sb.ToString());
    }

    static string GetPath(Transform t)
    {
        string p = t.name;
        while (t.parent != null) { t = t.parent; p = t.name + "/" + p; }
        return p;
    }
}
