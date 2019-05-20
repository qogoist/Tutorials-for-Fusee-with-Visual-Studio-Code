attribute vec3 fuVertex;

void main()
{
    gl_Position = vec4(fuVertex, 1.0);
}