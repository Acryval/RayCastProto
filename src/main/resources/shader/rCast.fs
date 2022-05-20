#version 330

out vec3 frag_col;

// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ UNIFORM CONSTS @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ //

uniform float time;
uniform vec2 scrRes;
uniform vec3 camPos;
uniform mat3 camRot;

// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ MATH CONSTS @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ //

#define PI 3.1415926
#define PI_INV 0.3183099
#define TWO_PI 6.2831853
#define TWO_PI_INV 0.1591549
#define HALF_PI 1.5707963
#define HALF_PI_INV 0.6366198

#define DEG_TO_RAD 0.0174533
#define RAD_TO_DEG 57.2957795

// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ SCENE FLAGS @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ //

#define ORTO false
#define PIX false

// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ SCENE CONSTS @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ //

#define PIX_SIZE 16

#define FOV 90.
//#define NEAR 1./tan(DEG_TO_RAD*FOV/2)
#define NEAR 1.
#define FAR 1000.
#define SURF .01

// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ HIT CONSTS @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ //

#define MAX_HITS 10
#define MAX_RHP_STACK 30

// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ LIGHT CONSTS @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ //

#define HALO_FACTOR 15
#define MAX_SHINE 32768
#define MIN_SHINE .05

#define MAX_ROUGH .5

#define SHADOW_TYPE 1
#define SHADOW_SCALE 1

#define MAX_REFLECTIONS 3

#define DIR_LIGHT_DIST 100.

// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ OBJ AND LIGHT TYPES @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ //

#define TYPE_UNDEF 0
#define TYPE_PLANE 1
#define TYPE_SPHERE 2

#define LTYPE_UNDEF 0
#define LTYPE_AMBIENT 1
#define LTYPE_POINT 2
#define LTYPE_DIR 3
#define LTYPE_CONE 4

// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ STRUCTURES @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ //

struct Material{
    vec3 color;

    float shine;
    float rsc;
    float rf;
    int ro;

    float transp;
    float transl;
} noMat = Material(vec3(0), 0, 0, 0, 0, 0, 0);

struct Object{
    int type;
    bool visible;

    vec3 pos;
    vec3 rot;
    vec3 scale;

    Material mat;
} noObj = Object(TYPE_UNDEF, false, vec3(0), vec3(0), vec3(0), noMat);

struct Light{
    int type;
    bool visible;

    vec3 pos;
    vec3 col;
    float value;
    float glow;
} noLight = Light(LTYPE_UNDEF, false, vec3(0), vec3(0), 0, 0);

struct Ray{
    vec3 orig;
    vec3 dir;
    bool refl;
} noRay = Ray(vec3(0), vec3(0), false);

struct Hit{
    Material m;
    float d;
    vec3 p;
    vec3 n;
    vec3 o;
} noHit = Hit(noMat, FAR, vec3(FAR), vec3(0), vec3(0));

struct RayHit{
    Ray ray;
    Hit h, ah;
} noRayHit = RayHit(noRay, noHit, noHit);

struct RHP{ // RAY_HIT_PROTOTYPE
    int hitNum;
    RayHit hit[MAX_HITS];
} noRayHitProto;

struct RHBuildStack{
    int size;
    RHP st[MAX_RHP_STACK];
} noStack;

// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ HELPER FUNC @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ //

vec3 along(Ray r, float dist){
    return r.orig + r.dir * dist;
}

Ray march(Ray r, float dist){
    return Ray(along(r, dist), r.dir, r.refl);
}

float limit(float value){
    return clamp(value, -FAR, FAR);
}

mat2 rotMat(float a){
    float sa = sin(a);
    float ca = cos(a);
    return mat2(ca, -sa, sa, ca);
}

float rnd(vec3 p){
    return 2*fract(sin((123.*p.x + 456*p.y + 789.*p.z - 69.) / max(max(max(p.x, p.y), p.z), 1))*420.) - 1.;
}

vec3 _smooth(vec3 p){
    return p*p*(3-2*p);
}

float noise_V1_3(vec3 p){
    vec2 s = vec2(0., 1.);

    vec3 cellf = _smooth(fract(p));
    vec3 celli = floor(p);

    float blf = rnd(celli + s.xxx);
    float brf = rnd(celli + s.yxx);
    float ulf = rnd(celli + s.xyx);
    float urf = rnd(celli + s.yyx);
    float blb = rnd(celli + s.xxy);
    float brb = rnd(celli + s.yxy);
    float ulb = rnd(celli + s.xyy);
    float urb = rnd(celli + s.yyy);

    float bf = mix(blf, brf, cellf.x);
    float uf = mix(ulf, urf, cellf.x);
    float bb = mix(blb, brb, cellf.x);
    float ub = mix(ulb, urb, cellf.x);

    float f = mix(bf, uf, cellf.y);
    float b = mix(bb, ub, cellf.y);

    return mix(f, b, cellf.z);
}

vec2 noise_V2_3(vec3 p){
    return vec2(noise_V1_3(p), noise_V1_3(-p));
}

float noise_S1_3(vec3 p){
    return noise_V1_3(mat3(1, -0.5773503, -0.4714045, 0, 1.1547005, -0.4714045, 0, 0, 1.4142136)*p);
}

vec2 noise_S2_3(vec3 p){
    return noise_V2_3(mat3(1, -0.5773503, -0.4714045, 0, 1.1547005, -0.4714045, 0, 0, 1.4142136)*p);
}

// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ HIT HELPER FUNC @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ //

bool isNoHit(RayHit r){
    return r.h.d >= FAR;
}

bool isInside(RayHit r){
    return r.h.d <= 0;
}

RayHit closestHit(RHP rhp){
    RayHit ret = noRayHit;

    for(int i = 0; i < rhp.hitNum; i++){
        if(ret.h.d > rhp.hit[i].h.d){
            ret = rhp.hit[i];
        }
    }

    return ret;
}

RHP fromRH(RayHit rh){
    RHP ret = noRayHitProto;
    ret.hitNum = 1;
    ret.hit[0] = rh;
    return ret;
}

// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ OBJ COMPOSITION FUNCTIONS @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ //
// ---------------------------------------------------- TEMPLATE ---------------------------------------------------- //
/*
RayHit FUNC_NAME(RayHit r1, RayHit r2){
    RayHit ret = noRayHit;
    ret.ray = r1.ray;

    if(isNoHit(r1)){
        if(isNoHit(r2)){
            // none hit

        }else{
            // only r2 hit

        }
    }else if(isNoHit(r2)){
        // only r1 hit

    }else if(r1.h.d < r2.h.d){
        if(r1.ah.d < r2.h.d){
            // #1 | | : :

        }else if(r1.ah.d < r2.ah.d){
            // #2 | : | :

        }else{
            // #3 | : : |

        }
    }else if(r1.ah.d < r2.ah.d){
        // #4 : | | :

    }else if(r1.h.d < r2.ah.d){
        // #5 : | : |

    }else{
        // #6 : : | |

    }
}
*/

// ----------------------------------------------------- UNION ------------------------------------------------------ //

RayHit uni(RayHit r1, RayHit r2){
    RayHit ret = noRayHit;
    ret.ray = r1.ray;

    if(isNoHit(r1)){
        if(isNoHit(r2)){
            // none hit
            return ret;
        }else{
            // only r2 hit
            return r2;
        }
    }else if(isNoHit(r2)){
        // only r1 hit
        return r1;
    }else if(r1.h.d <= r2.h.d){
        if(r1.ah.d < r2.h.d){
            // #1 | | : :
            return r1;
        }else if(r1.ah.d < r2.ah.d){
            // #2 | : | :
            ret.h = r1.h;
            ret.ah = r2.ah;
            return ret;
        }else{
            // #3 | : : |
            return r1;
        }
    }else if(r1.ah.d < r2.ah.d){
        // #4 : | | :
        return r2;
    }else if(r1.h.d <= r2.ah.d){
        // #5 : | : |
        ret.h = r2.h;
        ret.ah = r1.ah;
        return ret;
    }else{
        // #6 : : | |
        return r2;
    }
}

// -------------------------------------------------- INTERSECTION -------------------------------------------------- //

RayHit inter(RayHit r1, RayHit r2){
    RayHit ret = noRayHit;
    ret.ray = r1.ray;

    if(isNoHit(r1) || isNoHit(r2)){
        return ret;
    }else if(r1.h.d < r2.h.d){
        if(r1.ah.d <= r2.h.d){
            // #1 | | : :
            return ret;
        }else if(r1.ah.d <= r2.ah.d){
            // #2 | : | :
            ret.h = r2.h;
            ret.ah = r1.ah;
            return ret;
        }else{
            // #3 | : : |
            return r2;
        }
    }else if(r1.ah.d <= r2.ah.d){
        // #4 : | | :
        return r1;
    }else if(r1.h.d < r2.ah.d){
        // #5 : | : |
        ret.h = r1.h;
        ret.ah = r2.ah;
        return ret;
    }else{
        // #6 : : | |
        return ret;
    }
}

// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ DATA INIT @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ //

Material sph_mat = Material(vec3(1, 0, 0), .2, 1, 10., 1, 0, 0);
Material sph2_mat = Material(vec3(0, 0, 1), .95, .2, 30., 3, 0, 0);
Material pln_mat = Material(vec3(0.1372, 0.5372, 0.8549), .8, 1., .5, 5, 0, 1);

Object sphere = Object(TYPE_SPHERE, true, vec3(0, 1, 6), vec3(0), vec3(1), sph_mat);
Object sph2 = Object(TYPE_SPHERE, true, vec3(.8, 0.5 + sin(time), 6), vec3(0), vec3(.75), sph2_mat);
Object plane = Object(TYPE_PLANE, true, vec3(0, -1, time), normalize(vec3(0, 1, 0)), vec3(0), pln_mat);

Light l_ambient = Light(LTYPE_AMBIENT, true, vec3(0), vec3(0, 0.03, 0.07), 1, 0);
Light l_pt1 = Light(LTYPE_POINT, true, vec3(.6, 0.5 - sin(time), 6), vec3(1), 1, 0.1);
Light l_pt2 = Light(LTYPE_POINT, true, vec3(1 - 2*sin(3*time), 2.5, 6 + cos(time/2)), vec3(1, .5, .2), 1, 0.2);
Light daylight = Light(LTYPE_DIR, true, vec3(3, 3, 4), vec3(0.5, 1, 0), 1, 0.5);
Light nightlight = Light(LTYPE_DIR, true, vec3(sin(time/100), .1, cos(time/100)), vec3(1), .8, .3);

// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ OBJECT HIT FUNC @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ //
// ----------------------------------------------------- PLANE ------------------------------------------------------ //

RayHit planeHit(Ray r, Object pln){
    RayHit ret = noRayHit;
    ret.ray = r;

    float d = -dot(r.dir, pln.rot);
    float D = dot(vec4(pln.rot, dot(pln.pos, pln.rot)), vec4(r.orig, -1));

    if(D <= 0){
        if(d > 0){
            ret.h.d = limit(D/d);
            ret.ah.d = FAR;
        }else if(d == 0){
            ret.h.d = -FAR;
            ret.ah.d = FAR;
        }else{
            ret.h.d = -FAR;
            ret.ah.d = limit(D/d);
        }
    }else{
        if(d > 0){
            ret.h.d = limit(D/d);
            ret.ah.d = FAR;
        }else{
            return ret;
        }
    }

    ret.h.m = pln.mat;
    ret.h.n = pln.rot;
    ret.h.p = along(r, ret.h.d);
    ret.h.o = pln.pos;

    ret.ah.m = pln.mat;
    ret.ah.n = pln.rot;
    ret.ah.p = along(r, ret.ah.d);
    ret.ah.o = pln.pos;

    return ret;
}

// ----------------------------------------------------- SPHERE ----------------------------------------------------- //

RayHit sphereHit(Ray r, Object sph){
    RayHit ret = noRayHit;
    ret.ray = r;

    vec3 R = sph.pos - r.orig;

    if(length(R) >= FAR){
        return ret;
    }

    float r2 = sph.scale.x * sph.scale.x;
    float R2 = dot(R, R);
    float Rd = dot(R, r.dir);

    if(Rd < 0 && r2 < R2){
        return ret;
    }

    float sr2 = R2 - Rd*Rd;

    if(sr2 >= r2){
        return ret;
    }

    float s = sqrt(r2 - sr2);
    float dist = limit(Rd - s);

    ret.h.m = sph.mat;
    ret.h.d = dist;
    ret.h.p = along(r, dist);
    ret.h.n = normalize(ret.h.p - sph.pos);
    ret.h.o = sph.pos;

    dist = limit(Rd + s);

    ret.ah.m = sph.mat;
    ret.ah.d = dist;
    ret.ah.p = along(r, dist);
    ret.ah.n = normalize(ret.ah.p - sph.pos);
    ret.ah.o = sph.pos;

    return ret;
}

// ---------------------------------------------- GENERIC OBJ HIT FUNC ---------------------------------------------- //

RayHit objHit(Ray r, Object o){
    RayHit ret = noRayHit;
    ret.ray = r;

    if(!o.visible){
        return ret;
    }

    switch(o.type){
        case TYPE_PLANE:
            return planeHit(r, o);
        case TYPE_SPHERE:
            return sphereHit(r, o);
        default:
            return ret;
    }
}

// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ SCENE BUILDING FUNC @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ //
// ---------------------------------------------------- RAY CAST ---------------------------------------------------- //

RayHit rCast(Ray r){
    RayHit ret = noRayHit;
    ret.ray = r;

    ret = uni(ret, objHit(r, plane));
    ret = uni(ret, objHit(r, sphere));
    ret = uni(ret, objHit(r, sph2));

    return ret;
}

vec3 perturbe(Hit h, vec3 dir){
    vec3 sn = cross(dir, h.n);
    vec3 sb = cross(dir, sn);

    int k = 1;
    vec2 n = vec2(0);
    for(int i = 0; i < h.m.ro; i++){
        n += (1 + sin(time*(k-1)))*noise_S2_3(k*h.m.rf*(h.p - h.o)) / float(k);
        k <<= 1;
    }

    n *= MAX_ROUGH*h.m.rsc*0.25;
    return n.x*sn + n.y*sb;
}

RayHit reflectRayHit(RayHit hit){
    vec3 R = normalize(hit.ray.dir - 2*hit.h.n*dot(hit.h.n, hit.ray.dir));
    return rCast(Ray(hit.h.p + SURF*hit.h.n, R, true));
}

// ----------------------------------------------- LIGHT COMPUTATION ------------------------------------------------ //

vec3 lightFunc(RayHit hit, Light l){
    vec3 color = l.value * l.col;

    if(!l.visible || l.value == 0){
        return vec3(0);
    }

    vec3 lightPos;
    vec3 lightDir;
    float lightDist;

    RayHit rh;
    float light_intensity = 0;

    float halo_value = 0;
    float diffuse_value = 0;
    float specular_value = 0;
    bool inh = isNoHit(hit);

    switch(l.type){
        case LTYPE_UNDEF:
            return vec3(0);
        case LTYPE_AMBIENT:
            return color;
        case LTYPE_POINT:
            lightPos = l.pos;
            lightDir = l.pos - hit.h.p;
            lightDist = length(lightDir);
            lightDir /= lightDist;

            if(!hit.ray.refl){
                vec3 toOrigDir = lightPos - hit.ray.orig;
                float toOrigDist = length(toOrigDir);
                toOrigDir /= toOrigDist;

                if(rCast(Ray(hit.ray.orig, toOrigDir, true)).h.d > toOrigDist){
                    halo_value = pow(clamp(dot(toOrigDir, hit.ray.dir), 0, 1), MAX_SHINE >> int(HALO_FACTOR*l.glow)) / toOrigDist;
                }
            }
            break;
        case LTYPE_DIR:
            lightDir = normalize(l.pos);
            lightDist = DIR_LIGHT_DIST;
            lightPos = along(Ray(hit.h.p, lightDir, true), DIR_LIGHT_DIST);

            if(!hit.ray.refl && inh){
                halo_value = pow(clamp(dot(lightDir, hit.ray.dir), 0, 1), MAX_SHINE >> int(HALO_FACTOR*l.glow));
            }
            break;
        case LTYPE_CONE:
        default:
            return color;
    }

    if(inh){
        return clamp(color*halo_value, 0, 1);
    }

    switch(SHADOW_TYPE){
        case 0:

            //  SIMPLE SHADING
            rh = rCast(Ray(hit.h.p + hit.h.n*SURF, lightDir, true));
            light_intensity = rh.h.d / lightDist;

            if(light_intensity < 1){
                light_intensity *= 1 - light_intensity;
            }else{
                light_intensity = 1;
            }

            break;
        case 1:
            //  DEPTH SHADING
            rh = rCast(Ray(hit.h.p + hit.h.n*SURF, lightDir, true));
            float limitDist = lightDist - SURF;
            while(rh.h.d < limitDist){
                light_intensity += (min(rh.ah.d, limitDist) - rh.h.d)*(rh.h.m.transl - 1);
                limitDist += SURF - rh.ah.d;
                if(limitDist <= 0){
                    break;
                }

                rh = rCast(Ray(along(rh.ray, rh.ah.d + SURF), lightDir, true));
            }
            light_intensity = exp(SHADOW_SCALE * light_intensity);
            break;
        default:
            break;
    }

    diffuse_value = clamp(light_intensity * dot(lightDir, hit.h.n), 0, 1);
    specular_value = pow(clamp(light_intensity * dot(normalize(lightDir - hit.ray.dir), hit.h.n), 0, 1), int(MAX_SHINE * hit.h.m.shine) >> int(HALO_FACTOR*l.glow));

    color *= hit.h.m.color * (diffuse_value + hit.h.m.shine * (specular_value - diffuse_value)) + halo_value;

    return clamp(color, 0, 1);
}

// --------------------------------------------------- LIGHT CAST --------------------------------------------------- //

vec3 lCast(RayHit hit){
    vec3 col = lightFunc(hit, l_ambient);

    if(isInside(hit)){
        return col;
    }

    vec3 t_col;
    float s = 1.;

    hit.h.n = normalize(hit.h.n + perturbe(hit.h, hit.ray.dir));
    hit.ah.n = normalize(hit.ah.n + perturbe(hit.ah, hit.ray.dir));

    for(int i = 0; i < MAX_REFLECTIONS; i++){
        if(i != 0){
            hit = reflectRayHit(hit);
            hit.h.n = normalize(hit.h.n + perturbe(hit.h, hit.ray.dir));
            hit.ah.n = normalize(hit.ah.n + perturbe(hit.ah, hit.ray.dir));
        }

        t_col = vec3(0);


        //t_col += lightFunc(hit, l_pt1);
        //t_col += lightFunc(hit, l_pt2);

        //t_col += lightFunc(hit, daylight);
        t_col += lightFunc(hit, nightlight);

        col += s*t_col;

        if(isNoHit(hit)){
            break;
        }

        s *= hit.h.m.shine;
        if(s < MIN_SHINE){
            break;
        }
    }

    return clamp(col, 0, 1);
}

// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ MAIN @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ //

void main(){
    vec2 uv = gl_FragCoord.xy - 0.5*scrRes;

    if(PIX){
        uv = floor(uv / PIX_SIZE) * PIX_SIZE;
    }

    uv /= scrRes.y;

    vec3 ro;
    vec3 rd;

    if(ORTO){
        ro = camPos + NEAR*camRot*vec3(uv, 0);
        rd = camRot*vec3(0, 0 ,1);
    }else{
        ro = camPos;
        rd = camRot*normalize(vec3(uv, NEAR));
    }

    noRayHitProto.hitNum = 0;
    for(int i = 0; i < MAX_HITS; i++){
        noRayHitProto.hit[i] = noRayHit;
    }

    noStack.size = 0;
    for(int i = 0; i < MAX_RHP_STACK; i++){
        noStack.st[i] = noRayHitProto;
    }

    frag_col = lCast(rCast(Ray(ro, rd, false)));
    //frag_col = vec3(rCast(Ray(ro, rd, false)).h.d / FAR); // no-light, distance based env
}