#version 330

uniform vec2 scrRes;
uniform float time;

out vec4 frag_col;

#define MAX_STEPS $1$
#define MAX_DIST $2$
#define SURF $3$
#define EPS vec2($4$, .0)
#define ANGLE_MODE $5$

struct ManifoldData{
	vec3 position;
	vec3 rotation;
	vec3 scale;
	int visible;
};

$7$
float uni(float a, float b){
	if(a > b){
		return b;
	}
	return a;
}

float inter(float a, float b){
	if(a > b){
		return a;
	}
	return b;
}

float subs(float a, float b){
	return inter(a, -b);
}

mat2 rot(float ang){
	ang *= ANGLE_MODE;
	float c = cos(ang);
	float s = sin(ang);
	return mat2(c, s, -s, c);
}

float sphDist(vec3 p, vec4 sph){
    return length(sph.xyz - p) - sph.w;
}

$6$float distFun(vec3 p, int lightPass){
	float d = MAX_DIST;
	
	$8$
	$9$
	$10$
	return d;
}

float RM(vec3 o, vec3 d, int lightPass){
	float dO = 0;
	
	for(int i = 0; i < MAX_STEPS; i++){
		vec3 p = o + d*dO;
		float ds = distFun(p, lightPass);
		dO += ds;
		if(dO > MAX_DIST || ds < SURF) break;
	}
	
	return dO;
}

vec3 surfNormal(vec3 p){
	float d = distFun(p, 0);
	vec2 e = EPS;
	vec3 n = vec3(d) - vec3(distFun(p - e.xyy, 0), distFun(p - e.yxy, 0), distFun(p - e.yyx, 0));
	return normalize(n);
}

float lightFun(vec3 p){
	vec3 lightPos = vec3(3*cos(time), 3, 6 + 3*sin(time));
	
	vec3 lightDir = lightPos - p;
	float lightDist = length(lightDir);
	lightDir = normalize(lightDir);
	
	vec3 norm = surfNormal(p);
	float d = RM(p + 1.3*SURF*norm, lightDir, 1)/lightDist;
	
	float lightVal = 1.;
	if(d < 1){
		lightVal *= d*(1-d);
	}
	
	lightVal = clamp(lightVal * dot(lightDir, norm), 0., 1.);
	
	return lightVal;
}

void main(){
	vec2 uv = (gl_FragCoord.xy - 0.5*scrRes) / scrRes.y;
	
	vec3 ro = vec3(0, 1, 0);
	vec3 rd = normalize(vec3(uv, 1));
	
	float d = RM(ro, rd, 0);
	vec3 p = ro + rd*d;
	float dif = lightFun(p);
	vec3 col = vec3(dif);
	
	frag_col = vec4(col, 1);
}