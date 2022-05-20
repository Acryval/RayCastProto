package acv.rcproto.objData.prototypes;

import static acv.rcproto.shader.RCShader.*;

import acv.rcproto.objData.Light;
import acv.rcproto.objData.states.LightState;
import org.joml.Vector3f;

public class LightPrototype {
    public static final String LIGHT_PREFIX = "light_";

    public String name;
    public LightState init_state;

    public LightPrototype(int index){
        this.name = LIGHT_PREFIX + index;
        init_state = new LightState();
    }

    public void set(boolean on, boolean doesGlow, int type, float intensity, float glow, Vector3f pos, Vector3f dir, Vector3f col){
        init_state.set(on, doesGlow, type, intensity, glow, pos, dir, col);
    }

    public void setAmbient(boolean on, float intensity, Vector3f col){
        init_state.setOn(on).setType(LIGHT_AMBIENT).setIntensity(intensity).setCol(col);
    }

    public void setPoint(boolean on, boolean doesGlow, float intensity, float glow, Vector3f pos, Vector3f col){
        init_state.setOn(on).setDoesGlow(doesGlow).setType(LIGHT_POINT).setIntensity(intensity).setGlow(glow).setPos(pos).setCol(col);
    }

    public void setDirectional(boolean on, boolean doesGlow, float intensity, float glow, Vector3f dir, Vector3f col){
        init_state.setOn(on).setDoesGlow(doesGlow).setType(LIGHT_DIRECTIONAL).setIntensity(intensity).setGlow(glow).setDir(dir).setCol(col);
    }

    public void setCone(boolean on, boolean doesGlow, float intensity, float glow, Vector3f pos, Vector3f dir, Vector3f col){
        init_state.set(on, doesGlow, LIGHT_CONE, intensity, glow, pos, dir, col);
    }

    public String[] getStrings(){
        String[] s = new String[]{"", ""};

        s[0] = "uniform Light " + name + ";";
        s[1] = "t_col += lightup(hit, " + name + ");";

        return s;
    }

    public Light export(int programID){
        return new Light(programID, name, init_state);
    }

    public boolean isAmbient(){
        return init_state.type == LIGHT_AMBIENT;
    }
}
