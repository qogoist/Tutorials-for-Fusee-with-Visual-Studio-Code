#ifdef GL_ES
    precision highp float;
#endif

uniform vec4 DiffuseColor;
varying vec3 modelpos;
varying vec3 normal;

void main()
{
    gl_FragColor = DiffuseColor * vec4(normal * 0.5 + 0.5, 1.0);
}