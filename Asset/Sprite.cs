using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Sprite : EditorWindow
{
    [MenuItem("MaimaiRE/AssetRipper Patch/Sprite Patch")]
    private static void FixTextures()
    {
        // https://github.com/AssetRipper/AssetRipper/issues/779
        // Snippet from a kind stranger on the internet. Thx^^
        AssetDatabase.StartAssetEditing();
        try
        {
            string[] guids = AssetDatabase.FindAssets("t:Texture2D");
            int totalTextures = guids.Length;
            int processedTextures = 0;
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;

                if (importer != null && importer.textureType == TextureImporterType.Sprite)
                {
                    if (importer.spriteImportMode == SpriteImportMode.Single)
                    {
                        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);

                        if (texture != null)
                        {
                            TextureImporterSettings textureSettings = new TextureImporterSettings();
                            importer.ReadTextureSettings(textureSettings);
                            textureSettings.spriteMeshType = SpriteMeshType.FullRect;
                            if (texture.width == texture.height)
                            {
                                textureSettings.spritePivot = new Vector2(0.5f, 0.5f);
                            }

                            importer.SetTextureSettings(textureSettings);
                            importer.SaveAndReimport();
                        }
                    }
                }
                processedTextures++;
                EditorUtility.DisplayProgressBar("Fixing Textures", $"Processing {path}", (float)processedTextures / totalTextures);
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }
        Debug.Log("Textures Fix Done");
        EditorUtility.ClearProgressBar();
    }

    [MenuItem("MaimaiRE/AssetRipper Patch/UI Image Perserve Aspect")]
    private static void FixUIImages()
    {
        // XXX: Not actually based on anything. Though works for Sinmai
        AssetDatabase.StartAssetEditing();
        try
        {
            string[] guids = AssetDatabase.FindAssets("t:GameObject");
            int totalTextures = guids.Length;
            int processedTextures = 0;
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;

                if (importer != null && importer.textureType == TextureImporterType.Sprite)
                {
                    if (importer.spriteImportMode == SpriteImportMode.Single)
                    {
                        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);

                        if (texture != null)
                        {
                            TextureImporterSettings textureSettings = new TextureImporterSettings();
                            importer.ReadTextureSettings(textureSettings);
                            textureSettings.spriteMeshType = SpriteMeshType.FullRect;
                            if (texture.width == texture.height)
                            {
                                textureSettings.spritePivot = new Vector2(0.5f, 0.5f);
                            }

                            importer.SetTextureSettings(textureSettings);
                            importer.SaveAndReimport();
                        }
                    }
                }
                processedTextures++;
                EditorUtility.DisplayProgressBar("Fixing Textures", $"Processing {path}", (float)processedTextures / totalTextures);
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }
        Debug.Log("Textures Fix Done");
        EditorUtility.ClearProgressBar();
    }
}