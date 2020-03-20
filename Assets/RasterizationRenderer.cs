using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class RasterizationRenderer : MonoBehaviour {

    public Vector2Int resolution = new Vector2Int (512, 512);

    Texture2D result;

    void Start () {
        result = new Texture2D (resolution.x, resolution.y);
        Draw (result);
    }

    void Draw (Texture2D result) {
        Color[] pixels = result.GetPixels ();
        for (int i = 0; i < pixels.Length; i++) {
            pixels[i] = Color.red;
        }
        result.SetPixels (pixels);
        result.Apply ();
    }

    void OnGUI () {
        GUI.DrawTexture (new Rect (0, 0, resolution.x, resolution.y), result);
    }

}