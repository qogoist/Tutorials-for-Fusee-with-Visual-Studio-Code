#ifdef GL_ES
    precision highp float;
#endif

uniform vec4 DiffuseColor;

void main()
{
    gl_FragColor = DiffuseColor;
}