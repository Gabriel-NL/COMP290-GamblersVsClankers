using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class GridSerializer
{
    [Serializable]
    public class GridEntry<T>
    {
        public int x;
        public int y;   // use z if your grid is XZ
        public T value;
    }

    [Serializable]
    public class MetaPair
    {
        public string key;
        public string value;
    }

    [Serializable]
    public class GridWrapper<T>
    {
        public List<MetaPair> metadata;
        public List<GridEntry<T>> entries;
    }

    // === JSON (in-memory) ===
    public string ToJson<T>(Dictionary<(int x, int y), T> grid, Dictionary<string, string> meta)
    {
        var entries = new List<GridEntry<T>>(grid.Count);
        foreach (var kv in grid)
        {
            entries.Add(new GridEntry<T> { x = kv.Key.x, y = kv.Key.y, value = kv.Value });
        }

        var metaList = new List<MetaPair>();
        if (meta != null)
            foreach (var kv in meta)
                metaList.Add(new MetaPair { key = kv.Key, value = kv.Value });

        var wrapper = new GridWrapper<T> { metadata = metaList, entries = entries };
        return JsonUtility.ToJson(wrapper, true);
    }

    public Dictionary<(int x, int y), T> FromJson<T>(string json, out Dictionary<string, string> meta)
    {
        var wrapper = JsonUtility.FromJson<GridWrapper<T>>(json);

        meta = new Dictionary<string, string>();
        if (wrapper.metadata != null)
            foreach (var pair in wrapper.metadata)
                meta[pair.key] = pair.value;

        var dict = new Dictionary<(int, int), T>();
        if (wrapper.entries != null)
            foreach (var e in wrapper.entries)
                dict[(e.x, e.y)] = e.value;

        return dict;
    }

    // === File paths ===
    public static string GetDefaultFolder()
    {
        // e.g. .../AppData/LocalLow/Company/Product/Saves
        string dir = Path.Combine(Application.persistentDataPath, "Saves");
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        return dir;
    }

    public static string GetDefaultPath(string saveName)
    {
        // ensure .json
        string file = saveName.EndsWith(".json", StringComparison.OrdinalIgnoreCase)
            ? saveName
            : saveName + ".json";
        return Path.Combine(GetDefaultFolder(), file);
    }

    // === Save/Load to file ===
    public void SaveToFile<T>(Dictionary<(int x, int y), T> grid, Dictionary<string, string> meta, string pathOrName)
    {
        string path = Path.IsPathRooted(pathOrName) ? pathOrName : GetDefaultPath(pathOrName);
        string json = ToJson(grid, meta);

#if UNITY_WEBGL && !UNITY_EDITOR
        // WebGL can’t write to the file system; use PlayerPrefs as a simple fallback.
        PlayerPrefs.SetString(path, json);
        PlayerPrefs.Save();
#else
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, json, new UTF8Encoding(false)); // UTF-8 no BOM
#endif
    }

    public Dictionary<(int x, int y), T> LoadFromFile<T>(string pathOrName, out Dictionary<string, string> meta)
    {
        string path = Path.IsPathRooted(pathOrName) ? pathOrName : GetDefaultPath(pathOrName);

#if UNITY_WEBGL && !UNITY_EDITOR
        string json = PlayerPrefs.GetString(path, "{}");
#else
        if (!File.Exists(path))
            throw new FileNotFoundException("Save file not found", path);
        string json = File.ReadAllText(path, Encoding.UTF8);
#endif
        return FromJson<T>(json, out meta);
    }
}
