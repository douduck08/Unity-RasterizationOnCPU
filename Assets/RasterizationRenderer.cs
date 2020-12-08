using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class RasterizationRenderer : MonoBehaviour {

    public Model[] models;

    [Header ("Config")]
    public Vector2Int resolution = new Vector2Int (512, 512);
    public new Camera camera;
    public bool reversedZ = true;

    [Header ("Display")]
    public bool displayColor;
    public Rect colorView = new Rect (0, 0, 512, 512);
    public bool displayDepth;
    public Rect depthView = new Rect (0, 0, 512, 512);

    Texture2D colorBuffer;
    Texture2D depthBuffer;

    void OnEnable () {
        var time = Time.realtimeSinceStartup;

        var rasterizer = new Rasterizer (resolution.x, resolution.y);
        rasterizer.Draw (camera, models, reversedZ);

        Debug.LogFormat ("Time spent: {0:0.000} s", Time.realtimeSinceStartup - time);

        colorBuffer = rasterizer.ExportColorBuffer ();
        depthBuffer = rasterizer.ExportDepthBuffer ();
    }

    void OnGUI () {
        if (displayColor && colorBuffer != null) {
            GUI.DrawTexture (colorView, colorBuffer);
        }
        if (displayDepth && depthBuffer != null) {
            GUI.DrawTexture (depthView, depthBuffer);
        }
    }
}