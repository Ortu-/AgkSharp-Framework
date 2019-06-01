// Shader by GaborD
// https://forum.thegamecreators.com/thread/223376

attribute vec3 position;
attribute mediump vec2 uv;

uniform mat4 agk_ViewProj;
uniform vec3 agk_CameraPos;

uniform vec4 pos1;
uniform vec4 pos2;
uniform float thickness;

void main(){
	vec4 pos = vec4(mix(pos1.xyz,pos2.xyz,step(0.5,uv.x)),1);
	pos.xyz += cross(normalize(agk_CameraPos-pos.xyz),normalize(pos2.xyz-pos1.xyz))*thickness*uv.y;
	gl_Position = agk_ViewProj * pos;
}