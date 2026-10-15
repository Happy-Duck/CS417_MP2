using UnityEngine;
using UnityEditor;
using System.IO;

public static class MergeOpacity
{
    [MenuItem("Tools/Merge Albedo + Opacity")]
    static void Merge()
    {
        var sel = Selection.objects;
        Texture2D albedo = null, opacity = null;
        foreach (var o in sel)
        {
            if (o is Texture2D t)
            {
                if (t.name.ToLower().Contains("opaci")) opacity = t;
                else albedo = t;
            }
        }
        if (!albedo || !opacity) { Debug.LogError("Select the albedo and opacity textures"); return; }

        var result = new Texture2D(albedo.width, albedo.height, TextureFormat.RGBA32, false);
        for (int y = 0; y < albedo.height; y++)
            for (int x = 0; x < albedo.width; x++)
            {
                Color c = albedo.GetPixel(x, y);
                float u = (float)x / albedo.width, v = (float)y / albedo.height;
                c.a = opacity.GetPixelBilinear(u, v).r;
                result.SetPixel(x, y, c);
            }
        string path = Path.GetDirectoryName(AssetDatabase.GetAssetPath(albedo)) + "/" + albedo.name + "_Alpha.png";
        File.WriteAllBytes(path, result.EncodeToPNG());
        AssetDatabase.Refresh();
    }
}