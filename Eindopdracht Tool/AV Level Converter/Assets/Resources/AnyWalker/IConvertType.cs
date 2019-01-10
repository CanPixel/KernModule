using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using Loader;
using AlgorithmicBeatDetection;
using UnityEngine.Networking;

[InitializeOnLoad]
public class IConvert {
	public static Dictionary<string, IConvertType> FILETYPES = new Dictionary<string, IConvertType>();
	
	/* THE IMPORTED TYPE CONVERSIONS */
	/*To add more file type conversions, simply include the implementation of the IConvertType interface and the respective file extension as a string */
	public static IConvertType[] types = new IConvertType[]{new ITXT(), new IMP3(),  new IBMP(), new IWAV()};//, new IJPG(), new IOGG(), new IPNG(), new IGIF()};

	static IConvert() {
		foreach(IConvertType conv in types) FILETYPES.Add(conv.extension.ToLower(), conv);
	}
}

[System.Serializable]
public class Setting {
	public string name {private set;get;}
	public object value {private set;get;}
	public object[] options {private set;get;}

	public Setting(string name, object value,  object[] options) {
		this.name = name;
		this.value = value;
		this.options = options;
	}
	public Setting(string name, object value) : this(name, value, null) {}
}

public interface IConvertType {
	List<Setting> variables {set;get;}

	System.Object file {set;get;}

	Vector3 size {set;get;}

	void SetSettings(AnyWalker.GameType gt);

	string extension {
		get;
		set;
	}
	GameObject Convert(string path, object[] vars, AnyWalker.GameType type);
}

public abstract class IConversionBaseType : IConvertType {
	public List<Setting> dict = new List<Setting>();
	public virtual List<Setting> variables {
		get {return dict;}
		set {
			dict = value;
			UpdateSettings();}
	}
	protected Dictionary<string, object> settings = new Dictionary<string, object>();

	protected Vector3 sizeToApply = Vector3.zero;
	public Vector3 size {set{sizeToApply = value;}
		get{return sizeToApply;}
	}

	protected System.Object def = null;
	public System.Object file {set{}
		get{return def;}
	}

	public void UpdateSettings() {
		settings.Clear();
		for(int i = 0; i < variables.Count; i++) settings.Add(variables[i].name, variables[i].value);
	}

	public abstract string extension {get;set;}

	protected void GenerateAudioIcon() {
		Texture2D tex = new Texture2D(20, 20, TextureFormat.RGBA32, false);
		//Text File Icon
		for(int x = 0; x < tex.width; x++)
			for(int y = 0; y < tex.height; y++) {
				tex.SetPixel(x, y, AnyWalker.col_egg);
				for(int i = 0; i < 5; i++) tex.SetPixel(x, (tex.height / 2) + (int)(Mathf.Sin(x*x*10)*3+(tex.width/(x+1)*100)) - i + 1, AnyWalker.col_semiblack);
				for(int i = 0; i < 2; i++) tex.SetPixel(x, (tex.height / 2) + (int)(Mathf.Sin(x*x*10)+(tex.width/(x+1)*100)) - i - 1, Color.black);
			}
		tex.Apply();
		file = def = tex;
		tex.filterMode = FilterMode.Point;
	}
	protected void GenerateTextIcon() {
		Texture2D tex = new Texture2D(100, 100, TextureFormat.RGBA32, false);
		//Text File Icon
		for(int x = 0; x < tex.width; x++)
			for(int y = 0; y < tex.height; y++) {
				tex.SetPixel(x, y, AnyWalker.col_egg);
				if(x > 25 && x < tex.width - 10 && y > 10 && y < tex.height - 10 && y % 5 == 0) {
						tex.SetPixel(x, y, AnyWalker.col_semiblack);
						tex.SetPixel(x, y + 1, AnyWalker.col_semiblack);
				}
				if(x < 30 && y > tex.height - 15) tex.SetPixel(x, y, Color.clear);
				if(x < 15) tex.SetPixel(x, y, Color.clear);
			}
		tex.Apply();
		file = def = tex;
		tex.filterMode = FilterMode.Point;
	}
	
	public GameObject Convert(string path, object[] vars, AnyWalker.GameType type) {
		for(int i = 0; i < vars.Length; i++) settings[variables[i].name] = vars[i];
		return Convert(path, type);
	}

	public virtual GameObject Convert(string path, AnyWalker.GameType type) {
		return AnyWalker.Self.parent;
	}

	public virtual void SetSettings(AnyWalker.GameType gt) {}
}

public class IJPG : IConversionBaseType {
	public override string extension {
		get {return "JPG";}
		set {}
	}

	override public void SetSettings(AnyWalker.GameType type) {
		variables.Clear();
		List<Setting> dict = new List<Setting>();
		switch(type) {
				case AnyWalker.GameType.Landscape:
					dict.Add(new Setting("Step", 5, new object[]{0, 100}));
					dict.Add(new Setting("Amplitude", 1f, new object[]{1f, 1f}));
					dict.Add(new Setting("Randomiziation", true, new object[]{"Yes", "No"}));
					dict.Add(new Setting("Seed", "randomStringWillGoHere"));
					break;
				case AnyWalker.GameType.Layered:
					dict.Add(new Setting("Step", 5, new object[]{0, 100}));
					dict.Add(new Setting("Wall Height", 1f, new object[]{1f, 10f}));
					dict.Add(new Setting("Randomiziation", true, new object[]{"Yes", "No"}));
					dict.Add(new Setting("Seed", "saduifbdausi"));
					break;
		}
		variables = dict;
	}

    public override GameObject Convert(string path, AnyWalker.GameType type) {
		/* Texture2D tex = Texture2D.whiteTexture;
		WWW www = new WWW(path);
		www.LoadImageIntoTexture(tex);

		for(int x = 0; x < tex.width; x++)
			for(int y = 0; y < tex.height; y++) {
				Color col = tex.GetPixel(x, y);
				if(col.r > 0 && col.g > 0 && col.b > 0) AnyWalker.CreateCube(new Vector3(x, y, 0));
			}*/
			return AnyWalker.Self.parent;
	}
}
public class IBMP : IConversionBaseType {
	override public void SetSettings(AnyWalker.GameType type) {
		variables.Clear();
		List<Setting> dict = new List<Setting>();
		switch(type) {
				case AnyWalker.GameType.Landscape:
					dict.Add(new Setting("Noise", 0f, new object[]{0f, 5f}));
					dict.Add(new Setting("Amplitude", 1f, new object[]{1f, 10f}));
					dict.Add(new Setting("Colors", new AnyWalker.TerrainType(), new object[]{3}));
					dict.Add(new Setting("Smoothing", FilterMode.Point, new object[]{FilterMode.Point, FilterMode.Bilinear, FilterMode.Trilinear}));
					break;
				case AnyWalker.GameType.Runner:
					dict.Add(new Setting("Sample Size", 1, new object[]{1, 5}));
					dict.Add(new Setting("Wall Height", 1f, new object[]{1f, 10f}));
					dict.Add(new Setting("Density", 1f, new object[]{1f, 3f}));
					dict.Add(new Setting("Bump", 0.5f, new object[]{0f, 5f}));
					break;
				case AnyWalker.GameType.Layered:
					dict.Add(new Setting("Sample Size", 1, new object[]{1, 5}));
					dict.Add(new Setting("Wall Height", 1f, new object[]{1f, 10f}));
					dict.Add(new Setting("Density", 1f, new object[]{1f, 3f}));
					dict.Add(new Setting("Bump", 0.5f, new object[]{0f, 5f}));
					break;
		}
		variables = dict;
	}

	public override string extension {
		get {return "BMP";}
		set {}
	}

	protected void GenerateMesh(Texture2D tex) {
		List<Vector3[]> verts = new List<Vector3[]>();
		List<int> tris = new List<int>();
		List<Vector2> uvs = new List<Vector2>();

		AnyWalker.TerrainType type = ((AnyWalker.TerrainType)settings["Colors"]);
		int spacing = 50;
		
		for(int x = 0; x < tex.width; x++) {
			verts.Add(new Vector3[tex.width]);
			for(int y = 0; y < tex.height; y++) {
				Vector3 currentPoint = new Vector3();
				currentPoint.x = (x * spacing) - tex.width / 2;
				currentPoint.z = y * spacing - tex.height / 2;
				int offset = y % 2;
				if(offset == 1) currentPoint.x -= spacing * 0.5f;

				Color col = tex.GetPixel(x, y);
				if(col.r > 0 && col.g > 0 && col.b > 0) currentPoint.y = -spacing * (float)settings["Amplitude"];
				else currentPoint.y = 0;

				verts[x][y] = currentPoint;
				uvs.Add(new Vector2(x, y));
				if(x == 0 || y == 0) continue;
				tris.Add(tex.width * x + y);
				tris.Add(tex.width * x + (y - 1));
				tris.Add(tex.width * (x - 1) + (y - 1));
				tris.Add(tex.width * (x - 1) + (y - 1));
				tris.Add(tex.width * (x - 1) + y);
				tris.Add(tex.width * x + y);

				int curX = x + (1-offset);
				if(curX - 1 <= 0 || y <= 0 || curX >= tex.width) continue;
			}
		}
	
		//Copy verts into 1D array
		Vector3[] unfoldedVerts = new Vector3[tex.width*tex.width];
		int i = 0;
		foreach(Vector3[] v in verts) {
			v.CopyTo(unfoldedVerts, i * tex.width);
			i++;
		}

		//Apply settings to 1D array
		for(int v = 0; v < unfoldedVerts.Length; v++) {
			Vector3 vx = unfoldedVerts[v];
			float fin = (float)settings["Noise"]* 10 * Mathf.PerlinNoise(vx.x / tex.width, vx.z / tex.height)*5;
			unfoldedVerts[v] = new Vector3(vx.x, vx.y + fin, vx.z);
		}

		Color[,] colorMap = new Color[tex.width, tex.height];
		int lowest = 0;
		float max = 0, min = 0;
		for(int v = 0; v < unfoldedVerts.Length; v++) {
			if(type.colors.Count > 0) {
				Vector3 currentPoint = unfoldedVerts[v];
				float fin = currentPoint.y;
				for(int l = 0; l < type.colors.Count; l++)  {
					if(l > 0 && type.heights[l] < type.heights[l - 1]) lowest = l;
					if(max < fin) max = fin;
					if(min > fin) min = fin;
				}
			}
		}
	for(int x = 0; x < tex.width; x++) 
		for(int y = 0; y < tex.height; y++) {
				Vector3 currentPoint = verts[x][y];
				currentPoint.x = (x * spacing) - tex.width / 2;
				currentPoint.z = (y * spacing) - tex.height / 2;
				int offset = y % 2;
				if(offset == 1) currentPoint.x -= spacing * 0.5f;

				Color col = tex.GetPixel(x, y);
				if(col.r > 0 && col.g > 0 && col.b > 0) currentPoint.y = -spacing * (float)settings["Amplitude"];
				else currentPoint.y = 0;

				float noise = (float)settings["Noise"]* 10 * Mathf.PerlinNoise(currentPoint.x / tex.width, currentPoint.z / tex.height)*5;
				float fin = currentPoint.y + noise;
				float f = (fin - min) / (max - min);
				for(int l = 0; l < type.colors.Count; l++)  {
					try {
						if(f <= type.heights[l]) {
							colorMap[x, y] = type.colors[l];
							break;
						}
						else colorMap[x, y] = type.colors[lowest];
					}
					catch(System.IndexOutOfRangeException) {}
				}
		}

		//Color map to Texture2D
		Color[] colMap = new Color[tex.width * tex.height];
		for(int j = 0; j < colorMap.GetLength(1); j++)
			for(int m = 0; m < colorMap.GetLength(0); m++) {
				colMap[j*colorMap.GetLength(1)+m] = colorMap[m, j];
			}
		Texture2D Colored = new Texture2D(tex.width, tex.height);
		Colored.filterMode = (FilterMode)settings["Smoothing"];
		Colored.wrapMode = TextureWrapMode.Clamp;
		Colored.SetPixels(colMap);
		Colored.Apply();

		Mesh mesh = new Mesh();
		mesh.vertices = unfoldedVerts;
		mesh.triangles = tris.ToArray();
		mesh.uv = uvs.ToArray();

		mesh.RecalculateBounds();
		mesh.RecalculateTangents();
		mesh.RecalculateNormals();
		AnyWalker.BindMesh(mesh, Colored, spacing);
	} 
	protected void GenerateLayers(Texture2D tex) {
		int sample =  (int)settings["Sample Size"];
		for(int x = 0; x < tex.width / sample; x++)
			for(int y = 0; y < tex.height / sample; y++) {
				Color col = tex.GetPixel(x * sample, y * sample);
				if(col.r > 0 && col.g > 0 && col.b > 0) AnyWalker.CreateCube(new Vector3(x*(float)settings["Density"], y*(float)settings["Density"], Random.Range(0f, (float)settings["Bump"])));
				else for(int i = 0; i < (float)settings["Wall Height"]; i++) AnyWalker.CreateCube(new Vector3(x*(float)settings["Density"], y*(float)settings["Density"], -i), Vector3.one, 1);
			}
		size = new Vector3(tex.width/sample, 0, tex.height/sample);
	}

	protected void GenerateRunner(Texture2D tex) {
		int sample =  (int)settings["Sample Size"];
		for(int x = 0; x < tex.width / sample; x++)
			for(int y = 0; y < tex.height / sample; y++) {
				Color col = tex.GetPixel(x * sample, y * sample);
				if(col.r > 0 && col.g > 0 && col.b > 0) AnyWalker.CreateCube(new Vector3(x * (float)settings["Density"] + y * (float)settings["Density"], 0, Random.Range(0f, (float)settings["Bump"])));
				else for(int i = 0; i < (float)settings["Wall Height"] + 1; i++) AnyWalker.CreateCube(new Vector3(x * (float)settings["Density"] + y * (float)settings["Density"], i, 0), Vector3.one, 1);
			}
		size = new Vector3(tex.width / sample, 0, tex.height / sample);
	}

    public override GameObject Convert(string path, AnyWalker.GameType type) {
		if(File.Exists(path)) {
			Texture2D tex;
			byte[] fileData = File.ReadAllBytes(path);

			BMPLoader bmp = new BMPLoader();
			BMPImage bmpImg = bmp.LoadBMP(fileData);
			tex = bmpImg.ToTexture2D();
			sizeToApply = new Vector3(tex.width, tex.height, 0);

			switch(type) {
				case AnyWalker.GameType.Landscape:
					GenerateMesh(tex);
					break;
				case AnyWalker.GameType.Layered:
					GenerateLayers(tex);
					break;
				case AnyWalker.GameType.Runner:
					GenerateRunner(tex);
					break;
				default:
					break;
			}
			file = def = tex;
			tex.filterMode = FilterMode.Point;
		}
		return AnyWalker.Self.parent;
	}
}

public class ITXT : IConversionBaseType {
	public override string extension {
		get {return "txt";}
		set {}
	}

	override public void SetSettings(AnyWalker.GameType type) {
		variables.Clear();
		List<Setting> dict = new List<Setting>();
		switch(type) {
				case AnyWalker.GameType.Landscape:
					dict.Add(new Setting("Density", 0.5f, new object[]{0f, 1f}));
					dict.Add(new Setting("Scale", 5f, new object[]{0f, 1f}));
					dict.Add(new Setting("Amplitude", 1f, new object[]{0.1f, 4f}));
					dict.Add(new Setting("Randomiziation", true, new object[]{"Yes", "No"}));
					break;
				case AnyWalker.GameType.Layered:
					dict.Add(new Setting("Density", 0.5f, new object[]{0f, 1f}));
					dict.Add(new Setting("Scale", 5f, new object[]{0f, 1f}));
					dict.Add(new Setting("Amplitude", 1f, new object[]{0.1f, 4f}));
					dict.Add(new Setting("Randomiziation", true, new object[]{"Yes", "No"}));
					break;
		}
		variables = dict;
	}

    public override GameObject Convert(string path, AnyWalker.GameType type) {
		if(File.Exists(path)) {
			GenerateTextIcon();

			string fileData;
			try {
				fileData = File.ReadAllText(path);
			} catch(IOException) {
				Debug.LogError("File in use!");
				return null;
			}

			string[] words = fileData.Replace('.', ' ').Replace(',', ' ').Trim().Split(' ');
			List<int> map = new List<int>();
			foreach(string i in words) {
				char[] ch = i.ToCharArray();
				foreach(char c in ch) {
					int value = (int)c;
					int sum = 0;
					while(value != 0) {
						sum += value % 10;
						value /= 10;
					}
					map.Add(sum);
				}
			}
			int fl = Mathf.Min(words.Length / 2, words.Length - (words.Length / 2));
			int size = map.Count - fl;
			int width = (int)Mathf.Sqrt(size);
			int height = width;
			
			//Size Cap
			const int CAP = 50;
			width = Mathf.Clamp(width, width, CAP);
			height = Mathf.Clamp(height, height, CAP);

			for(int x = 0; x < width; x++)
				for(int y = 0; y < height; y++) {
					float amplitude = (float)settings["Amplitude"];
					int current = (int)(map[x * y] * amplitude);
					AnyWalker.CreateCube(new Vector3(x/(float)settings["Density"], y/(float)settings["Density"], current));
				}
		}
		return AnyWalker.Self.parent;
	}
}
 
public class IWAV : IConversionBaseType {
	override public void SetSettings(AnyWalker.GameType type) {
		variables.Clear();
		List<Setting> dict = new List<Setting>();
		switch(type) {
				case AnyWalker.GameType.Landscape:
					dict.Add(new Setting("Amplitude", 1f, new object[]{1f, 10f}));
					dict.Add(new Setting("Mid-Frequency Peaks", 5f, new object[]{0f, 10f}));
					dict.Add(new Setting("High-Frequency Peaks", 5f, new object[]{0f, 10f}));
					dict.Add(new Setting("Length", 5, new object[]{1, 100}));
					break;
				case AnyWalker.GameType.Layered:
					dict.Add(new Setting("Amplitude", 1f, new object[]{1f, 10f}));
					dict.Add(new Setting("Mid-Frequency Peaks", 5f, new object[]{0f, 10f}));
					dict.Add(new Setting("High-Frequency Peaks", 5f, new object[]{0f, 10f}));
					dict.Add(new Setting("Length", 5, new object[]{1, 100}));
					break;
		}
		variables = dict;
	}

	public override string extension {
		get {return "wav";}
		set {}
	}

	public void doneLoading(SongInfo songInfo) {
		for(int width = 0; width < songInfo.GetDataLength(); width++) {
			const int CAP = 500;
			int count = 0;
			foreach(var item in songInfo.GetData(width)) {
				AnyWalker.CreateCube(new Vector3(width, item.time, -item.prunedSpectralFlux*(float)settings["Amplitude"]), new Vector3(1, 20, 20));
				count++;
				if(count > CAP) break;
			}
		}
	} 

	public override GameObject Convert(string path, AnyWalker.GameType type) {
		using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.WAV)) {
			www.SendWebRequest();
			while(!www.isDone) ;
			if(www.isNetworkError || www.isHttpError) Debug.LogError(www.error);
			else if(www.downloadHandler.isDone) {
				AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
				new AnalyzeManager().GetSongInfo(clip, doneLoading);
			}
		}
		return AnyWalker.Self.parent;
	}
}

public class IMP3 : IConversionBaseType {
	override public void SetSettings(AnyWalker.GameType type) {
		variables.Clear();
		List<Setting> dict = new List<Setting>();
		switch(type) {
				case AnyWalker.GameType.Landscape:
					dict.Add(new Setting("Amplitude", 1f, new object[]{1f, 10f}));
					dict.Add(new Setting("Mid-Frequency Peaks", 5f, new object[]{0f, 10f}));
					dict.Add(new Setting("High-Frequency Peaks", 5f, new object[]{0f, 10f}));
					dict.Add(new Setting("Length", 5, new object[]{1, 100}));
					break;
				case AnyWalker.GameType.Layered:
					dict.Add(new Setting("Amplitude", 1f, new object[]{1f, 10f}));
					dict.Add(new Setting("Mid-Frequency Peaks", 5f, new object[]{0f, 10f}));
					dict.Add(new Setting("High-Frequency Peaks", 5f, new object[]{0f, 10f}));
					dict.Add(new Setting("Length", 5, new object[]{1, 100}));
					break;
		}
		variables = dict;
	}

	public override string extension {
		get {return "mp3";}
		set {}
	}

	public void doneLoading(SongInfo songInfo) {
		for(int width = 0; width < songInfo.GetDataLength(); width++) {
			foreach(var item in songInfo.GetData(width)) AnyWalker.CreateCube(new Vector3(width, item.time, -item.prunedSpectralFlux), new Vector3(1, 20, 20));
		}
	} 

	private float[] ConvertByteToFloat(byte[] array) {
		float[] floatArr = new float[array.Length / 4];
		for(int i = 0; i < floatArr.Length; i += 2) {
			if(System.BitConverter.IsLittleEndian) System.Array.Reverse(array, i * 4, 4);
			floatArr[i] = (float)(System.BitConverter.ToInt16(array, i * 2) / 8000 * ((float)settings["Amplitude"]));
		}
		return floatArr;
	}

	public override GameObject Convert(string path, AnyWalker.GameType type) {
		GenerateAudioIcon();
		
		byte[] file = File.ReadAllBytes(path);
		float[] data = ConvertByteToFloat(file);
		AudioClip clip = AudioClip.Create("FlurryHurry", data.Length, 1, 44100, false);
		clip.SetData(data, 0);

		new AnalyzeManager().GetSongInfo(clip, doneLoading);
		return AnyWalker.Self.parent;
	}
}

public class IOGG : IConversionBaseType {
	override public void SetSettings(AnyWalker.GameType type) {
		variables.Clear();
		List<Setting> dict = new List<Setting>();
		switch(type) {
				case AnyWalker.GameType.Landscape:
					dict.Add(new Setting("Amplitude", 1f, new object[]{1f, 10f}));
					dict.Add(new Setting("Mid-Frequency Peaks", 5f, new object[]{0f, 10f}));
					dict.Add(new Setting("High-Frequency Peaks", 5f, new object[]{0f, 10f}));
					dict.Add(new Setting("Length", 5, new object[]{1, 100}));
					break;
				case AnyWalker.GameType.Layered:
					dict.Add(new Setting("Amplitude", 1f, new object[]{1f, 10f}));
					dict.Add(new Setting("Mid-Frequency Peaks", 5f, new object[]{0f, 10f}));
					dict.Add(new Setting("High-Frequency Peaks", 5f, new object[]{0f, 10f}));
					dict.Add(new Setting("Length", 5, new object[]{1, 100}));
					break;
		}
		variables = dict;
	}

	public override string extension {
		get {return "ogg";}
		set {}
	}

	public void doneLoading(SongInfo songInfo) {
		for(int width = 0; width < songInfo.GetDataLength(); width++) {
			const int CAP = 500;
			int count = 0;
			foreach(var item in songInfo.GetData(width)) {
				AnyWalker.CreateCube(new Vector3(width, item.time, -item.prunedSpectralFlux*(float)settings["Amplitude"]), new Vector3(1, 20, 20));
				count++;
				if(count > CAP) break;
			}
		}
	} 

	public override GameObject Convert(string path, AnyWalker.GameType type) {
		using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.OGGVORBIS)) {
			www.SendWebRequest();
			while(!www.isDone) ;
			if(www.isNetworkError || www.isHttpError) Debug.LogError(www.error);
			else if(www.downloadHandler.isDone) {
				AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
				new AnalyzeManager().GetSongInfo(clip, doneLoading);
			}
		}
		return AnyWalker.Self.parent;
	}
}