using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Collections;

[CustomEditor(typeof(Generator))]
public class GeneratorEditor : Editor {
	public static string FILE_TYPE = "bruut";
	private	string path = "", buttonName = "Load Directory";
	private List<string> files = new List<string>();

	private Texture folderIcon;

	private bool hasDirectory = false;
	private DirectoryInfo currentDir;

	[System.Serializable]
	public struct SaveData {
		public int mode;
		public string dir;
		public string file;
	}
	private SaveData save;
	private string  pathJSON;
	private int lastMode = 0;
	private string lastFile = "";

	private void LoadSettings() {
		pathJSON = Application.persistentDataPath + "/settings";
		Debug.LogWarning("JSON file generated at: " + pathJSON);
		string content = File.ReadAllText(pathJSON);
		save = JsonUtility.FromJson<SaveData>(content);
		path = save.dir;
		lastMode = save.mode;
		lastFile = save.file;
		currentDir = new DirectoryInfo(path);
		hasDirectory = true;
	}

	private void SaveSettings() {
		save.dir = path;
		string content = JsonUtility.ToJson(save, true);
	    File.WriteAllText(pathJSON, content);
	}

	void OnEnable() {
		LoadSettings();
	}

	public override void OnInspectorGUI() {
		Generator tar = (Generator)target;
		
		//EditorGUI.ProgressBar(Rect, value:, label);

		//Colors
		Color col_egg = new Color(), col_black = new Color(), col_semiblack = new Color();
		ColorUtility.TryParseHtmlString("#eaf0ce", out col_egg);
		ColorUtility.TryParseHtmlString("#333232", out col_black);
		ColorUtility.TryParseHtmlString("#474647", out col_semiblack);

		//Styling
		GUIContent content = new GUIContent();
		GUIStyle style = new GUIStyle(GUI.skin.button), labelstyle = EditorStyles.boldLabel, generateButton = new GUIStyle(GUI.skin.button), elementBtn = new GUIStyle(GUI.skin.button);
		style.imagePosition = ImagePosition.ImageLeft;
		style.fixedHeight = style.fixedWidth = 32;
		elementBtn.fontStyle = FontStyle.Bold;
		elementBtn.onActive.textColor = col_egg;
		elementBtn.onNormal.textColor = col_egg;
		elementBtn.active.textColor = col_semiblack;
		generateButton.active.textColor = col_black;
		generateButton.normal.textColor = col_semiblack;
		generateButton.onHover.textColor = col_black;
		generateButton.fontStyle = FontStyle.Bold;
		generateButton.padding = new RectOffset(10, 10, 10, 5);
		labelstyle.normal.textColor = col_egg;

		//Load Icons
		folderIcon = Resources.Load("Folder") as Texture;
		content.image = (Texture2D)folderIcon;
		content.tooltip = "Load a main directory folder where all the ."+FILE_TYPE+" files are located.";

		//File loading
		GUI.backgroundColor = col_semiblack;
		GUILayout.BeginVertical("Box");
		GUILayout.Label("File Loading", labelstyle);
		GUI.backgroundColor = col_black;
		GUILayout.BeginHorizontal("box");
		GUI.backgroundColor = col_egg;
		if(GUILayout.Button(content,style)) 
		{
			path = EditorUtility.OpenFolderPanel("Select ."+FILE_TYPE+" directory", "", FILE_TYPE);
			currentDir = new DirectoryInfo(path);
			SaveSettings();
			FileInfo[] info = currentDir.GetFiles("*."+FILE_TYPE);
			foreach(FileInfo f in info) files.Add(f.Name);
		}
		GUI.backgroundColor = Color.white;

		if(path.Length <= 1) {
			buttonName = "Load Directory\n <--";
			hasDirectory = false;
		} else {
			buttonName = "Directory loaded: \n" + Simplify(path);
			hasDirectory = true;
		}
		GUILayout.Box(buttonName, labelstyle);

		GUI.backgroundColor = col_egg;
		if(GUILayout.Button("Reset", elementBtn, GUILayout.Width(60))) {
			path = "";
			currentDir = null;
			hasDirectory = false;
		}
		GUILayout.EndHorizontal();
		
		//Open File Panel
		int labelSize = labelstyle.fontSize;
		if(hasDirectory) {
			GUILayout.BeginVertical("Box");
			FileInfo[] files = GetFileList();
			for(int i = 0; i < files.Length; i++) {
				if(save.file == files[i].Name) {
					GUI.backgroundColor = col_semiblack;
					labelstyle.normal.textColor = col_egg;
					labelstyle.fontSize = 12;
				}
				else {
					GUI.backgroundColor = col_egg;
					labelstyle.normal.textColor = col_semiblack;
					labelstyle.fontSize = labelSize;
				}
				GUILayout.BeginHorizontal("box");
				if(GUILayout.Button(files[i].Name, labelstyle)) {
					save.file = files[i].Name;
					ScanFileType();
				}
				GUILayout.EndHorizontal();
			}
			if(save.file != lastFile) SaveSettings();
			GUILayout.EndVertical();
		}
		GUILayout.EndVertical();

		//Reset style vars
		labelstyle.fontSize = labelSize;
		labelstyle.normal.textColor = col_egg;

		//GameType
		if(hasDirectory) {
			GUILayout.Space(10);
			GUI.backgroundColor = col_semiblack;
			GUILayout.BeginVertical("Box");
			GUILayout.Label("Game Type", EditorStyles.boldLabel);
			string[] names = new string[Generator.AMOUNT_GAMEMODES()];
			for(int i = 0; i < names.Length; i++) names[i] = ((Generator.GameType[])System.Enum.GetValues(typeof(Generator.GameType)))[i].ToString().Replace('_', ' ');

			GUI.backgroundColor = col_egg;
			elementBtn.padding = new RectOffset(0, 0, 5, 5);
			save.mode = GUILayout.SelectionGrid(save.mode, names, 2, elementBtn);
			GUILayout.EndVertical();
			if(save.mode != lastMode) SaveSettings();

			GUILayout.Space(10);

			//Preview Window
			GUI.backgroundColor = col_semiblack;
			GUILayout.BeginVertical("Box");
			GUILayout.Label("Preview", EditorStyles.boldLabel);
			GUI.backgroundColor = col_egg;
			if(GUILayout.Button("Open Level Editor", generateButton)) {
				
			}
			GUILayout.EndVertical();
			GUILayout.Space(5);

			//Generate button
			GUI.backgroundColor = col_semiblack;
			GUILayout.BeginVertical("Box");
			GUILayout.Label("Generation", EditorStyles.boldLabel);
			GUI.backgroundColor = col_egg;
			if(GUILayout.Button("Generate Level", generateButton)) tar.Generate();
			GUILayout.EndVertical();
			GUILayout.Space(5);
		}
	}

	public FileInfo[] GetFileList() {
		List<FileInfo> files = new List<FileInfo>();
		foreach(FileInfo inf in currentDir.GetFiles()) {
			if(inf.Extension == ".meta") continue;
			files.Add(inf);
		}
		return files.ToArray();
	}

	public IConvertType ScanFileType() {
		DirectoryInfo currentFile = new DirectoryInfo(save.dir + "/" + save.file);
		string extension = currentFile.Extension.Replace('.', ' ').ToLower().Trim();
		if(AVLevelConverter.FILETYPES[extension] != null) {
			Debug.Log(AVLevelConverter.FILETYPES[extension]);
			return AVLevelConverter.FILETYPES[extension];
		}
		return null;
	}

	public string Simplify(string path) {
		string[] splits = path.Split('/');
		string[] data = Application.dataPath.Split('/');
		string fin = "";
		System.Array.Resize(ref data, 8);
		System.Array.Reverse(splits);
		System.Array.Reverse(data);
		bool lastShorten = false;
		for(int i = splits.Length - 1; i >= 0; i--) {
			bool shorten = false;
			if(splits[i] != "C:") {
			 foreach(string m in data) if(m.Trim() == splits[i].Trim()) {
				 if(!lastShorten) {
					 fin += "../"; 
					 lastShorten = true;
				 }
				 shorten = true; 
				 break;
			 }
			}
			if(!shorten) fin += splits[i] + "/";
		}
		return fin;
	}
}
