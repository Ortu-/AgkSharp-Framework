
attribute highp vec3 position;

uniform highp mat4 agk_WorldViewProj;

void main()
{
	gl_Position = agk_WorldViewProj * vec4(position,1.0);
}
