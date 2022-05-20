package acv.rcproto.objData.states;

import static acv.rcproto.shader.RCShader.*;

import org.joml.Vector3f;

public class LightState {
    public boolean  on,
                    doesGlow;
    public int      type;
    public float    intensity,
                    glow;
    public Vector3f pos,
                    dir,
                    col;

    public boolean  nu_on,
                    nu_doesGlow,
                    nu_type,
                    nu_intensity,
                    nu_glow,
                    nu_pos,
                    nu_dir,
                    nu_col;

    public static final int NUM_UNIFORMS = 8;

    public LightState(){
        this(false, false, LIGHT_AMBIENT, 0, 0, new Vector3f(0), new Vector3f(0, 1, 0), new Vector3f(0));
    }

    public LightState(LightState s){
        set(s.on, s.doesGlow, s.type, s.intensity, s.glow, s.pos, s.dir, s.col);
    }

    public LightState(boolean on, boolean doesGlow, int type, float intensity, float glow, Vector3f pos, Vector3f dir, Vector3f col){
        set(on, doesGlow, type, intensity, glow, pos, dir, col);
    }

    public LightState set(LightState s){
        return setOn(s.on).setDoesGlow(s.doesGlow).setType(s.type).setIntensity(s.intensity).setGlow(s.glow).setPos(s.pos).setDir(s.dir).setCol(s.col);
    }

    public LightState set(boolean on, boolean doesGlow, int type, float intensity, float glow, Vector3f pos, Vector3f dir, Vector3f col){
        return setOn(on).setDoesGlow(doesGlow).setType(type).setIntensity(intensity).setGlow(glow).setPos(pos).setDir(dir).setCol(col);
    }

    public LightState setOn(boolean on) {
        this.on = on;
        this.nu_on = true;
        return this;
    }

    public LightState setDoesGlow(boolean doesGlow) {
        this.doesGlow = doesGlow;
        this.nu_doesGlow = true;
        return this;
    }

    public LightState setType(int type) {
        this.type = type;
        this.nu_type = true;
        return this;
    }

    public LightState setIntensity(float intensity) {
        this.intensity = intensity;
        this.nu_intensity = true;
        return this;
    }

    public LightState setGlow(float glow) {
        this.glow = glow;
        this.nu_glow = true;
        return this;
    }

    public LightState setPos(Vector3f pos) {
        this.pos = pos;
        this.nu_pos = true;
        return this;
    }

    public LightState setDir(Vector3f dir){
        this.dir = dir.normalize();
        this.nu_dir = true;
        return this;
    }

    public LightState setCol(Vector3f col) {
        this.col = col;
        this.nu_col = true;
        return this;
    }

    public LightState toggle(){
        return setOn(!on);
    }
}
