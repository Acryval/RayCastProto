package acv.rcproto.shader;

import acv.rcproto.RCProto;

import java.io.*;
import java.util.Objects;

import static org.lwjgl.opengl.GL40.*;

public abstract class AbstractShader {
	
	public static final String SHADER_PREFIX = "shader/";
	
	private String vName, fName;
	protected int pID, vsID, fsID;
	protected boolean compiled, started;
	protected static boolean srcDbg = false;
	
	// CONSTRUCTOR //
	
	
	public AbstractShader(String VSName, String FSName) {
		vName = VSName;
		fName = FSName;
		compiled = false;
		started = false;
	}
	
	
	// PRIVATE FUNCTIONS //
	
	
	//
	
	
	// PROTECTED FUNCTIONS //
	
		// ABSTRACT //
	protected abstract String[] prepareShaderData();
	protected abstract void bindAttributes();
	protected abstract void getUniformLocations();
		// ABSTRACT //
	
	protected void bindAttribute(int attribute, String name) {
		glBindAttribLocation(pID, attribute, name);
	}
	
	protected int getUniformLocation(String name) {
		return glGetUniformLocation(pID, name);
	}
	
	protected int getUniformBlockIndex(String name) {
		return glGetUniformBlockIndex(pID, name);
	}
	
	protected void bindUniformBlock(int indx, int binding) {
		glUniformBlockBinding(pID, indx, binding);
	}
	
	
	// PUBLIC FUNCTIONS //
	
	
	public void compile() {
		String[] cData = prepareShaderData();
		
		vsID = loadShader(SHADER_PREFIX + vName + ".vs", GL_VERTEX_SHADER, cData);
		fsID = loadShader(SHADER_PREFIX + fName + ".fs", GL_FRAGMENT_SHADER, cData);
		
		pID = glCreateProgram();
		
		glAttachShader(pID, vsID);
		glAttachShader(pID, fsID);
		
		bindAttributes();
		
		glLinkProgram(pID);
		glValidateProgram(pID);
		glUseProgram(pID);
		
		getUniformLocations();
		compiled = true;
	}
	
	public void start() {
		glUseProgram(pID);
		started = true;
	}
	
	public void stop() {
		glUseProgram(0);
		started = false;
	}

	public boolean isStarted(){
		return started;
	}
	
	public int getProgramID() {
		return pID;
	}

	public int getVertexShaderID() {
		return vsID;
	}

	public int getFragmentShaderID() {
		return fsID;
	}

	public void drawScene() {
		glBegin(GL_QUADS);

		glVertex2f(-1, 1);
		glVertex2f(-1, -1);
		glVertex2f(1, -1);
		glVertex2f(1, 1);

		glEnd();
	}

	public void dispose() {
		stop();
		compiled = false;
		
		glDetachShader(pID, vsID);
		glDetachShader(pID, fsID);
		
		glDeleteShader(vsID);
		glDeleteShader(fsID);
		
		glDeleteProgram(pID);
	}
	
	
	//  STATIC FUNCTIONS  //
	
	
	public static int loadShader(String shaderFilepath, int shaderType, String...data) {
		StringBuilder src = new StringBuilder();
		
		try {
			BufferedReader br = new BufferedReader(new InputStreamReader(Objects.requireNonNull(ClassLoader.getSystemResourceAsStream(shaderFilepath))));
			
			String ln;
			while((ln = br.readLine()) != null) {
				src.append(ln).append("\n");
			}
			
			br.close();
		}catch(IOException e) {
			System.err.println("Shader file couldn't be read");
			e.printStackTrace();
			System.exit(0);
		}
		
		String[] split = src.toString().split("\\$");
		src = new StringBuilder(split[0]);
		int ind;
		for(int i = 1; i < split.length; i+=2) {
			ind = Integer.parseInt(split[i]) - 1;
			if(ind < data.length) {
				src.append(data[ind]);
			}
			src.append(split[i + 1]);
		}
		
		if(srcDbg) {
			System.out.println(src);
		}
		
		int sid = glCreateShader(shaderType);
		
		glShaderSource(sid, src.toString());
		glCompileShader(sid);
		
		if(glGetShaderi(sid, GL_COMPILE_STATUS) == GL_FALSE) {
			System.err.println(glGetShaderInfoLog(sid, 512));
			System.err.println("Shader couldn't be compiled");
			System.exit(0);
		}
		
		return sid;
	}

	public static void srcDebug(){
		srcDbg = true;
	}
	
	public static String[] fuseStrArrays(String[]...arrays) {
		int sum = 0, i = 0;

		for(String[] array : arrays) {
			sum += array.length;
		}
		
		String[] s = new String[sum];
		sum = 0;

		for(String[] array : arrays) {
			for (i = 0; i < array.length; i++) {
				s[sum + i] = array[i];
			}
			sum += i;
		}
		
		return s;
	}
}