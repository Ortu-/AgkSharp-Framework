
attribute highp vec3 position;
attribute mediump vec3 normal;
attribute mediump vec2 uv;

varying mediump vec3 lightVarying;
varying mediump vec2 uvVarying;

uniform highp mat3 agk_WorldNormal;
uniform highp mat4 agk_World;
uniform highp mat4 agk_ViewProj;
uniform mediump vec4 uvBounds0;

uniform mediump vec3 dirLightDirection;
uniform mediump vec3 dirLightDiffuse;

void main()
{
	highp vec4 pos = agk_World * vec4(position,1.0);
	gl_Position = agk_ViewProj * pos;

	mediump vec3 norm = normalize(agk_WorldNormal * normal);

	lightVarying = dirLightDiffuse * max(0.0, dot(norm, dirLightDirection));

	uvVarying = uv * uvBounds0.xy + uvBounds0.zw;
}
