#version 450

#extension GL_ARB_separate_shader_objects : enable
#extension GL_ARB_shading_language_420pack : enable

struct LightUBO
{
	vec4 color;
	vec4 direction;
};

struct CameraUBO {
	mat4 modelViewProjection;
	vec3 position;
};

struct Material {

};

layout (binding = 0) uniform UBO 
{
	LightUBO lights[1];
	CameraUBO cameras[1];
	Material materials[1];
} ubo;

layout (location = 0) in vec3	vertPosition;
layout (location = 1) in vec3	vertNormal;
layout (location = 2) in vec3	vertTangent;
layout (location = 3) in vec2	vertTexCoords0;
layout (location = 4) in vec2	vertTexCoords1;
layout (location = 5) in vec4	vertColor0;
layout (location = 6) in vec4	vertColor1;
layout (location = 7) in ivec4	vertJoints0;
layout (location = 8) in ivec4	vertJoints1;
layout (location = 9) in vec4	vertWeights0;
layout (location = 10) in vec4	vertWeights1;

layout (location = 11) in vec3	instTranslation;
layout (location = 12) in vec3	instScale;
layout (location = 13) in vec4	instRotation;
layout (location = 14) in int	instCameraIndex;
layout (location = 15) in int	instMaterialIndex;
layout (location = 16) in int	instLightIndex;

layout (location = 0) out vec3	voutPosition;
layout (location = 1) out vec3	voutColor;
layout (location = 3) out int	voutCameraIndex;
layout (location = 4) out int	voutMaterialIndex;
layout (location = 5) out int	voutLightIndex;

mat4 quatToMat4(vec4 q) {
	mat4 a = mat4(
		 vec4( q.w, -q.z,  q.y, -q.x)
		,vec4( q.z,  q.w, -q.x, -q.y)
		,vec4(-q.y,  q.x,  q.w, -q.z)
		,vec4( q.x,  q.y,  q.z,  q.w)
	);
	
	mat4 b = mat4(
		 vec4( q.w, -q.z,  q.y,  q.x)
		,vec4( q.z,  q.w, -q.x,  q.y)
		,vec4(-q.y,  q.x,  q.w,  q.z)
		,vec4(-q.x, -q.y, -q.z,  q.w)
	);		
	
	return a * b;
}


out gl_PerVertex 
{
    vec4 gl_Position;   
};


void main() 
{

	vec4 t = vec4(instTranslation, 1.0);
	mat4 r = quatToMat4(instRotation);
	vec4 s = vec4(instScale, 1.0);

	mat4 modelMatrix = t * r * s;

	vec4 pos = modelMatrix * vertPosition;
	voutPosition = vec3(pos.xyz) / pos.w;
	voutColor = vertColor;

	// 
	voutCameraIndex = instCameraIndex;
	voutMaterialIndex = instMaterialIndex;
	voutLightIndex = instLightIndex;
	
	mat4 mvpMatrix = ubo[instCameraIndex].modelViewProjection;

	// VULKAN
	gl_Position = mvpMatrix * vec4(voutPosition, 1.0);
}
