using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;
using System;
using System.IO;
using System.Threading.Tasks;

public class AsmDefinition
{
    public string ClassName;
    public string GUID;
    public long FileID;
    public AsmDefinition(string className, string guid, long fileID)
    {
        ClassName = className;
        GUID = guid;
        FileID = fileID;
    }
    public string YamlSection()
    {
        return $"{{fileID: {FileID}, guid: {GUID}, type: 3}}";
    }
    public override string ToString()
    {
        return $"{ClassName}, fileID:{FileID}, guid:{GUID} ";
    }
}
public class AsmDefinitions : Dictionary<string, AsmDefinition> { }; // wheres typedef
public class MetaDefinition
{
    public string Name;
    public string GUID;
    public MetaDefinition(string name, string guid)
    {
        Name = name;
        GUID = guid;
    }
    public string YamlSection()
    {
        return $"{{fileID: 11500000, guid: {GUID}, type: 3}}";
    }
    public override string ToString()
    {
        return $"{Name}, guid:{GUID}";
    }
}
public class MetaDefinitions : Dictionary<string, MetaDefinition> { };
public class AssemblyToLooseScript : EditorWindow
{
    private List<string> Extensions = new List<string>(){
        "playable",
        "prefab",
        "unity",
        "asset",
        "anim",
        "controller",
        "mat",
        "shader",
        "cginc",
        "compute",
        "mask",
        "render",
        "shadervariants",
        "spriteatlas",
        "terrainlayer",
        "physicmaterial",
        "physicsmaterial2d",
        "physicmaterial2d",
        "physicsmaterial",
        "giparams",
        "flare",
        "cubemap",
        "lighting",
        "lightmap",
    };
    private string Title = "Assembly To Loose Script";
    private string AsmPath = "Assets/Plugins/Unity.Timeline.dll";
    private string LooseScriptPath = "Packages/com.unity.timeline/Runtime";
    private string AssetPath = "Assets";

    [MenuItem("MaimaiRE/AssetRipper Patch/Asm. To Loose Script")]
    public static void ShowWindow()
    {
        GetWindow<AssemblyToLooseScript>("Scene Conversion");
    }
    private static AsmDefinitions UpdateAsmDefinitions(string assetPath)
    {
        var results = new AsmDefinitions();
        MonoScript[] scripts = AssetDatabase.LoadAllAssetsAtPath(assetPath)
        .Where(a => a is MonoScript)
        .Select(a => (MonoScript)a).ToArray();
            results.Clear();
        foreach (MonoScript script in scripts)
        {
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(script, out string guid, out long fileID);
            results.Add(script.name,new AsmDefinition(script.name, guid, fileID));
        }
        return results;
    }
    private static MetaDefinitions UpdateMetaDefinitions(string assetPath)
    {
        var results = new MetaDefinitions();
        string[] guids = AssetDatabase.FindAssets("t:script", new string[] { assetPath });
        results.Clear();
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string name = Path.GetFileNameWithoutExtension(path);
            results.Add(name, new MetaDefinition(name, guid));
        }
        return results;
    }
    private static List<string> GetFiles(string path, List<string> extensions)
    {
        List<string> files = new List<string>();
        foreach (string extension in extensions)
        {
            files.AddRange(Directory.GetFiles(path, $"*.{extension}", SearchOption.AllDirectories));
        }
        return files;
    }    
    private AsmDefinitions asmDefinitions;
    private MetaDefinitions metaDefinitions;
    private List<string> assets;
    private void ConvertOne(string assetPath)
    {
        var lines = File.ReadAllLines(assetPath);
        int count = 0;
        for (int i = 0; i < lines.Length; i++)
        {        
            foreach (var key in asmDefinitions.Keys)
            {
                if (metaDefinitions.ContainsKey(key))
                {
                    var asmDef = asmDefinitions[key];
                    var asmYaml = asmDef.YamlSection();
                    var metaDef = metaDefinitions[key];
                    var metaYaml = metaDef.YamlSection();
                    while (lines[i].Contains(asmYaml))
                    {
                        lines[i] = lines[i].Replace(asmYaml, metaYaml);
                        count++;
                    }
                }
            }        
        }
        if (count > 0)
        {
            File.WriteAllLines(assetPath, lines);
            Debug.Log($"Converted {assetPath}, n={count}");
        }
    }
    private void ConvertAll()
    {
        Parallel.ForEach(assets, assetPath =>
        {
            ConvertOne(assetPath);
        });
    }
    private Vector2[] scroll;
    private void OnEnable()
    {
        scroll = new Vector2[16];
    }
    private void OnGUI()
    {
        GUILayout.Label(Title, EditorStyles.boldLabel);
        EditorGUILayout.Space();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Assembly Path: ", GUILayout.Width(100));
        AsmPath = GUILayout.TextField(AsmPath);
        GUILayout.EndHorizontal();
        if (GUILayout.Button("Load Definitions"))
        {
            asmDefinitions = UpdateAsmDefinitions(AsmPath);
        }
        if (asmDefinitions != null)
        {
            scroll[0] = GUILayout.BeginScrollView(scroll[0], GUILayout.Height(100));
            foreach (var asmDefinition in asmDefinitions)
            {
                GUILayout.Label(asmDefinition.Value.ToString());
            }
            GUILayout.EndScrollView();
        }
        GUILayout.BeginHorizontal();
        GUILayout.Label("Loose Script Path: ", GUILayout.Width(100));
        LooseScriptPath = GUILayout.TextField(LooseScriptPath);
        GUILayout.EndHorizontal();
        if (GUILayout.Button("Load Definitions"))
        {
            metaDefinitions = UpdateMetaDefinitions(LooseScriptPath);
        }
        if (metaDefinitions != null)
        {
            scroll[1] = GUILayout.BeginScrollView(scroll[1], GUILayout.Height(100));
            foreach (var metaDefinitions in metaDefinitions)
            {
                GUILayout.Label(metaDefinitions.Value.ToString());
            }
            GUILayout.EndScrollView();
        }
        GUILayout.BeginHorizontal();
        GUILayout.Label("Asset Path: ", GUILayout.Width(100));
        AssetPath = GUILayout.TextField(AssetPath);
        GUILayout.EndHorizontal();
        if (GUILayout.Button("Scan For Assets"))
        {
            assets = GetFiles(AssetPath, Extensions);
        }
        if (assets != null)
        {
            GUILayout.Label($"Found {assets.Count} assets");
        }
        if (metaDefinitions != null && asmDefinitions != null && assets != null)
        {
            GUILayout.Label("Reload the project for changes to take effect");
            if (GUILayout.Button("Convert All"))
            {
                ConvertAll();
            }
        }
    }
}
