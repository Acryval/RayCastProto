package acv.rcproto.display;

import org.joml.Math;
import org.joml.Matrix3f;
import org.joml.Vector3f;

public class Camera {
    private Vector3f pos, rot;
    private float vel, fvelm, rots;
    private boolean fast, stopFast;

    private float PI_2 = 1.5707963f;

    public Camera(Vector3f startPos, Vector3f startRot, float velocity, float fastVelMultiplier, float rotationSensitivity){
        pos = startPos;
        rot = startRot;
        vel = velocity;
        fvelm = fastVelMultiplier;
        rots = rotationSensitivity;
        fast = false;
        stopFast = false;
    }

    public void startFast(){
        fast = true;
    }

    public void stopFast(){
        stopFast = true;
    }

    public Vector3f right(){
        Vector3f ret = new Vector3f(0);
        new Matrix3f().identity().rotateY(rot.y).getColumn(0, ret);
        //getRotMat().getColumn(0, ret);
        return ret;
    }

    public Vector3f up(){
        Vector3f ret = new Vector3f(0, 1, 0);
        //new Matrix3f().identity().rotateY(rot.y).getColumn(1, ret);
        //getRotMat().getColumn(1, ret);
        return ret;
    }

    public Vector3f front(){
        Vector3f ret = new Vector3f(0);
        new Matrix3f().identity().rotateY(rot.y).getColumn(2, ret);
        //getRotMat().getColumn(2, ret);
        return ret;
    }

    public void move(Vector3f dir, float dtime){
        if(dtime != 0){
            if(dir.lengthSquared() == 0){
                if(stopFast){
                    fast = false;
                    stopFast = false;
                }
                return;
            }

            float v = vel;

            if(fast) {
                v *= fvelm;
            }

            Vector3f dpos = new Vector3f();

            dpos.add(right().mul(dir.x));
            dpos.add(up().mul(dir.y));
            dpos.add(front().mul(dir.z));

            pos.add(dpos.mul(v*dtime));
        }
    }

    public void rotate(float[] dr){
        rotate(new Vector3f(dr[1], dr[0], 0));
    }

    public void rotate(Vector3f drot){
        rot.add(drot.mul(rots));
        rot.x = Math.clamp(-PI_2, PI_2, rot.x);
    }

    public Vector3f getPos(){
        return pos;
    }

    public Matrix3f getRotMat(){
        return new Matrix3f().identity().rotateZYX(rot);
    }

    public void setPos(Vector3f pos){
        this.pos = pos;
    }

    public void setRot(Vector3f rot){
        this.rot = rot;
    }
}
