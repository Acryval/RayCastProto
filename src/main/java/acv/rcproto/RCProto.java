package acv.rcproto;

import static org.lwjgl.glfw.GLFW.*;
import static org.lwjgl.opengl.GL40.*;
import static acv.rcproto.shader.RCShader.*;

import org.joml.Vector3f;
import org.lwjgl.glfw.GLFWErrorCallback;

import acv.rcproto.display.Window;
import acv.rcproto.shader.RCShader;
import acv.rcproto.display.Camera;

public class RCProto {

	private Window window;
	private RCShader shader;
	private Camera cam;
	private Vector3f move;
	private long stime, ctime, ltime, ptime, dtime, fps, fps_target;
	private float ftime;
	private boolean paused, postCompile;
	private int shaderMode, numShaderModes;

	public void run() {
		GLFWErrorCallback.createPrint(System.err).set();
		
		if(!glfwInit()) {
			throw new IllegalStateException("GLFW couldn't be initialized!");
		}

		fps = 60;
		fps_target = 1000 / fps;
		
		int width = 1200,
			height = 700;
		
		boolean fullscr = true,
				vsync = true,
				centered = true,
				resizeable = false;

		paused = false;
		postCompile = false;
		shaderMode = 0;
		numShaderModes = 3;

		String title = "RayCast Prototype";
		
		window = new Window(width, height, title, fullscr, vsync, centered, resizeable);
		width = window.getWidth();
		height = window.getHeight();
		move = new Vector3f();

		glfwSetKeyCallback(window.getWindow(), (long wnd, int key, int scancode, int action, int mods) -> {
			if(key == GLFW_KEY_ESCAPE && action == GLFW_RELEASE) {
				window.close();
			}

			if(action == GLFW_PRESS){
				switch (key) {
					case GLFW_KEY_H -> {
						shaderMode = ++shaderMode % numShaderModes;
						postCompile = !paused;
						paused = true;
					}
					case GLFW_KEY_P -> togglePause();
					case GLFW_KEY_W -> move.add(0, 0, 1);
					case GLFW_KEY_S -> move.add(0, 0, -1);
					case GLFW_KEY_A -> move.add(-1, 0, 0);
					case GLFW_KEY_D -> move.add(1, 0, 0);
					case GLFW_KEY_LEFT_SHIFT -> move.add(0, -1, 0);
					case GLFW_KEY_SPACE -> move.add(0, 1, 0);
					case GLFW_KEY_LEFT_CONTROL -> cam.startFast();
					default -> {
					}
				}
			}else if(action == GLFW_RELEASE){
				switch (key) {
					case GLFW_KEY_B -> shader.getLight(4).state.toggle();
					case GLFW_KEY_H -> {
						shader.decompile();
						switch (shaderMode) {
							case 1 -> {
								shader.setVal_pix(true);
								shader.setVal_pixSize(5);
							}
							case 2 -> shader.setVal_shaderMode(1);
							default -> {
							}
						}
						shader.compile();
						shader.stop();
					}
					case GLFW_KEY_W -> move.sub(0, 0, 1);
					case GLFW_KEY_S -> move.sub(0, 0, -1);
					case GLFW_KEY_A -> move.sub(-1, 0, 0);
					case GLFW_KEY_D -> move.sub(1, 0, 0);
					case GLFW_KEY_LEFT_SHIFT -> move.sub(0, -1, 0);
					case GLFW_KEY_SPACE -> move.sub(0, 1, 0);
					case GLFW_KEY_LEFT_CONTROL -> cam.stopFast();
					default -> {
					}
				}
			}
		});

		cam = new Camera(new Vector3f(0, 1, 0), new Vector3f(0, 0, 0), .1f, 3f, 2.5f);

		shader = new RCShader(width, height, false);

		shader.addMaterial().set(true, true, true, SHADOW_DEPTH, 0.75f, 1, new Vector3f(1, 0, 0));
		shader.addMaterial().set(true, true, true, SHADOW_DEPTH, 0, 0.5f, new Vector3f(0, 0, 1));
		shader.addMaterial().set(true, true, true, SHADOW_DEPTH, 0.5f, 1, new Vector3f(0.4824f, 0.3020f, 0.2078f));

		shader.addLight().setAmbient(true, 1, new Vector3f(0, 0.03f, 0.07f));
		shader.addLight().setPoint(true, true, 1, 0, new Vector3f(0, 0.5f, 0), new Vector3f(1));
		shader.addLight().setPoint(true, true, 2, 0.2f, new Vector3f(0, 2, 0), new Vector3f(1));
		shader.addLight().setPoint(true, true, 4, 0, new Vector3f(0, 20, -6), new Vector3f(1));
		shader.addLight().setDirectional(true, true, 0.5f, 0.3f, new Vector3f(1), new Vector3f(1));

		System.out.println("compile");
		shader.compile();
		System.out.println("compile done");
		shader.stop();
		
		glClearColor(0f, 0f, 0f, 0f);
		glViewport(0, 0, window.getWidth(), window.getHeight());

		glMatrixMode(GL_PROJECTION);
		glLoadIdentity();

		glMatrixMode(GL_MODELVIEW);
		glLoadIdentity();
		
		stime = System.currentTimeMillis();
		ctime = stime;
		ltime = stime;
		ptime = 0;
		dtime = 0;
		ftime = 0;
		
		while(!window.shouldClose()) {
			ctime = System.currentTimeMillis();
			dtime = ctime - ltime;

			cam.rotate(window.getDmouse());

			if(dtime >= fps_target) {
				ltime = ctime;
				if(paused){
					ptime += dtime;
					if(postCompile){
						togglePause();
						postCompile = false;
					}
				}
				ftime = (ctime - ptime - stime) / 1000.f;
				glClear(GL_COLOR_BUFFER_BIT);

				cam.move(move, dtime / 20.f);

				shader.start();

				shader.setUniform_time(ftime);
				shader.update(cam);

				shader.drawScene();

				shader.stop();
				window.update();
			}

			glfwPollEvents();
		}
		
		shader.dispose();
		window.dispose();

		glfwTerminate();
		glfwSetErrorCallback(null).free();
	}

	public void togglePause(){
		paused ^= true;
	}

	public static void main(String[] args) {
		new RCProto().run();
	}
}