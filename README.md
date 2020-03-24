## Rasterization on CPU
A practical implement of CPU rasterization pipepline.

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

Rasterization: a Practical Implementation
* https://www.scratchapixel.com/lessons/3d-basic-rendering/rasterization-practical-implementation/overview-rasterization-algorithm

Q: What does hardware work about the conversion from clip space to NDC.
* https://forum.unity.com/threads/what-coordinate-space-dose-fragment-shaders-input-in.521960/
* https://answers.unity.com/questions/1443941/shaders-what-is-clip-space.html