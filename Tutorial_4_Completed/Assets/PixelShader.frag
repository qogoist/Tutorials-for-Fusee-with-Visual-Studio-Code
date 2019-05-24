#ifdef GL_ES
    precision highp float;
#endif

uniform vec4 DiffuseColor;
varying vec3 modelpos;
varying vec3 normal;

void main()
{
    float intensity = dot(normal, vec3(0, 0, -1));
    gl_FragColor = DiffuseColor * vec4(intensity, intensity, intensity, 1.0);
}