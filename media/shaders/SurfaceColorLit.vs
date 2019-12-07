
attribute highp vec3 position;
attribute mediump vec3 normal;

varying mediump vec3 v_light;

uniform highp mat3 agk_WorldNormal;
uniform highp mat4 agk_World;
uniform highp mat4 agk_ViewProj;

uniform mediump vec3 u_lightDirection;
uniform mediump vec3 u_lightDiffuse;

void main()
{
	highp vec4 pos = agk_World * vec4(position,1.0);
	gl_Position = agk_ViewProj * pos;

	mediump vec3 norm = normalize(agk_WorldNormal * normal);

	v_light = u_lightDiffuse * max(0.0, dot(norm, u_lightDirection));
}
