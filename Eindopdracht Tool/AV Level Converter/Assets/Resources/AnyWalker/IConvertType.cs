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
	public static IConvertType[] types = new IConvertType[]{new IJPG(), new ITXT(), new IMP3(),  new IBMP()}; //new IWAV(), new IOGG(), new IBMP()};

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

	string extension {
		get;
		set;
	}
	GameObject Convert(string path, object[] vars);
}

public abstract class IConversionBaseType : IConvertType {
	public abstract List<Setting> variables {get;set;}

	protected Dictionary<string, object> settings = new Dictionary<string, object>();

	protected Vector3 sizeToApply = Vector3.zero;
	public Vector3 size {set{}
		get{return sizeToApply;}
	}

	protected System.Object def = null;
	public System.Object file {set{}
		get{return def;}
	}

	public abstract string extension {get;set;}

	protected void GenerateAudioIcon() {
		Texture2D tex = new Texture2D(20, 20, TextureFormat.RGBA32, false);
		//Text File Icon
		for(int x = 0; x < tex.width; x++)
			for(int y = 0; y < tex.height; y++) {
				tex.SetPixel(x, y, Generator.col_egg);
				for(int i = 0; i < 5; i++) tex.SetPixel(x, (tex.height / 2) + (int)(Mathf.Sin(x*x*10)*3+(tex.width/(x+1)*100)) - i + 1, Generator.col_semiblack);
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
				tex.SetPixel(x, y, Generator.col_egg);
				if(x > 25 && x < tex.width - 10 && y > 10 && y < tex.height - 10 && y % 5 == 0) {
						tex.SetPixel(x, y, Generator.col_semiblack);
						tex.SetPixel(x, y + 1, Generator.col_semiblack);
				}
				if(x < 30 && y > tex.height - 15) tex.SetPixel(x, y, Color.clear);
				if(x < 15) tex.SetPixel(x, y, Color.clear);
			}
		tex.Apply();
		file = def = tex;
		tex.filterMode = FilterMode.Point;
	}
	
	public GameObject Convert(string path, object[] vars) {
		for(int i = 0; i < vars.Length; i++) settings[variables[i].name] = vars[i];
		return Convert(path);
	}

	public virtual GameObject Convert(string path) {
		return Generator.Self.parent;
	}
}

public class IJPG : IConversionBaseType {
	public override List<Setting> variables {
		get {
			List<Setting> dict = new List<Setting>();
			dict.Add(new Setting("Step", 5, new object[]{0, 100}));
			dict.Add(new Setting("Amplitude", 5f, new object[]{0f, 1f}));
			dict.Add(new Setting("Randomiziation", true, new object[]{"Yes", "No"}));
			dict.Add(new Setting("Seed", "randomStringWillGoHere"));
			return dict;}
		set {}
	}

	public override string extension {
		get {return "JPG";}
		set {}
	}

    public override GameObject Convert(string path) {
		/* Texture2D tex = Texture2D.whiteTexture;
		WWW www = new WWW(path);
		www.LoadImageIntoTexture(tex);

		for(int x = 0; x < tex.width; x++)
			for(int y = 0; y < tex.height; y++) {
				Color col = tex.GetPixel(x, y);
				if(col.r > 0 && col.g > 0 && col.b > 0) Generator.CreateCube(new Vector3(x, y, 0));
			}*/
			return Generator.Self.parent;
	}
}
public class IBMP : IConversionBaseType {
	public override List<Setting> variables {
		get {
			List<Setting> dict = new List<Setting>();
			dict.Add(new Setting("Step", 5, new object[]{0, 100}));
			dict.Add(new Setting("Amplitude", 5f, new object[]{0f, 1f}));
			dict.Add(new Setting("Randomiziation", true, new object[]{"Yes", "No"}));
			dict.Add(new Setting("Seed", "randomStringWillGoHere"));
			return dict;}
		set {}
	}

	public override string extension {
		get {return "BMP";}
		set {}
	}

    public override GameObject Convert(string path) {
		if(File.Exists(path)) {
			Texture2D tex;
			byte[] fileData = File.ReadAllBytes(path);

			BMPLoader bmp = new BMPLoader();
			BMPImage bmpImg = bmp.LoadBMP(fileData);
			tex = bmpImg.ToTexture2D();

			sizeToApply = new Vector3(tex.width, tex.height, 0);

			for(int x = 0; x < tex.width; x++)
				for(int y = 0; y < tex.height; y++) {
					Color col = tex.GetPixel(x, y);
					if(col.r > 0 && col.g > 0 && col.b > 0) Generator.CreateCube(new Vector3(x, y, 0));
				}
			file = def = tex;
			tex.filterMode = FilterMode.Point;
		}
		return Generator.Self.parent;
	}
}

public class ITXT : IConversionBaseType {
	public override List<Setting> variables {
		get {
			List<Setting> dict = new List<Setting>();
			dict.Add(new Setting("Density", 0.5f, new object[]{0f, 1f}));
			dict.Add(new Setting("Scale", 5f, new object[]{0f, 1f}));
			dict.Add(new Setting("Amplitude", 1f, new object[]{0.1f, 4f}));
			dict.Add(new Setting("Randomiziation", true, new object[]{"Yes", "No"}));
			return dict;}
		set {}
	}

	public override string extension {
		get {return "txt";}
		set {}
	}

    public override GameObject Convert(string path) {
		if(File.Exists(path)) {
			GenerateTextIcon();

			string fileData = File.ReadAllText(path);
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
			for(int x = 0; x < width; x++)
				for(int y = 0; y < height; y++) {
					float amplitude = (float)settings["Amplitude"];
					int current = (int)(map[x * y] * amplitude);
					Generator.CreateCube(new Vector3(x/(float)settings["Density"], y/(float)settings["Density"], current));
				}
		}
		return Generator.Self.parent;
	}
}

/* 
public class IWAV : IConversionBaseType {
		public override List<Setting> variables {
		get {
			List<Setting> dict = new List<Setting>();
			dict.Add(new Setting("Low-Frequency Peaks", 5f, new object[]{0f, 10f}));
			dict.Add(new Setting("Mid-Frequency Peaks", 5f, new object[]{0f, 10f}));
			dict.Add(new Setting("High-Frequency Peaks", 5f, new object[]{0f, 10f}));
			dict.Add(new Setting("Length", 5, new object[]{1, 100}));
			return dict;}
		set {}
	}

	private delegate void Callback(AudioClip www);
	public void doneLoading(AudioClip www){
		SongInfo songInfo =  new AnalyzeManager().GetSongInfo(www);
		Debug.Log(songInfo);
		foreach(var item in songInfo.highFreqPeaks) {
			Debug.Log(item);
			Generator.CreateCube(new Vector3(item.Key,item.Value, 0));
		}
	}

	public override string extension {
		get {return "wav";}
		set {}
	}

	private IEnumerator loadAudio(string path, Callback whendone) {
		using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.WAV)) {
			yield return www.Send();
			if(www.isNetworkError) Debug.LogError(www.error);
			else {
				AudioClip cl = DownloadHandlerAudioClip.GetContent(www);
				whendone(cl);
			}
		}
	}

	public override GameObject Convert(string path) {
		//Generator.coroutineHandler.StartCoroutine(loadAudio(path, doneLoading));

		//for(float i = 0; i < songInfo.duration; i += 0.1f) {

		//}
		return Generator.Self.parent;
	}
}*/

public class IMP3 : IConversionBaseType {
		public override List<Setting> variables {
		get {
			List<Setting> dict = new List<Setting>();
			dict.Add(new Setting("Low-Frequency Peaks", 5f, new object[]{0f, 10f}));
			dict.Add(new Setting("Mid-Frequency Peaks", 5f, new object[]{0f, 10f}));
			dict.Add(new Setting("High-Frequency Peaks", 5f, new object[]{0f, 10f}));
			dict.Add(new Setting("Length", 5, new object[]{1, 100}));
			return dict;}
		set {}
	}

	public override string extension {
		get {return "mp3";}
		set {}
	}

	private delegate void Callback(SongInfo www);
	public void doneLoading(SongInfo songInfo) {
		for(int width = 0; width < songInfo.GetDataLength(); width++) {
			foreach(var item in songInfo.GetData(width)) Generator.CreateCube(new Vector3(width, item.time, -item.prunedSpectralFlux), new Vector3(1, 20, 20));
		}
	} 

	public override GameObject Convert(string path) {
		GenerateAudioIcon();
		//AudioClip ad = new AudioClip();
		new AnalyzeManager().GetSongInfo(Resources.Load("FlurryHurry") as AudioClip, doneLoading);
		return Generator.Self.parent;
	}
}

/* 
public class IOGG : IConversionBaseType {
		public override List<Setting> variables {
		get {
			List<Setting> dict = new List<Setting>();
			dict.Add(new Setting("Low-Frequency Peaks", 5f, new object[]{0f, 10f}));
			dict.Add(new Setting("Mid-Frequency Peaks", 5f, new object[]{0f, 10f}));
			dict.Add(new Setting("High-Frequency Peaks", 5f, new object[]{0f, 10f}));
			dict.Add(new Setting("Length", 5, new object[]{1, 100}));
			return dict;}
		set {}
	}

	public override string extension {
		get {return "ogg";}
		set {}
	}

	private delegate void Callback(AudioClip www);
	public void doneLoading(AudioClip www){
		SongInfo songInfo =  new AnalyzeManager().GetSongInfo(www);
		Debug.Log("Stap 4 jullie verdienen koffie");
		foreach(var item in songInfo.highFreqPeaks) {
			Debug.Log("Wees maar blij at this point");
			Generator.CreateCube(new Vector3(item.Key,item.Value, 0));
		}
	}

	private IEnumerator loadAudio(string path, Callback whendone) {
		using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.OGGVORBIS)) {
			Debug.Log("Stap 2 soortvan ish gehaald");
			yield return www.Send();
			if(www.isNetworkError) Debug.LogError(www.error);
			else {
				AudioClip cl = DownloadHandlerAudioClip.GetContent(www);
				Debug.Log("Stap 3 het is soortvan gelukt");
				whendone(cl);
			}
		}
	}

	public override GameObject Convert(string path) {
		Debug.Log("Stap 1 gehaald");
		Generator.coroutineHandler.StartCoroutine(loadAudio(path, doneLoading));
		return Generator.Self.parent;
	}
}*/