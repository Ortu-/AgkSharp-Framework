
attribute highp vec3 position;
attribute mediump vec3 normal;
attribute mediump vec2 uv;

varying highp vec3 v_position;
varying mediump mat3 v_normal;
varying highp vec2 v_uvCoord;

uniform highp mat4 agk_World;
uniform highp mat3 agk_WorldNormal;
uniform highp mat4 agk_ViewProj;
uniform mediump vec4 uvBounds0;

void main()
{
	//Position
	highp vec4 p = agk_World * vec4(position, 1.0);
	gl_Position = agk_ViewProj * p;
	v_position = p.xyz;

	//UV
	v_uvCoord = uv * uvBounds0.xy + uvBounds0.zw;

	//Normal
	mediump vec3 tangent;
	if( abs(normal.y) > 0.999 )
	{
		tangent = vec3(normal.y, 0.0, 0.0);
	}
	else
	{
		tangent = normalize(vec3(-normal.z, 0.0, normal.x));
	}
	mediump vec3 n = normalize(agk_WorldNormal * normal);
	mediump vec3 t = normalize(agk_WorldNormal * tangent);
	mediump vec3 b = normalize(cross(t, n));
	v_normal = mat3(t, b, n);
}
