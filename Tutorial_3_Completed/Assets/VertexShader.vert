attribute vec3 fuVertex;
attribute vec3 fuNormal;
uniform mat4 xform;
varying vec3 modelpos;
varying vec3 normal;

void main()
{
    modelpos = fuVertex;
    normal = fuNormal;
    gl_Position = xform * vec4(fuVertex, 1.0);
}