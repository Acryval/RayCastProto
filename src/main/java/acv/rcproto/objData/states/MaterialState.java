package acv.rcproto.objData.states;

import static acv.rcproto.shader.RCShader.*;

import org.joml.Vector3f;

public class MaterialState {
    public boolean  reflective,
                    opaque,
                    halo;
    public int      shadowMode;
    public float    reflectivity,
                    density;
    public Vector3f color;

    public boolean  nu_reflective,
                    nu_opaque,
                    nu_halo,
                    nu_shadowMode,
                    nu_reflectivity,
                    nu_density,
                    nu_color;

    public static final int NUM_UNIFORMS = 7;

    public MaterialState(){
        this(false, false, false, SHADOW_OFF, 0, 1, new Vector3f(0.9961f, 0.0039f, 0.6941f));
    }

    public MaterialState(MaterialState s){
        set(s.reflective, s.opaque, s.halo, s.shadowMode, s.reflectivity, s.density, s.color);
    }

    public MaterialState(boolean reflective, boolean opaque, boolean halo, int shadowMode, float reflectivity, float density, Vector3f color){
        set(reflective, opaque, halo, shadowMode, reflectivity, density, color);
    }

    public MaterialState set(MaterialState s){
        return setReflective(s.reflective).setOpaque(s.opaque).setHalo(s.halo).setShadowMode(s.shadowMode).setReflectivity(s.reflectivity).setDensity(s.density).setColor(s.color);
    }

    public MaterialState set(boolean reflective, boolean opaque, boolean halo, int shadowMode, float reflectivity, float density, Vector3f color){
        return setReflective(reflective).setOpaque(opaque).setHalo(halo).setShadowMode(shadowMode).setReflectivity(reflectivity).setDensity(density).setColor(color);
    }

    public MaterialState setReflective(boolean reflective) {
        this.reflective = reflective;
        nu_reflective = true;
        return this;
    }

    public MaterialState setOpaque(boolean opaque) {
        this.opaque = opaque;
        nu_opaque = true;
        return this;
    }

    public MaterialState setHalo(boolean halo) {
        this.halo = halo;
        nu_halo = true;
        return this;
    }

    public MaterialState setShadowMode(int shadowMode) {
        this.shadowMode = shadowMode;
        nu_shadowMode = true;
        return this;
    }

    public MaterialState setReflectivity(float reflectivity) {
        this.reflectivity = reflectivity;
        nu_reflectivity = true;
        return this;
    }

    public MaterialState setDensity(float density) {
        this.density = density;
        nu_density = true;
        return this;
    }

    public MaterialState setColor(Vector3f color) {
        this.color = color;
        nu_color = true;
        return this;
    }
}
