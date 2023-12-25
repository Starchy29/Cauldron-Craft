using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum TileType {
    Ground,
    Wall,
    Pit
}

public class TypedTile : Tile
{
    [SerializeField] private TileType type;
    public TileType Type { get { return type; } }

    // from https://docs.unity3d.com/Manual/Tilemap-ScriptableTiles-Example.html
#if UNITY_EDITOR
    [MenuItem("Assets/Create/TypedTile")]
    public static void CreateTypedTile()
    {
        string path = EditorUtility.SaveFilePanelInProject("Save Typed Tile", "New Typed Tile", "Asset", "Save Typed Tile", "Assets");
        if (path == "")
            return;
        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<TypedTile>(), path);
    }
#endif
}
