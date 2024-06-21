using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using Unity.Collections;

public class VFXAnimator : MonoBehaviour
{
    private Mesh mesh;
    private float t;

    void Start()
    {
        mesh = new Mesh();

        Vector3[] vertices = new Vector3[4] { 
            new Vector3(-0.5f, -0.5f, 0.0f),
            new Vector3(0.5f, -0.5f, 0.0f),
            new Vector3(-0.5f, 0.5f, 0.0f),
            new Vector3(0.5f, 0.5f, 0.0f)
        };
        mesh.vertices = vertices;

        int[] indices = new int[6] {
            0,2,1,
            2,3,1
        };
        mesh.triangles = indices;

        Vector2[] uvs = new Vector2[4] {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1)
        };
        mesh.uv = uvs;

        SetMeshColor(Color.cyan);

        MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = new Material(Shader.Find("Unlit/WaveParticleShader"));

        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;
    }

    // Update is called once per frame
    void Update()
    {
        t += 3.0f * Time.deltaTime;
        SetRadius((Mathf.Sin(t) + 1.0f) / 2.0f);
    }

    private void SetRadius(float radius) {
        List<Vector2> fakeUVs = new List<Vector2> {
            new Vector2(radius, 0f), new Vector2(radius, 0f), new Vector2(radius, 0f), new Vector2(radius, 0f)
        };
        mesh.SetUVs(1, fakeUVs);
    }

    private void SetMeshColor(Color color) {
        Color[] colors = new Color[4] {
            color, color, color, color
        };

        mesh.colors = colors;
    }
}
