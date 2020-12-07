using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Model {
    public MeshFilter meshFilter;
    public Color color;
}

public class VaryingData {
    public List<Vector4> sv_position = new List<Vector4> (); // positions in clip space
}

public class FragmentData {
    public List<int> index = new List<int> (); // index of color buffer and depth buffer
    public List<Vector3> sv_position = new List<Vector3> (); // positions in clip space and NDC
}

public class Rasterizer {

    int width, height;
    Vector2 pixelSize;
    Color[] colorBuffer;
    float[] depthBuffer;

    public Rasterizer (int width, int height) {
        this.width = width;
        this.height = height;
        pixelSize = new Vector2 (1f / width, 1f / height);

        var length = width * height;
        colorBuffer = new Color[length];
        depthBuffer = new float[length];
    }

    public void Draw (Camera camera, Model[] models, bool reversedZ = true) {
        Clear (camera.backgroundColor, reversedZ ? 0f : 1f);

        var vpMatrix = GetViewProjectionMatrix (camera, reversedZ);
        var pixelSize = new Vector2 (1f / width, 1f / height);

        foreach (var model in models) {
            var mesh = model.meshFilter.sharedMesh;
            var mvp = vpMatrix * model.meshFilter.transform.localToWorldMatrix;
            var varyings = GeometryProcessing (mesh.vertices, mvp);
            var fragmentData = ClippingAndRasterization (varyings, mesh.GetIndices (0));
            PixelProcessingAndMerge (fragmentData, model.color);
        }
    }

    public Texture2D ExportColorBuffer () {
        var result = new Texture2D (width, height);
        result.filterMode = FilterMode.Point; ;
        result.SetPixels (colorBuffer);
        result.Apply ();
        return result;
    }

    public Texture2D ExportDepthBuffer () {
        var result = new Texture2D (width, height);
        result.filterMode = FilterMode.Point; ;
        result.SetPixels (depthBuffer.Select (z => new Color (z, z, z, 1f)).ToArray ());
        result.Apply ();
        return result;
    }

    void Clear (Color color, float depth) {
        var length = colorBuffer.Length;
        for (int i = 0; i < length; i++) {
            colorBuffer[i] = color;
            depthBuffer[i] = depth;
        }
    }

    VaryingData GeometryProcessing (Vector3[] vertices, Matrix4x4 mvp) {
        var result = new VaryingData ();
        var count = vertices.Length;
        for (int i = 0; i < count; i++) {
            result.sv_position.Add (mvp * new Vector4 (vertices[i].x, vertices[i].y, vertices[i].z, 1));
        }
        return result;
    }

    FragmentData ClippingAndRasterization (VaryingData varyingData, int[] indexes) {
        var result = new FragmentData ();
        var count = indexes.Length;
        for (int i = 0; i < count; i += 3) {
            var index0 = indexes[i];
            var index1 = indexes[i + 1];
            var index2 = indexes[i + 2];
            var v0 = new Vector3 (varyingData.sv_position[index0].x, varyingData.sv_position[index0].y, varyingData.sv_position[index0].z) / varyingData.sv_position[index0].w;
            var v1 = new Vector3 (varyingData.sv_position[index1].x, varyingData.sv_position[index1].y, varyingData.sv_position[index1].z) / varyingData.sv_position[index1].w;
            var v2 = new Vector3 (varyingData.sv_position[index2].x, varyingData.sv_position[index2].y, varyingData.sv_position[index2].z) / varyingData.sv_position[index2].w;

            var xMin = Mathf.Max (Mathf.FloorToInt ((Mathf.Min (v0.x, v1.x, v2.x) * 0.5f + 0.5f) / pixelSize.x), 0);
            var xMax = Mathf.Min (Mathf.CeilToInt ((Mathf.Max (v0.x, v1.x, v2.x) * 0.5f + 0.5f) / pixelSize.x) + 1, width);
            var yMin = Mathf.Max (Mathf.FloorToInt ((Mathf.Min (v0.y, v1.y, v2.y) * 0.5f + 0.5f) / pixelSize.y), 0);
            var yMax = Mathf.Min (Mathf.CeilToInt ((Mathf.Max (v0.y, v1.y, v2.y) * 0.5f + 0.5f) / pixelSize.y) + 1, height);

            var area = EdgeFunction (v0, v1, v2);
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

                        var invZ0 = 1f / v0.z;
                        var invZ1 = 1f / v1.z;
                        var invZ2 = 1f / v2.z;

                        var position = new Vector3 ();
                        position.x = v0.x * w0 + v1.x * w1 + v2.x * w2;
                        position.y = v0.y * w0 + v1.y * w1 + v2.y * w2;
                        position.z = 1f / (invZ0 * w0 + invZ1 * w1 + invZ2 * w2);

                        var index = y * width + x;
                        result.index.Add (index);
                        result.sv_position.Add (position);
                    }
                }
            }
        }
        return result;
    }

    void PixelProcessingAndMerge (FragmentData fragmentData, Color materialColor) {
        var size = fragmentData.index.Count;
        for (int i = 0; i < size; i++) {
            var index = fragmentData.index[i];
            var currentZ = fragmentData.sv_position[i].z;
            if (currentZ > depthBuffer[index]) {
                colorBuffer[index] = materialColor;
                depthBuffer[index] = currentZ;
            }
        }
    }

    // ************** //
    // helper methods //
    // ************** //
    public static Matrix4x4 GetViewProjectionMatrix (Camera camera, bool reversedZ = true) {
        var view = camera.transform.worldToLocalMatrix;
        var project = GetProjectionMatrix (camera.fieldOfView, camera.aspect, camera.nearClipPlane, camera.farClipPlane, reversedZ);
        return project * view;
    }

    public static Matrix4x4 GetProjectionMatrix (float fov, float aspect, float zNear, float zFar, bool reversedZ = true) {
        var project = new Matrix4x4 ();

        float halfHeight = zNear * Mathf.Tan (Mathf.Deg2Rad * fov * 0.5f); // unity fov is vertical fov
        float halfWidth = halfHeight * aspect;

        project[0, 0] = -zNear / halfWidth;
        project[1, 1] = -zNear / halfHeight;
        project[3, 2] = -1;

        if (reversedZ) {
            // z[near, far] -> ndc[1, 0] (reversed-Z)
            project[2, 2] = zNear / (zFar - zNear);
            project[2, 3] = -1f * zFar * zNear / (zFar - zNear);
        } else {
            // z[near, far] -> ndc[0, 1]
            project[2, 2] = -1f * zFar / (zFar - zNear);
            project[2, 3] = zFar * zNear / (zFar - zNear);
        }

        return project;
    }

    static float EdgeFunction (Vector2 a, Vector2 b, Vector2 c) {
        return (c.x - a.x) * (b.y - a.y) - (c.y - a.y) * (b.x - a.x);
    }
}
