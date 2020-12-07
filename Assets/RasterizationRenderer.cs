using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

[System.Serializable]
public class Model {
    public MeshFilter meshFilter;
    public Color color;

    public bool IsActive () {
        return meshFilter.gameObject.activeInHierarchy;
    }

    public VertexBuffer CreateVertexBuffer (Matrix4x4 viewProj) {
        var mesh = meshFilter.sharedMesh;
        var vertices = mesh.vertices;
        var triangles = mesh.triangles;

        var vertexBuffer = new VertexBuffer ();
        for (int i = 0; i < triangles.Length; i += 3) {
            vertexBuffer.vertices.Add (vertices[triangles[i]]);
            vertexBuffer.vertices.Add (vertices[triangles[i + 1]]);
            vertexBuffer.vertices.Add (vertices[triangles[i + 2]]);
        }

        var m = meshFilter.transform.localToWorldMatrix;
        vertexBuffer.mvp = viewProj * m;
        return vertexBuffer;
    }
}

public class VertexBuffer {
    public List<Vector3> vertices = new List<Vector3> ();

    // Uniforms
    public Matrix4x4 mvp;

    // Varyings
    public List<Vector4> positions = new List<Vector4> (); // positions in clip space
}

public class PixelBuffer {
    public List<int> indexes = new List<int> ();

    // Varyings
    public List<Vector3> positions = new List<Vector3> (); // positions in NDC
}

public class RasterizationRenderer : MonoBehaviour {

    [Header ("Display")]
    public bool displayDepth;
    public Vector2 displaySize = new Vector2 (512, 512);

    [Header ("Draw")]
    public Vector2Int resolution = new Vector2Int (512, 512);
    public Color backgroundColor;
    public new Camera camera;
    public List<Model> models;

    Texture2D result;
    Texture2D depthMap;

    void Start () {
        result = new Texture2D (resolution.x, resolution.y);
        result.filterMode = FilterMode.Point;

        depthMap = new Texture2D (resolution.x, resolution.y);
        depthMap.filterMode = FilterMode.Point;

        var time = Time.realtimeSinceStartup;
        Draw ();
        time = Time.realtimeSinceStartup - time;
        Debug.LogFormat ("Time spent: {0:0.000} s", time);

    }

    void Draw () {
        // GPU Pipeline:
        // 1. Application Stage
        // 2. Geometry Processing
        //   * Vertex Shader
        //   * Tessellation
        //   * Geometry Shader
        // 3. Clipping
        // 4. Rasterization
        // 5. Pixel Processing
        //   * Early-Z
        //   * Fragment shader
        //   * Z-test
        //   * Color blending

        var view = camera.worldToCameraMatrix;
        var proj = camera.projectionMatrix;
        var viewProj = Rasterizer.GetViewProjectionMatrix (camera);

        Color[] colorBuffer = result.GetPixels ();
        float[] depthBuffer = new float[colorBuffer.Length];
        Clear (colorBuffer, depthBuffer, backgroundColor, 0f);

        foreach (var model in models) {
            if (!model.IsActive ()) {
                continue;
            }

            var vertexBuffer = model.CreateVertexBuffer (viewProj);
            GeometryProcessing (vertexBuffer);

            var pixelBuffer = new PixelBuffer ();
            ClippingAndRasterization (vertexBuffer, pixelBuffer);
            PixelProcessing (pixelBuffer, colorBuffer, depthBuffer, model.color);
        }

        result.SetPixels (colorBuffer);
        result.Apply ();

        depthMap.SetPixels (depthBuffer.Select (z => new Color (z, z, z)).ToArray ());
        depthMap.Apply ();
    }

    void Clear (Color[] colorBuffer, float[] depthBuffer, Color color, float depth) {
        var size = colorBuffer.Length;
        for (int i = 0; i < size; i++) {
            colorBuffer[i] = color;
            depthBuffer[i] = depth;
        }
    }

    void GeometryProcessing (VertexBuffer vertexBuffer) {
        var size = vertexBuffer.vertices.Count;
        for (int i = 0; i < size; i++) {
            vertexBuffer.positions.Add (vertexBuffer.mvp * new Vector4 (vertexBuffer.vertices[i].x, vertexBuffer.vertices[i].y, vertexBuffer.vertices[i].z, 1));
        }
    }

    void ClippingAndRasterization (VertexBuffer vertexBuffer, PixelBuffer pixelBuffer) {
        var pixelSize = new Vector2 (1f / resolution.x, 1f / resolution.y);
        var size = vertexBuffer.vertices.Count;
        for (int i = 0; i < size; i += 3) {
            var v0 = new Vector2 (vertexBuffer.positions[i].x, vertexBuffer.positions[i].y) / vertexBuffer.positions[i].w;
            var v1 = new Vector2 (vertexBuffer.positions[i + 1].x, vertexBuffer.positions[i + 1].y) / vertexBuffer.positions[i + 1].w;
            var v2 = new Vector2 (vertexBuffer.positions[i + 2].x, vertexBuffer.positions[i + 2].y) / vertexBuffer.positions[i + 2].w;
            var area = EdgeFunction (v0, v1, v2);

            var xMin = Mathf.Max (Mathf.FloorToInt ((Mathf.Min (v0.x, v1.x, v2.x) * 0.5f + 0.5f) / pixelSize.x), 0);
            var xMax = Mathf.Min (Mathf.CeilToInt ((Mathf.Max (v0.x, v1.x, v2.x) * 0.5f + 0.5f) / pixelSize.x) + 1, resolution.x);
            var yMin = Mathf.Max (Mathf.FloorToInt ((Mathf.Min (v0.y, v1.y, v2.y) * 0.5f + 0.5f) / pixelSize.y), 0);
            var yMax = Mathf.Min (Mathf.CeilToInt ((Mathf.Max (v0.y, v1.y, v2.y) * 0.5f + 0.5f) / pixelSize.y) + 1, resolution.y);

            for (int x = xMin; x < xMax; x++) {
                for (int y = yMin; y < yMax; y++) {
                    var p = new Vector2 (x * pixelSize.x, y * pixelSize.y) + pixelSize * 0.5f;
                    p = p * 2f - new Vector2 (1f, 1f);

                    var w0 = EdgeFunction (v1, v2, p);
                    var w1 = EdgeFunction (v2, v0, p);
                    var w2 = EdgeFunction (v0, v1, p);
                    if (w0 >= 0 && w1 >= 0 && w2 >= 0) {
                        // triangle contains the point
                        w0 /= area;
                        w1 /= area;
                        w2 /= area;

                        var invZ0 = 1f / (vertexBuffer.positions[i].z / vertexBuffer.positions[i].w);
                        var invZ1 = 1f / (vertexBuffer.positions[i + 1].z / vertexBuffer.positions[i + 1].w);
                        var invZ2 = 1f / (vertexBuffer.positions[i + 2].z / vertexBuffer.positions[i + 2].w);

                        var position = new Vector3 ();
                        position.x = v0.x * w0 + v1.x * w1 + v2.x * w2;
                        position.y = v0.y * w0 + v1.y * w1 + v2.y * w2;
                        position.z = 1f / (invZ0 * w0 + invZ1 * w1 + invZ2 * w2);

                        var index = y * resolution.x + x;
                        pixelBuffer.indexes.Add (index);
                        pixelBuffer.positions.Add (position);
                    }
                }
            }
        }
    }

    void PixelProcessing (PixelBuffer pixelBuffer, Color[] colorBuffer, float[] depthBuffer, Color materialColor) {
        var size = pixelBuffer.indexes.Count;
        for (int i = 0; i < size; i++) {
            var index = pixelBuffer.indexes[i];
            var currentZ = pixelBuffer.positions[i].z;
            if (currentZ > depthBuffer[index]) {
                colorBuffer[index] = materialColor;
                depthBuffer[index] = currentZ;
            }
        }
    }

    float EdgeFunction (Vector2 a, Vector2 b, Vector2 c) {
        return (c.x - a.x) * (b.y - a.y) - (c.y - a.y) * (b.x - a.x);
    }

    void OnGUI () {
        if (displayDepth) {
            GUI.DrawTexture (new Rect (0, 0, displaySize.x, displaySize.y), depthMap);
        } else {
            GUI.DrawTexture (new Rect (0, 0, displaySize.x, displaySize.y), result);
        }
    }
}