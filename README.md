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
* CPU: i5-8600, 3.10GHz
* Orinal Scene
![Orinal Scene](https://github.com/douduck08/Unity-RasterizationOnCPU/blob/master/images/original_scene.png)
* Testing with 320x180 pixels, 4110 triangles, about 0.24s
![color 320x180](https://github.com/douduck08/Unity-RasterizationOnCPU/blob/master/images/color320x180.png)
![depth 320x180](https://github.com/douduck08/Unity-RasterizationOnCPU/blob/master/images/depth320x180.png)
* Testing with 1920x1080 pixels, 4110 triangles, about 7.46s
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

Q: How exactly does OpenGL do perspectively correct linear interpolation?
* https://stackoverflow.com/questions/24441631/how-exactly-does-opengl-do-perspectively-correct-linear-interpolation