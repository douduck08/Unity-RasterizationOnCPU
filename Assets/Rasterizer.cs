using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rasterizer {

    public static Matrix4x4 GetViewProjectionMatrix (Camera camera) {
        var view = camera.transform.worldToLocalMatrix;
        var project = GetProjectionMatrix (camera.fieldOfView, camera.aspect, camera.nearClipPlane, camera.farClipPlane);
        return project * view;
    }

    public static Matrix4x4 GetProjectionMatrix (float fov, float aspect, float zNear, float zFar) {
        var project = new Matrix4x4 ();

        float halfHeight = zNear * Mathf.Tan (Mathf.Deg2Rad * fov * 0.5f); // unity fov is vertical fov
        float halfWidth = halfHeight * aspect;

        project[0, 0] = -zNear / halfWidth;
        project[1, 1] = -zNear / halfHeight;

        // z[near, far] -> ndc[0, 1]
        // project[2, 2] = -1f * zFar / (zFar - zNear);
        // project[2, 3] = zFar * zNear / (zFar - zNear);

        // z[near, far] -> ndc[1, 0] (reversed-Z)
        project[2, 2] = zNear / (zFar - zNear);
        project[2, 3] = -1f * zFar * zNear / (zFar - zNear);

        project[3, 2] = -1;

        project = Matrix4x4.Translate (new Vector3 (0, 0, 0.5f)) * Matrix4x4.Scale (new Vector3 (1, 1, 0.5f)) * project;

        return project;
    }


}
