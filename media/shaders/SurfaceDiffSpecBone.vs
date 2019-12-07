#version 330

in vec3 position;
in vec3 normal;
in vec2 uv;
in vec4 boneweights;
in vec4 boneindices;

out vec3 v_position;
out mat3 v_normal;
out vec2 v_uvCoord;

//uniform highp mat4 agk_World;
uniform mat3 agk_WorldNormal;
uniform mat4 agk_ViewProj;
uniform vec4 agk_bonequats1[30];
uniform vec4 agk_bonequats2[30];
uniform vec4 uvBounds0;

void main()
{
	//Position
	vec4 q1 = agk_bonequats1[int(boneindices.x)] * boneweights.x;
	q1 += agk_bonequats1[int(boneindices.y)] * boneweights.y;
	q1 += agk_bonequats1[int(boneindices.z)] * boneweights.z;
	q1 += agk_bonequats1[int(boneindices.w)] * boneweights.w;

	vec4 q2 = agk_bonequats2[int(boneindices.x)] * boneweights.x;
	q2 += agk_bonequats2[int(boneindices.y)] * boneweights.y;
	q2 += agk_bonequats2[int(boneindices.z)] * boneweights.z;
	q2 += agk_bonequats2[int(boneindices.w)] * boneweights.w;

	float len = 1.0/length(q1);
	q1 *= len;
	q2 = (q2 - q1*dot(q1,q2)) * len;
	vec3 p = position + (2.0 * cross(q1.xyz, cross(q1.xyz, position) + q1.w * position));
		p += 2.0 * ((q1.w * q2.xyz) - (q2.w * q1.xyz) + cross(q1.xyz, q2.xyz));
	vec4 pos = vec4( p, 1.0 );

	gl_Position = agk_ViewProj * pos;
	v_position = pos.xyz;

	//UV
	v_uvCoord = uv * uvBounds0.xy + uvBounds0.zw;

	//Normal
	vec3 tangent;
	if( abs(normal.y) > 0.999 )
	{
		tangent = vec3(normal.y, 0.0, 0.0);
	}
	else
	{
		tangent = normalize(vec3(-normal.z, 0.0, normal.x));
	}
	vec3 n = normalize(agk_WorldNormal * normal);
	vec3 t = normalize(agk_WorldNormal * tangent);
	vec3 b = normalize(cross(t, n));
	v_normal = mat3(t, b, n);
}
