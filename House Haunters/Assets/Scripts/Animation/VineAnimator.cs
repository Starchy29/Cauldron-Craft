using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using Unity.Collections;

public class VineAnimator : IMoveAnimator
{
    public bool Completed { get; private set; }

    private Monster user;
    private Monster grabbed;
    private StatusAilment slowness;
    private SpriteRenderer renderer;

    private Vector2 direction;
    private float grabDist;
    private float releaseDist;
    private float length;
    private bool returning;

    private const int MAX_RANGE = 4;

    public VineAnimator(Monster user, Monster grabbed, Vector2Int endTile, StatusAilment slowness) {
        this.user = user;
        this.grabbed = grabbed;
        this.slowness = slowness;

        GameObject created = GameObject.Instantiate(PrefabContainer.Instance.pullVines);
        renderer = created.GetComponent<SpriteRenderer>();
        renderer.sharedMaterial = new Material(Shader.Find("Unlit/ExtensionShader"));
        created.SetActive(false);

        Vector2 userPos = LevelGrid.Instance.Tiles.GetCellCenterWorld((Vector3Int)user.Tile);
        Vector2 targetPos = LevelGrid.Instance.Tiles.GetCellCenterWorld((Vector3Int)grabbed.Tile);
        direction = (targetPos - userPos).normalized;
        created.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Rad2Deg * Mathf.Atan2(direction.y, direction.x));
        created.transform.position = userPos + MAX_RANGE / 2f * direction;

        Vector2Int tileDiff = grabbed.Tile - user.Tile;
        grabDist = Mathf.Abs(tileDiff.x) + Mathf.Abs(tileDiff.y);
        tileDiff = endTile - user.Tile;
        releaseDist = Mathf.Abs(tileDiff.x) + Mathf.Abs(tileDiff.y);
    }

    public void Start() {
        renderer.gameObject.SetActive(true);
        SetLength(0);
    }

    public void Update(float deltaTime) {
        if(returning) {
            length -= 2f * deltaTime;
            if(GetVineLocation() > releaseDist) {
                grabbed.transform.position = user.transform.position + GetVineLocation() * (Vector3)direction;
                grabbed.UpdateSortingOrder();
            } else {
                grabbed.transform.position = user.transform.position + releaseDist * (Vector3)direction;
                grabbed.UpdateSortingOrder();
            }

            if(length < 0) {
                GameObject.Destroy(renderer.gameObject);
                Completed = true;
                return;
            }
        } else {
            length += 5f * deltaTime;
            if(GetVineLocation() > grabDist) {
                length = grabDist / MAX_RANGE;
                returning = true;

                // apply slowness effect in the middle
                grabbed.ApplyStatus(slowness).SetActive(true);
                SoundManager.Instance.PlaySound(Sounds.VineLash);
            }
        }
        
        SetLength(length);
    }

    private float GetVineLocation() {
        return length * MAX_RANGE;
    }

    public void SetLength(float percent) {
        // See ExtensionShader, uses TEXCOORD1 for the extension value
        Vector2[] fakeUVs = new Vector2[4] {
            new Vector2(percent, 0f),
            new Vector2(percent, 0f),
            new Vector2(percent, 0f),
            new Vector2(percent, 0f)
        };
        SpriteDataAccessExtensions.SetVertexAttribute(renderer.sprite, UnityEngine.Rendering.VertexAttribute.TexCoord1,
            new NativeArray<Vector2>(fakeUVs, Allocator.Temp));
    }
}
