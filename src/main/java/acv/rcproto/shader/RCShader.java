package acv.rcproto.shader;

import acv.rcproto.display.Camera;
import acv.rcproto.objData.Light;
import acv.rcproto.objData.Material;
import acv.rcproto.objData.prototypes.LightPrototype;
import acv.rcproto.objData.prototypes.MaterialPrototype;
import acv.rcproto.objData.states.LightState;
import acv.rcproto.objData.states.MaterialState;
import org.joml.Vector3f;

import java.util.ArrayList;

import static org.lwjgl.opengl.GL40.*;

public class RCShader extends AbstractShader{
    public static final String VERTEX = "doNothing",
                               FRAGMENT = "rcs";

    public static final int SHADOW_OFF = 0,
                            SHADOW_SIMPLE = 1,
                            SHADOW_DEPTH = 2;

    public static final int SHADER_RAYCAST = 0,
                            SHADER_DEPTH = 1;

    public static final int GEOMETRY_DUMMY = 0,
                            GEOMETRY_SPHERE = 1,
                            GEOMETRY_PLANE = 2,
                            GEOMETRY_CUBOID = 3;

    public static final int LIGHT_AMBIENT = 0,
                            LIGHT_POINT = 1,
                            LIGHT_DIRECTIONAL = 2,
                            LIGHT_CONE = 3;

    private int loc_time,
                loc_scrRes,
                loc_camPos,
                loc_camRot;

    private boolean val_pix,
                    val_ortho;
    private int     val_pixSize,
                    val_haloFactor,
                    val_maxShine,
                    val_maxReflections,
                    val_maxShadowDepth,
                    val_shaderMode;
    private float   val_near,
                    val_fov,
                    val_far,
                    val_surf,
                    val_minReflectivity,
                    val_shadowScale,
                    val_dirLightDist,
                    val_scrSize;

    private int num_materials;
    private ArrayList<Material> materials;
    private ArrayList<MaterialPrototype> materials_protos;

    private int num_lights;
    private ArrayList<Light> lights;
    private ArrayList<LightPrototype> lights_protos;

    private int scr_width,
                scr_height;

    public RCShader(int width, int height, boolean srcDebug) {
        super(VERTEX, FRAGMENT);
        if(srcDebug) srcDebug();

        getDefaultValues();

        num_materials = 0;
        materials = new ArrayList<Material>();
        materials_protos = new ArrayList<MaterialPrototype>();

        num_lights = 0;
        lights = new ArrayList<Light>();
        lights_protos = new ArrayList<LightPrototype>();

        scr_width = width;
        scr_height = height;
    }

    private void getDefaultValues(){
        val_pix = false;
        val_ortho = false;

        val_pixSize = 16;
        val_haloFactor = 15;
        val_maxShine = 32767;
        val_maxReflections = 3;
        val_maxShadowDepth = 3;
        val_shaderMode = SHADER_RAYCAST;

        val_near = 1.0f;
        val_fov = 90.0f;
        val_far = 1000.0f;
        val_surf = 0.001f;
        val_minReflectivity = 0.1f;
        val_shadowScale = 10.0f;
        val_dirLightDist = 100.0f;
        val_scrSize = 0.2f;
    }

    @Override
    public void compile() {
        super.compile();
        setUniform_scrRes(scr_width, scr_height);








































    }

    @Override
    protected String[] prepareShaderData() {
        String[] s = new String[15];

        s[0] = Boolean.toString(val_pix);
        s[1] = Boolean.toString(val_ortho);

        s[2] = Integer.toString(val_pixSize);
        s[3] = Integer.toString(val_haloFactor);
        s[4] = Integer.toString(val_maxShine);
        s[5] = Integer.toString(val_maxReflections);
        s[6] = Integer.toString(val_maxShadowDepth);
        s[7] = val_shaderMode == 0 ? "false" : "true";

        s[8] = Float.toString(val_ortho ? val_near : 1.0f / (float)Math.tan(0.00872665*val_fov));
        s[9] = Float.toString(val_far);
        s[10] = Float.toString(val_surf);
        s[11] = Float.toString(val_minReflectivity);
        s[12] = Float.toString(val_shadowScale);
        s[13] = Float.toString(val_dirLightDist);
        s[14] = Float.toString(val_scrSize);

        return fuseStrArrays(s, prepareMaterials(), prepareLights(), prepareObjects());
    }

    private String[] prepareMaterials(){
        String[] s = new String[]{""}, temp;

        for(MaterialPrototype p : materials_protos){
            temp = p.getStrings();

            s[0] += temp[0] + "\n";
        }

        return s;
    }

    private String[] prepareLights(){
        String[] s = new String[]{"", "", ""}, temp;

        for(LightPrototype p : lights_protos){
            temp = p.getStrings();

            s[0] += temp[0] + "\n";
            if(p.isAmbient()){
                s[1] += "\t" + temp[1] + "\n";
            }else{
                s[2] += "\t\t" + temp[1] + "\n";
            }
        }

        return s;
    }

    private String[] prepareObjects(){
        return new String[0];
    }

    @Override
    protected void bindAttributes() {}

    @Override
    protected void getUniformLocations() {
        loc_time = getUniformLocation("time");
        loc_scrRes = getUniformLocation("scrRes");
        loc_camPos = getUniformLocation("camPos");
        loc_camRot = getUniformLocation("camRot");

        loadMaterials();
        loadLights();
        loadObjects();
    }

    public void loadMaterials(){
        for(int i = 0; i < num_materials; i++){
            materials.add(materials_protos.get(i).export(pID));
        }
    }

    public void loadLights(){
        for(int i = 0; i < num_lights; i++){
            lights.add(lights_protos.get(i).export(pID));
        }
    }

    public void loadObjects(){

    }

    public void update(Camera cam){
        Vector3f cp = cam.getPos();
        float[] cr = new float[9];
        cam.getRotMat().get(cr);

        glUniform3f(loc_camPos, cp.x, cp.y, cp.z);
        glUniformMatrix3fv(loc_camRot, false, cr);

        for(Material m : materials){
            m.update();
        }

        for(Light l : lights) {
            l.update();
        }
    }

    public int getNextMaterialIndex(){
        return num_materials++;
    }
    public int getNumMaterials(){
        return num_materials;
    }
    public MaterialPrototype addMaterial(){
        MaterialPrototype mat_proto = new MaterialPrototype(getNextMaterialIndex());
        materials_protos.add(mat_proto);
        return mat_proto;
    }
    public Material getMaterial(int index){
        return materials.get(index);
    }

    public int getNextLightIndex(){
        return num_lights++;
    }
    public int getNumLights(){
        return num_lights;
    }
    public LightPrototype addLight(){
        LightPrototype light_proto = new LightPrototype(getNextLightIndex());
        lights_protos.add(light_proto);
        return light_proto;
    }
    public Light getLight(int index){
        return lights.get(index);
    }

    private int getNumUniforms(){
        return 4 + num_materials * MaterialState.NUM_UNIFORMS + num_lights * LightState.NUM_UNIFORMS;
    }

    public void decompile(){
        super.dispose();
        materials.clear();
        lights.clear();
        getDefaultValues();
    }

    public void clearProtos(){
        materials_protos.clear();
        num_materials = 0;

        lights_protos.clear();
        num_lights = 0;
    }

    @Override
    public void dispose() {
        decompile();
        clearProtos();
    }

    public void setUniform_time(float time) {
        glUniform1f(loc_time, time);
    }

    public void setUniform_scrRes(int width, int height) {
        glUniform2f(loc_scrRes, width, height);
    }

    public void setVal_near(float val_near) {
        this.val_near = val_near;
    }

    public void setVal_far(float val_far) {
        this.val_far = val_far;
    }

    public void setVal_surf(float val_surf) {
        this.val_surf = val_surf;
    }

    public void setVal_minReflectivity(float val_minReflectivity) {
        this.val_minReflectivity = val_minReflectivity;
    }

    public void setVal_shadowScale(float val_shadowScale) {
        this.val_shadowScale = val_shadowScale;
    }

    public void setVal_dirLightDist(float val_dirLightDist) {
        this.val_dirLightDist = val_dirLightDist;
    }

    public void setVal_pixSize(int val_pixSize) {
        this.val_pixSize = val_pixSize;
    }

    public void setVal_haloFactor(int val_haloFactor) {
        this.val_haloFactor = val_haloFactor;
    }

    public void setVal_maxShine(int val_maxShine) {
        this.val_maxShine = val_maxShine;
    }

    public void setVal_maxReflections(int val_maxReflections) {
        this.val_maxReflections = val_maxReflections;
    }

    public void setVal_maxShadowDepth(int val_maxShadowDepth) {
        this.val_maxShadowDepth = val_maxShadowDepth;
    }

    public void setVal_fov(float val_fov) {
        this.val_fov = val_fov;
    }

    public void setVal_shaderMode(int val_shaderMode) {
        this.val_shaderMode = val_shaderMode;
    }

    public void setVal_pix(boolean val_pix) {
        this.val_pix = val_pix;
    }

    public void setVal_ortho(boolean val_ortho) {
        this.val_ortho = val_ortho;
    }
}
