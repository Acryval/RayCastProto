package acv.rcproto.objData.prototypes;

import acv.rcproto.objData.Material;
import acv.rcproto.objData.states.MaterialState;
import org.joml.Vector3f;

public class MaterialPrototype {
    public static final String MATERIAL_PREFIX = "m";

    public String name;
    public MaterialState init_state;

    public MaterialPrototype(int index){
        this.name = MATERIAL_PREFIX + index;
        init_state = new MaterialState();
    }

    public void set(boolean reflective, boolean opaque, boolean halo, int shadowMode, float reflectivity, float density, Vector3f color){
        init_state.set(reflective, opaque, halo, shadowMode, reflectivity, density, color);
    }

    public String[] getStrings(){
        return new String[]{"uniform Material " + name + ";"};
    }

    public Material export(int programID){
        return new Material(programID, name, init_state);
    }
}
