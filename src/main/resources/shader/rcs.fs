#version 330
out vec3 frag_col;
// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ UNIFORMS CONSTS @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ //
uniform float time;
uniform vec2 scrRes;
uniform vec3 camPos;
uniform mat3 camRot;
// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ MATH CONSTS @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ //
#define PI 3.1415926
#define HALF_PI 1.5707963
#define DEG_TO_RAD 0.0174533
#define RAD_TO_DEG 57.2957795
// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ SCENE CONSTS @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ //
#define PIX_SIZE $3$
#define NEAR $9$
#define FAR $10$
#define SCR_SIZE $15$
#define SURF $11$
// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ SCENE FLAGS @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ //
#define ORTO $2$
#define PIX $1$
// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ LIGHT CONSTS @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ //
#define HALO_FACTOR $4$
#define MAX_SHINE $5$
#define MIN_REFLECTIVITY $12$
#define MAX_REFLECTIONS $6$

#define SHADOW_OFF 0
#define SHADOW_SIMPLE 1
#define SHADOW_DEPTH 2

#define SHADOW_SCALE $13$
#define MAX_SHADOW_DEPTH $7$

#define DIR_LIGHT_DIST $14$
// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ LIGHT FLAGS @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ //
#define LIGHTMODE_DISTANCE $8$
// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ OBJ TYPES @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ //
#define UNDEFINED 0
#define SPHERE 1
#define PLANE 2
#define CUBOID 3
// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ LIGHT TYPES @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ //
#define AMBIENT 0
#define POINT 1
#define DIRECTIONAL 2
#define CONE 3
// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ STRUCTURES @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ //
struct Material{
    bool reflective;
    bool opaque;
    bool halo;
    int shadowMode;
    float reflectivity;
    float density;
    vec3 col;
} noMat = Material(false, false, false, SHADOW_OFF, 0, 1, vec3(0.9961f, 0.0039f, 0.6941f));// ----------------------- //
struct ObjData{
    bool visible;
    int type;
    vec3 pos;
    vec3 rot;
    vec3 scale;
} noData = ObjData(false, UNDEFINED, vec3(0), vec3(0), vec3(0));// -------------------------------------------------- //
struct Object{
    ObjData obj;
    Material mat;
} noObj = Object(noData, noMat);// ---------------------------------------------------------------------------------- //
struct Light{
    bool on;
    bool doesGlow;
    int type;
    float intensity;
    float glow;
    vec3 pos;
    vec3 dir;
    vec3 col;
} noLight = Light(false, false, UNDEFINED, 0, 0, vec3(0), vec3(0), vec3(0));// -------------------------------------- //
struct Ray{
    vec3 origin;
    vec3 direction;
} noRay = Ray(vec3(0), vec3(0));// ---------------------------------------------------------------------------------- //
struct RayHit{
    Ray ray;
    bool hit;
    float dist;
    float dist2;
    vec3 point;
    vec3 point2;
    vec3 normal;
    vec3 normal2;
    Material mat;
    Material altmat;
} noRayHit = RayHit(noRay, false, FAR, FAR, vec3(FAR), vec3(FAR), vec3(FAR), vec3(FAR), noMat, noMat);
// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ UTILITY FUNCTIONS @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ //
float limit(float a){
    return clamp(a, -FAR, FAR);
}// ----------------------------------------------------------------------------------------------------------------- //
mat2 rotMat2(float a){
    float _sin = sin(a);
    float _cos = cos(a);
    return mat2(_cos, -_sin, _sin, _cos);
}// ----------------------------------------------------------------------------------------------------------------- //
vec3 rotVec3(vec3 v, vec3 a){
    v.xy *= rotMat2(a.z);
    v.zx *= rotMat2(a.y);
    v.yz *= rotMat2(a.x);
    return v;
}// ----------------------------------------------------------------------------------------------------------------- //
vec3 along(Ray r, float dist){
    return r.origin + dist * r.direction;
}// ----------------------------------------------------------------------------------------------------------------- //
bool isInside(RayHit rh){
    return rh.dist * rh.dist2 <= SURF;
}// ----------------------------------------------------------------------------------------------------------------- //
bool isNoHit(RayHit rh){
    return !rh.hit || rh.dist >= FAR;
}// ----------------------------------------------------------------------------------------------------------------- //
Ray reflect(Ray r, vec3 normal, vec3 point){
    vec3 reflected_dir = normalize(r.direction - 2*normal*dot(normal, r.direction));
    return Ray(point + SURF * normal, reflected_dir);
}
// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ COMBINATION FUNCTIONS @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ //
RayHit unify(RayHit r1, RayHit r2){
    if(isNoHit(r1)){
        return r2;
    }else if(isNoHit(r2)){
        return r1;
    }else if(r1.dist < r2.dist){
        if(r1.dist2 < r2.dist){
            // | | : :
            return r1;
        }else if(r1.dist2 < r2.dist2){
            // | : | :
            r1.dist2   = r2.dist2;
            r1.point2  = r2.point2;
            r1.normal2 = r2.normal2;
            r1.altmat  = r2.altmat;
            return r1;
        }else{
            // | : : |
            return r1;
        }
    }else{
        if(r2.dist2 < r1.dist){
            // : : | |
            return r2;
        }else if(r2.dist2 < r1.dist2){
            // : | : |
            r2.dist2   = r1.dist2;
            r2.point2  = r1.point2;
            r2.normal2 = r1.normal2;
            r2.altmat  = r1.altmat;
            return r2;
        }else{
            // : | | :
            return r2;
        }
    }
}// ----------------------------------------------------------------------------------------------------------------- //
RayHit intersect(RayHit r1, RayHit r2){
    RayHit noHit = noRayHit;
    noHit.ray = r1.ray;

    if(isNoHit(r1) || isNoHit(r2)){
        return noHit;
    }else if(r1.dist < r2.dist){
        if(r1.dist2 < r2.dist){
            // | | : :
            return noHit;
        }else if(r1.dist2 < r2.dist2){
            // | : | :
            r2.dist2   = r1.dist2;
            r2.point2  = r1.point2;
            r2.normal2 = r1.normal2;
            r2.altmat  = r1.altmat;
            return r2;
        }else{
            // | : : |
            return r2;
        }
    }else{
        if(r2.dist2 < r1.dist){
            // : : | |
            return noHit;
        }else if(r2.dist2 < r1.dist2){
            // : | : |
            r1.dist2   = r2.dist2;
            r1.point2  = r2.point2;
            r1.normal2 = r2.normal2;
            r1.altmat  = r2.altmat;
            return r1;
        }else{
            // : | | :
            return r1;
        }
    }
}// ----------------------------------------------------------------------------------------------------------------- //
RayHit subtract(RayHit r1, RayHit r2){
    RayHit noHit = noRayHit;
    noHit.ray = r1.ray;

    if(isNoHit(r1)){
        return noHit;
    }else if(isNoHit(r2)){
        return r1;
    }else if(r1.dist < r2.dist){
        if(r1.dist2 < r2.dist){
            // | | : :
            return r1;
        }else if(r1.dist2 < r2.dist2){
            // | : | :
            r1.dist2   = r2.dist;
            r1.point2  = r2.point;
            r1.normal2 = r2.normal;
            r1.altmat  = r2.mat;
            return r1;
        }else{
            // | : : |
            return r1;
        }
    }else{
        if(r2.dist2 < r1.dist){
            // : : | |
            return r1;
        }else if(r2.dist2 < r1.dist2){
            // : | : |
            r1.dist   = r2.dist2;
            r1.point  = r2.point2;
            r1.normal = r2.normal2;
            r1.mat    = r2.altmat;
            return r1;
        }else{
            // : | | :
            return noHit;
        }
    }
}// ----------------------------------------------------------------------------------------------------------------- //
// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ OBJ FUNCTIONS @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ //
RayHit objSphere(Ray r, Object o){
    RayHit ret = noRayHit;
    ret.ray = r;

    ObjData obj = o.obj;

    // obj.pos - position of the center of the sphere
    // obj.rot - ignored
    // obj.scale - it's length is the radius of the sphere

    vec3 toCenter = obj.pos - r.origin;
    float toCenterSq = dot(toCenter, toCenter);

    if(toCenterSq >= FAR*FAR){
        return ret;
    }

    float radiusSq = dot(obj.scale, obj.scale);
    float origToProjection = dot(toCenter, r.direction);

    if(toCenterSq > radiusSq && origToProjection < 0){
        return ret;
    }

    float centerToProjection = toCenterSq - origToProjection*origToProjection;

    if(centerToProjection >= radiusSq){
        return ret;
    }

    ret.hit = true;

    float projectionToHit = sqrt(radiusSq - centerToProjection);

    ret.dist = limit(origToProjection - projectionToHit);
    ret.point = along(r, ret.dist);
    ret.normal = normalize(ret.point - obj.pos);
    ret.mat = o.mat;

    ret.dist2 = limit(origToProjection + projectionToHit);
    ret.point2 = along(r, ret.dist2);
    ret.normal2 = normalize(ret.point2 - obj.pos);
    ret.altmat = o.mat;

    return ret;
}// ----------------------------------------------------------------------------------------------------------------- //
RayHit objPlane(Ray r, Object o){
    RayHit ret = noRayHit;
    ret.ray = r;
    ObjData obj = o.obj;

    // obj.pos - position of some point in the center of the plane
    // obj.rot - plane's normal vector (normalized)
    // obj.scale - it's length is the plane's thickness

    float directionConvergence = -dot(obj.rot, r.direction);
    float distToPlane = dot(obj.rot, r.origin - obj.pos);
    float thick = length(obj.scale) / 2;

    float d1, d2;
    vec3 tempRot;

    if(directionConvergence > 0){
        d1 = distToPlane - thick;
        d2 = distToPlane + thick;
        tempRot = obj.rot;
    }else{
        d1 = distToPlane + thick;
        d2 = distToPlane - thick;
        tempRot = -obj.rot;
    }

    if(d2 * directionConvergence > 0.0) {
        ret.hit = true;

        ret.dist = limit(d1 / directionConvergence);
        ret.point = along(r, ret.dist);
        ret.normal = tempRot;
        ret.mat = o.mat;

        ret.dist2 = limit(d2 / directionConvergence);
        ret.point2 = along(r, ret.dist2);
        ret.normal2 = -tempRot;
        ret.altmat = o.mat;
    }

    return ret;
}// ----------------------------------------------------------------------------------------------------------------- //
RayHit objCuboid(Ray r, Object o){
    RayHit noHit = noRayHit;
    noHit.ray = r;
    ObjData obj = o.obj;

    // obj.pos - position of the center of the cuboid
    // obj.rot - rotation angles about X, Y, Z axis
    // obj.scale - dimensions of the cuboid

    vec3 s = 0.5*obj.scale;

    Object tempObj = o;
    tempObj.obj.scale = s;

    if(isNoHit(objSphere(r, tempObj))){
        return noHit;
    }

    vec3 rot1 = rotVec3(vec3(1, 0, 0), obj.rot);
    vec3 rot2 = rotVec3(vec3(0, 1, 0), obj.rot);
    vec3 rot3 = rotVec3(vec3(0, 0, 1), obj.rot);

    tempObj.obj.rot = rot1;
    tempObj.obj.scale = vec3(obj.scale.x, 0, 0);
    RayHit plane1 = objPlane(r, tempObj);

    tempObj.obj.rot = rot2;
    tempObj.obj.scale = vec3(obj.scale.y, 0, 0);
    RayHit plane2 = objPlane(r, tempObj);

    tempObj.obj.rot = rot3;
    tempObj.obj.scale = vec3(obj.scale.z, 0, 0);
    RayHit plane3 = objPlane(r, tempObj);

    bool ins1 = isInside(plane1);
    bool ins2 = isInside(plane2);
    bool ins3 = isInside(plane3);

    if(ins1){
        if(ins2){
            if(ins3){
                return intersect(intersect(plane1, plane2), plane3);
            }else{
                vec3 v = plane3.point - obj.pos;
                if(abs(dot(v, rot1)) >= s.x ||
                   abs(dot(v, rot2)) >= s.y){
                    return noHit;
                }return plane3;
            }
        }else{
            if(ins3){
                vec3 v = plane2.point - obj.pos;
                if(abs(dot(v, rot1)) >= s.x ||
                   abs(dot(v, rot3)) >= s.z){
                    return noHit;
                }return plane2;
            }else{
                RayHit plane23 = intersect(plane2, plane3);
                if(abs(dot(plane23.point - obj.pos, rot1)) >= s.x){
                    return noHit;
                }return plane23;
            }
        }
    }else{
        if(ins2){
            if(ins3){
                vec3 v = plane1.point - obj.pos;
                if(abs(dot(v, rot3)) >= s.z ||
                   abs(dot(v, rot2)) >= s.y){
                    return noHit;
                }return plane1;
            }else{
                RayHit plane13 = intersect(plane1, plane3);
                if(abs(dot(plane13.point - obj.pos, rot2)) >= s.y){
                    return noHit;
                }return plane13;
            }
        }else{
            if(ins3){
                RayHit plane12 = intersect(plane1, plane2);
                if(abs(dot(plane12.point - obj.pos, rot3)) >= s.z){
                    return noHit;
                }return plane12;
            }else{
                return intersect(intersect(plane1, plane2), plane3);
            }
        }
    }
}// ----------------------------------------------------------------------------------------------------------------- //
RayHit objHit(Ray r, Object o){
    RayHit ret = noRayHit;
    ret.ray = r;
    ObjData obj = o.obj;

    if(obj.visible){
        switch(obj.type){
            default:
            case UNDEFINED:
                return ret;
            case SPHERE:
                return objSphere(r, o);
            case PLANE:
                return objPlane(r, o);
            case CUBOID:
                return objCuboid(r, o);
        }
    }
    return ret;
}
// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ DATA INITIALIZATION @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ //

$16$
Object sphere_1 = Object(ObjData(true, SPHERE, vec3(0, 1, 6), vec3(0), vec3(1, 0, 0)), m0);
Object sphere_2 = Object(ObjData(true, SPHERE, vec3(0.8, 0.5 + sin(time), 6), vec3(0), vec3(0.75, 0, 0)), m1);
Object cube = Object(ObjData(true, CUBOID, vec3(0, 1, -3), vec3(0, time/10, 0), vec3(40, 50, 0.5*(1 + sin(time)))), m0);
Object ground_plane = Object(ObjData(true, PLANE, vec3(0, 0, 0), normalize(vec3(0, 1, 0)), vec3(SURF, 0, 0)), m2);

$17$
// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ CAST FUNCTIONS @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ //
RayHit rayCast(Ray r){
    RayHit scene = noRayHit;
    scene.ray = r;

    /*scene = unify(scene, objHit(r, sphere_1));
    scene = unify(scene, objHit(r, sphere_2));*/
    scene = unify(scene, objHit(r, cube));
    scene = unify(scene, objHit(r, ground_plane));

    return scene;
}// ----------------------------------------------------------------------------------------------------------------- //
vec3 lightup(RayHit hit, Light light){
    if(!light.on || light.intensity <= 0){
        return vec3(0);
    }
    vec3 _col = light.col * light.intensity, t_col = hit.mat.col;
    bool inh = isNoHit(hit);

    vec3 _pos;
    vec3 _dir;
    float _dist;

    float value = 0;
    float diffuse = 0;
    float specular = 0;
    float halo = 0;

    switch(light.type){
        case AMBIENT:
            return _col;
        case POINT:
            _pos = light.pos;
            _dir = _pos - hit.point;
            _dist = length(_dir);
            _dir /= _dist;

            if(hit.mat.halo && light.doesGlow){
                vec3 toOrigDir = _pos - hit.ray.origin;
                float toOrigDist = length(toOrigDir);
                toOrigDir /= toOrigDist;

                if(rayCast(Ray(hit.ray.origin, toOrigDir)).dist > toOrigDist){
                    halo = pow(clamp(dot(toOrigDir, hit.ray.direction), 0, 1), MAX_SHINE >> int(HALO_FACTOR * light.glow)) / toOrigDist;
                }
            }
            break;
        case DIRECTIONAL:
            _dir = light.dir;
            _dist = DIR_LIGHT_DIST;
            _pos = along(Ray(hit.point, _dir), _dist);

            if(light.doesGlow && inh){
                halo = pow(clamp(dot(_dir, hit.ray.direction), 0, 1), MAX_SHINE >> int(HALO_FACTOR * light.glow));
            }
            break;
        case CONE:
            _pos = light.pos;
            _dir = _pos - hit.point;
            _dist = length(_dir);
            _dir /= _dist;

            _col = clamp(_col*dot(_dir, light.dir), 0, 1);
            break;
        default:
            return vec3(0);
    }

    if(inh){
        return clamp(_col * halo, 0, 1);
    }
    if(hit.mat.opaque){
        RayHit shadowRay;
        switch(hit.mat.shadowMode){
            default:
            case SHADOW_OFF:
                value = 1;
                break;
            case SHADOW_SIMPLE:
                shadowRay = rayCast(Ray(hit.point + SURF * hit.normal, _dir));
                value = shadowRay.dist / _dist;
                if(value < 1){
                    value *= 1 - value;
                }else{
                    value = 1;
                }
                break;
            case SHADOW_DEPTH:
                float depth = 0;
                float limitDist = _dist - SURF;
                shadowRay = rayCast(Ray(hit.point + SURF * hit.normal, _dir));

                for(int i = 0; i < MAX_SHADOW_DEPTH; i++){
                    if(i != 0){
                        shadowRay = rayCast(Ray(shadowRay.point2 + SURF*_dir, _dir));
                    }
                    if(shadowRay.dist >= limitDist){
                        break;
                    }
                    depth += (min(limitDist, shadowRay.dist2) - shadowRay.dist)*shadowRay.mat.density;
                    limitDist -= shadowRay.dist2 - SURF;
                    if(limitDist <= 0){
                        break;
                    }
                }
                value = exp(-SHADOW_SCALE * depth);
                break;
        }
    }

    diffuse = clamp(value*abs(dot(hit.normal, _dir)), 0, 1);

    if(hit.mat.reflective){
        specular = pow(clamp(value * dot(hit.normal, normalize(_dir - hit.ray.direction)), 0, 1), int(MAX_SHINE * hit.mat.reflectivity) >> int(HALO_FACTOR * light.glow));
        t_col *= diffuse + hit.mat.reflectivity*(specular - diffuse);
    }else{
        t_col *= diffuse;
    }

    return clamp(_col * (t_col + halo), 0, 1);
}// ----------------------------------------------------------------------------------------------------------------- //
vec3 lightCast(RayHit hit){
    if(LIGHTMODE_DISTANCE){
        return vec3(sqrt(hit.dist / FAR));
    }

    vec3 _col = vec3(0), t_col = vec3(0);

$18$
    _col += t_col;

    if(isInside(hit)){
        return _col;
    }
    float t_refl = 1.0;

    for(int i = 0; i <= MAX_REFLECTIONS; i++){
        if(i != 0){
            hit = rayCast(reflect(hit.ray, hit.normal, hit.point));
        }
        t_col = vec3(0);

$19$
        _col += t_refl * t_col;
        if(isNoHit(hit)){
            break;
        }
        t_refl *= hit.mat.reflectivity;
        if(t_refl <= MIN_REFLECTIVITY){
            break;
        }
    }

    return clamp(_col, 0, 1);
}
// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ MAIN @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ //
void main(){
    vec2 uv = gl_FragCoord.xy - 0.5*scrRes.xy;

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
        ro = camPos + SCR_SIZE*camRot*vec3(uv, 0);
        rd = camRot*normalize(vec3(uv, NEAR));
    }

    frag_col = lightCast(rayCast(Ray(ro, rd)));
}
// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@  @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ //
// ----------------------------------------------------------------------------------------------------------------- //