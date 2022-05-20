package acv.rcproto.display;

import static org.lwjgl.glfw.Callbacks.glfwFreeCallbacks;
import static org.lwjgl.glfw.GLFW.*;

import org.lwjgl.glfw.GLFW;
import org.lwjgl.glfw.GLFWKeyCallbackI;
import org.lwjgl.glfw.GLFWVidMode;
import org.lwjgl.opengl.GL;

public class Window {
	
	private long window;
	private int width, height;
	private String title;
	private float[] mouse, dmouse;
	
	public Window(int width, int height, String title, boolean fullscr, boolean vsync, boolean centered, boolean res) {
		this.width = width;
		this.height = height;
		this.title = title;
		this.mouse = new float[2];
		this.dmouse = new float[2];
		
		initWindow(fullscr, vsync, centered, res);
	}
	
	private void initWindow(boolean fullscr, boolean vsync, boolean centered, boolean resizeable) {
		long monitor = 0;

		System.out.println("init");
		glfwDefaultWindowHints();
		glfwWindowHint(GLFW_VISIBLE, GLFW_FALSE);
		glfwWindowHint(GLFW_RESIZABLE, resizeable ? GLFW_TRUE : GLFW_FALSE);
		System.out.println("hints");
		
		if(fullscr) {
			monitor = GLFW.glfwGetPrimaryMonitor();
			GLFWVidMode mode = GLFW.glfwGetVideoMode(monitor);
			width = mode.width();
			height = mode.height();
			System.out.println("w: " + width + ", h:" + height);
		}
		System.out.println("fulscr check");
		
		window = glfwCreateWindow(width, height, title, monitor, 0);
		System.out.println("create window");
		
		if(window == 0) {
			throw new RuntimeException("GLFW window cannot be initialized");
		}
		System.out.println("success");
		
		if(centered && !fullscr) {
			GLFWVidMode mode = GLFW.glfwGetVideoMode(GLFW.glfwGetPrimaryMonitor());
			glfwSetWindowPos(window, (mode.width() - width)/2, (mode.height() - height)/2);
		}
		System.out.println("centered");

		glfwSetInputMode(window, GLFW_CURSOR, GLFW_CURSOR_DISABLED);
		if(glfwRawMouseMotionSupported()){
			glfwSetInputMode(window, GLFW_RAW_MOUSE_MOTION, GLFW_TRUE);
		}
		System.out.println("input mode");
		
		setDefaultKeyCallback();
		
		glfwMakeContextCurrent(window);
		glfwSwapInterval(vsync ? 1 : 0);
		glfwShowWindow(window);
		GL.createCapabilities();
		System.out.println("gl capabilities");
	}
	
	public boolean shouldClose() {
		return glfwWindowShouldClose(window);
	}
	
	public void close() {
		glfwSetWindowShouldClose(window, true);
	}
	
	public void update() {
		glfwSwapBuffers(window);
	}
	
	public void setKeyCallback(GLFWKeyCallbackI cbfun) {
		glfwSetKeyCallback(window, cbfun);
	}
	
	public void setDefaultKeyCallback() {
		setKeyCallback((long wnd, int key, int scancode, int action, int mods) -> {
			if(key == GLFW_KEY_ESCAPE && action == GLFW_RELEASE) {
				close();
			}
		});

		glfwSetCursorPosCallback(window, (long wnd, double x, double y) -> {
			dmouse[0] = (float)x - mouse[0];
			dmouse[1] = (float)y - mouse[1];

			mouse[0] = (float)x;
			mouse[1] = (float)y;
		});

		double[] x = new double[1], y = new double[1];
		glfwGetCursorPos(window, x, y);
		mouse[0] = (float)x[0];
		mouse[1] = (float)y[0];
	}

	public float[] getMousePos(){
		return mouse;
	}

	public float[] getDmouse(){
		float[] ret = new float[2];

		ret[0] = dmouse[0] / width;
		ret[1] = dmouse[1] / height;
		dmouse[0] = 0;
		dmouse[1] = 0;

		return ret;
	}
	
	public long getWindow() {
		return window;
	}

	public int getWidth() {
		return width;
	}

	public int getHeight() {
		return height;
	}

	public float getRatio(){
		return (float)width / (float)height;
	}

	public String getTitle() {
		return title;
	}

	public void dispose() {
		glfwFreeCallbacks(window);
		glfwDestroyWindow(window);
	}
}
