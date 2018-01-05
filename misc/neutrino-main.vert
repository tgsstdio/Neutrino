#version 450

#extension GL_ARB_shading_language_420pack : require

precision highp float;

struct CameraUBO
{
	mat4 ProjectionMatrix;
	mat4 ViewMatrix;
	vec4 CameraPosition;
}

layout (std140, binding = 0) uniform UBOData0 
{
	CameraUBO Cameras[1];
} ubo0;

// PER VERTEX
layout(location = 0) in vec3 inPosition;
layout(location = 1) in vec3 inNormal;
layout(location = 2) in vec4 inTangent;
layout(location = 3) in vec2 inTexCoords0;
layout(location = 4) in vec2 inTexCoords1;
layout(location = 5) in vec4 inColor0;
layout(location = 6) in vec4 inColor1;
layout(location = 7) in ivec4 inJoints0;
layout(location = 8) in ivec4 inJoints1;
layout(location = 9) in vec4 inWeights0;
layout(location = 10) in vec4 inWeights1;

// PER INSTANCE
layout(location = 11) in vec3 inTranslation;
layout(location = 11) in vec3 inScale;
layout(location = 11) in vec4 inRotation;
layout(location = 11) in int inCameraIndex;
layout(location = 11) in int inMaterialIndex;

mat4 quatToMat4(vec4 q) {
	// Normalized quaternion
	// http://www.euclideanspace.com/maths/geometry/rotations/conversions/quaternionToMatrix/index.htm

	mat4 a;
	mat4 b;
	
	// column then row order
	a[0][0] = a[1][1] = a[2][2] = a[3][3] = b[0][0] = b[1][1] = b[2][2] = b[3][3] =  q.w;
	a[3][0] = a[2][1] = b[0][3] = b[2][1] = q.x;
	a[0][3] = a[1][2] = b[3][0] = b[1][2] = -q.x;
	a[3][1] = a[0][2] = b[0][2] = b[1][3] = q.y;
	a[2][0] = a[1][3] = b[2][0] = b[3][1] = -q.y;
	a[1][0] = a[3][2] = b[1][0] = b[2][3] = q.z;
	a[0][1] = a[2][3] = b[0][1] = b[3][2] = -q.z;
	
	return a * b;
}

// varying frag variables
out vec2 texCoords0;
out vec2 texCoords1;
out vec4 color0;
out vec4 color1;
out int materialIndex;
out vec3 cameraPos;
out vec3 vertexPos;

out mat3 v_TBN;

void vertFunc(void)
{
  texCoords0 = inTexCoords0;
  texCoords1 = inTexCoords1;  
  color0 = inColor0;
  color1 = inColor1;  
  materialIndex = inMaterialIndex;
  
  // TODO: figure this out
  cameraPos = ubo0.Cameras[0].CameraPosition;
  
  mat4 rotMatrix = quatToMat4(inRotation);
  
  vec4 localPosition = vec4(inTranslation, 0) + rotMatrix * vec4(inScale * inPosition, 1);  
  vertexPos = localPosition.xyz;

  vec3 normalW = normalize(vec3(rotMatrix * vec4(inNormal.xyz, 0.0)));
  vec3 tangentW = normalize(vec3(rotMatrix * vec4(inTangent.xyz, 0.0)));
  vec3 bitangentW = cross(normalW, tangentW) * inTangent.w;
  v_TBN = mat3(tangentW, bitangentW, normalW);  
  
  gl_Position = ubo0.Cameras[0].ProjectionMatrix * ubo0.Cameras[0].ViewMatrix * localPosition;
  
  // VULKAN => OPENGL
  gl_Position.y *= -1.0;
}