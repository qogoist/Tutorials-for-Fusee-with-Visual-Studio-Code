attribute vec3 fuVertex;
uniform float alpha;
varying vec3 modelpos;

void main()
{
    modelpos = fuVertex;
    float s = sin(alpha);
    float c = cos(alpha);
    gl_Position = vec4( fuVertex.x * c - fuVertex.z * s,
                        fuVertex.y,
                        fuVertex.x * s + fuVertex.z * c,
                        1.0);
}