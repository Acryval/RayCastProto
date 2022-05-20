package acv.rcproto.objData;

import acv.rcproto.objData.states.LightState;
import org.joml.Vector3f;

import static org.lwjgl.opengl.GL40.*;

public class Light {
    private int loc_on;
    private int loc_doesGlow;
    private int loc_type;
    private int loc_intensity;
    private int loc_glow;
    private int loc_pos;
    private int loc_dir;
    private int loc_col;

    public LightState state;

    public Light(int programID, String name){
        this(programID, name, new LightState());
    }

    public Light(int programID, String name, LightState init_state){
        loc_on = glGetUniformLocation(programID, name + ".on");
        loc_doesGlow = glGetUniformLocation(programID, name + ".doesGlow");
        loc_type = glGetUniformLocation(programID, name + ".type");
        loc_intensity = glGetUniformLocation(programID, name + ".intensity");
        loc_glow = glGetUniformLocation(programID, name + ".glow");
        loc_pos = glGetUniformLocation(programID, name + ".pos");
        loc_dir = glGetUniformLocation(programID, name + ".dir");
        loc_col = glGetUniformLocation(programID, name + ".col");

        state = new LightState(init_state);

        update();
    }

    public Light set(boolean on, boolean doesGlow, int type, float intensity, float glow, Vector3f pos, Vector3f dir, Vector3f col){
        state.set(on, doesGlow, type, intensity, glow, pos, dir, col);
        return this;
    }

    public void update(){
        if(state.nu_on){
            glUniform1i(loc_on, state.on ? 1 : 0);
            state.nu_on = false;
        }
        if(state.nu_doesGlow){
            glUniform1i(loc_doesGlow, state.doesGlow ? 1 : 0);
            state.nu_doesGlow = false;
        }

        if(state.nu_type){
            glUniform1i(loc_type, state.type);
            state.nu_type = false;
        }

        if(state.nu_intensity){
            glUniform1f(loc_intensity, state.intensity);
            state.nu_intensity = false;
        }
        if(state.nu_glow){
            glUniform1f(loc_glow, state.glow);
            state.nu_glow = false;
        }

        if(state.nu_pos){
            glUniform3f(loc_pos, state.pos.x, state.pos.y, state.pos.z);
            state.nu_pos = false;
        }
        if(state.nu_dir){
            glUniform3f(loc_dir, state.dir.x, state.dir.y, state.dir.z);
            state.nu_dir = false;
        }
        if(state.nu_col){
            glUniform3f(loc_col, state.col.x, state.col.y, state.col.z);
            state.nu_col = false;
        }
    }
}
