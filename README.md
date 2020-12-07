## Rasterization on CPU
A practical implement of CPU rasterization pipepline.

### GPU Pipeline
1. Application Stage
2. Geometry Processing
    * Vertex Shader
    * Tessellation
    * Geometry Shader
3. Clipping
4. Rasterization
5. Pixel Processing
    * Early-Z
    * Fragment shader
    * Z-test
    * Color blending

### Testing Cases
* Testing with 1 quad, 2 cubes and 1 sphere: 320x180 pixels, 794 triangles, 8.205s
* Optimizing with triangle bounding box, 0.049s
![color 320x180](https://github.com/douduck08/Unity-RasterizationOnCPU/blob/master/images/color320x180.png)
![depth 320x180](https://github.com/douduck08/Unity-RasterizationOnCPU/blob/master/images/depth320x180.png)

* Testing with another scene, with triangle bounding box: 1920x1080 pixels, 4110 triangles, 4.895s
![color 1920x1080](https://github.com/douduck08/Unity-RasterizationOnCPU/blob/master/images/color1920x1080.png)
![depth 1920x1080](https://github.com/douduck08/Unity-RasterizationOnCPU/blob/master/images/depth1920x1080.png)

### Ref
OpenGL Projection Matrix
* http://www.songho.ca/opengl/gl_projectionmatrix.html

About the Projection Matrix, the GPU Rendering Pipeline and Clipping
* https://www.scratchapixel.com/lessons/3d-basic-rendering/perspective-and-orthographic-projection-matrix/projection-matrix-GPU-rendering-pipeline-clipping

Rasterization: a Practical Implementation
* https://www.scratchapixel.com/lessons/3d-basic-rendering/rasterization-practical-implementation/overview-rasterization-algorithm

Q: What does hardware work about the conversion from clip space to NDC.
* https://forum.unity.com/threads/what-coordinate-space-dose-fragment-shaders-input-in.521960/
* https://answers.unity.com/questions/1443941/shaders-what-is-clip-space.html