package acv.rcproto.objData;

import acv.rcproto.objData.states.MaterialState;
import org.joml.Vector3f;

import static org.lwjgl.opengl.GL40.*;

public class Material {
    private int     loc_reflective,
                    loc_opaque,
                    loc_halo,
                    loc_shadowMode,
                    loc_reflectivity,
                    loc_density,
                    loc_color;

    public MaterialState state;

    public Material(int programID, String name){
        this(programID, name, new MaterialState());
    }

    public Material(int programID, String name, MaterialState init_state){
        loc_reflective = glGetUniformLocation(programID, name + ".reflective");
        loc_opaque = glGetUniformLocation(programID, name + ".opaque");
        loc_halo = glGetUniformLocation(programID, name + ".halo");
        loc_shadowMode = glGetUniformLocation(programID, name + ".shadowMode");
        loc_reflectivity = glGetUniformLocation(programID, name + ".reflectivity");
        loc_density = glGetUniformLocation(programID, name + ".density");
        loc_color = glGetUniformLocation(programID, name + ".col");

        state = new MaterialState(init_state);

        update();
    }

    public Material set(boolean reflective, boolean opaque, boolean halo, int shadowMode, float reflectivity, float density, Vector3f color){
        state.set(reflective, opaque, halo, shadowMode, reflectivity, density, color);
        return this;
    }

    public void update(){
        if(state.nu_reflective){
            glUniform1i(loc_reflective, state.reflective ? 1 : 0);
            state.nu_reflective = false;
        }
        if(state.nu_opaque){
            glUniform1i(loc_opaque, state.opaque ? 1 : 0);
            state.nu_opaque = false;
        }
        if(state.nu_halo){
            glUniform1i(loc_halo, state.halo ? 1 : 0);
            state.nu_halo = false;
        }

        if(state.nu_shadowMode){
            glUniform1i(loc_shadowMode, state.shadowMode);
            state.nu_shadowMode = false;
        }

        if(state.nu_reflectivity){
            glUniform1f(loc_reflectivity, state.reflectivity);
            state.nu_reflectivity = false;
        }
        if(state.nu_density){
            glUniform1f(loc_density, state.density);
            state.nu_density = false;
        }

        if(state.nu_color){
            glUniform3f(loc_color, state.color.x, state.color.y, state.color.z);
            state.nu_color = false;
        }
    }
}
