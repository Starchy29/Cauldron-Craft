using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugHelp : MonoBehaviour
{
    [SerializeField] private GameObject tileMarkerPrefab;

    private TMPro.TextMeshPro[,] tileMarkers; 

    private static DebugHelp instance;
    public static DebugHelp Instance { get { 
        if(instance == null) {
            instance = Instantiate(PrefabContainer.Instance.debugger).GetComponent<DebugHelp>();
        }
        return instance;
    } }

    public void MarkTile(Vector2Int tile, string label) {
        if(tileMarkers == null) {
            tileMarkers = new TMPro.TextMeshPro[LevelGrid.Instance.Height, LevelGrid.Instance.Width];
        }

        if(tileMarkers[tile.y, tile.x] == null) {
            GameObject spawned = Instantiate(tileMarkerPrefab);
            tileMarkers[tile.y, tile.x] = spawned.GetComponent<TMPro.TextMeshPro>();
            spawned.transform.position = LevelGrid.Instance.Tiles.GetCellCenterWorld((Vector3Int)tile);
        }

        tileMarkers[tile.y, tile.x].text = label;
    }

    public void ClearMarks() {
        if(tileMarkers == null) {
            return;
        }

        foreach(TMPro.TextMeshPro tileMarker in tileMarkers) {
            if(tileMarker != null) {
                tileMarker.text = "";
            }
        }
    }
}
