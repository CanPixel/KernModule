﻿using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Collections;

[CustomEditor(typeof(AnyWalker))]
public class AnyWalkerEditor : Editor {
	private	string path = "", buttonName = "Load Directory";
	private List<string> files = new List<string>();

	public static string TOOL_NAME = "AnyWalker";
	public static string[] UNSUPPORTED_TYPES = {".meta", ".ini", ".bat", ".gp5", ".gp6", ".gp7", ".sys", ".zip", ".rar", ".torrent", ".xcf", ".dll", "", ".dwlt", ".exe", ".tar"};
	public static string[] TEXT_TYPES = {".txt", ".pdf", ".db", ".log", ".json", ".csproj", ".sln", ".unity", ".asset", ".docx"};

	private Texture folderIcon;

	private bool hasDirectory = false;
	private DirectoryInfo currentDir;

	[System.Serializable]
	public struct SaveData {
		public int mode;
		public string dir;
		public string file;
	}
	public SaveData save;
	private string  pathJSON;
	private int lastMode = 0;
	private string lastFile = "";

	private Vector2 gameTypeScroll, fileTypeScroll;

	private Preview preview;

	public 	static Color col_egg = new Color(), col_black = new Color(), col_semiblack = new Color(), col_red = new Color(), col_blank = new Color();

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

	public string GetCurrentPath() {
		return path + "/" + save.file;
	}

	public override void OnInspectorGUI() {
		AnyWalker tar = (AnyWalker)target;
		
		//EditorGUI.ProgressBar(Rect, value:, label);

		//Colors
		ColorUtility.TryParseHtmlString("#eaf0ce", out col_egg);
		ColorUtility.TryParseHtmlString("#333232", out col_black);
		ColorUtility.TryParseHtmlString("#474647", out col_semiblack);
		ColorUtility.TryParseHtmlString("#f45d01", out col_red);
		ColorUtility.TryParseHtmlString("#e1aa7d", out col_blank);

		//Styling
		GUIContent content = new GUIContent();
		GUIStyle oldLabel = EditorStyles.boldLabel;
		GUIStyle style = new GUIStyle(GUI.skin.button), 
		labelstyle = EditorStyles.boldLabel, 
		generateButton = new GUIStyle(GUI.skin.button), 
		elementBtn = new GUIStyle(GUI.skin.button),
		resetButton = new GUIStyle(GUI.skin.button);

		style.imagePosition = ImagePosition.ImageLeft;
		style.fixedHeight = style.fixedWidth = 45;
		elementBtn.fontStyle = FontStyle.Bold;
		resetButton.fontStyle = FontStyle.Bold;
		elementBtn.onActive.textColor = col_egg;
		elementBtn.onNormal.textColor = col_egg;
		elementBtn.active.textColor = col_semiblack;
		generateButton.active.textColor = col_black;
		generateButton.normal.textColor = col_semiblack;
		generateButton.onHover.textColor = col_black;
		generateButton.fontStyle = FontStyle.Bold;
		generateButton.padding = new RectOffset(10, 10, 10, 5);
		labelstyle.normal.textColor = col_egg;
		labelstyle.alignment = TextAnchor.MiddleLeft;
		labelstyle.fontStyle = FontStyle.Bold;
		labelstyle.fontSize = 14;

		//Load Icons
		folderIcon = Resources.Load(TOOL_NAME + "/Folder") as Texture;
		content.image = (Texture2D)folderIcon;
		content.tooltip = "Choose a main directory folder where all files are located.";

		//File Loading & Directory
		GUI.backgroundColor = col_semiblack;
		GUILayout.BeginVertical("Box");
		GUILayout.Label("File Loading", labelstyle);
		GUI.backgroundColor = col_black;
		GUILayout.BeginHorizontal("box");
		GUI.backgroundColor = col_egg;
		if(GUILayout.Button(content, style)) 
		{
			path = EditorUtility.OpenFolderPanel("Select directory", "", "");
			currentDir = new DirectoryInfo(path);
			SaveSettings();
			FileInfo[] info = currentDir.GetFiles("*.");
			foreach(FileInfo f in info) files.Add(f.Name);
		}
		GUI.backgroundColor = Color.white;

		if(path.Length <= 1) {
			buttonName = "Load Directory";
			hasDirectory = false;
		} else {
			buttonName = "Directory loaded:";
			hasDirectory = true;
		}

		GUILayout.BeginVertical();
		GUILayout.Box(buttonName, labelstyle);
		int oldSize = labelstyle.fontSize;
		labelstyle.fontSize = 10;
		if(hasDirectory) GUILayout.Label(Simplify(path), labelstyle);
		else GUILayout.Label("<--", labelstyle);
		labelstyle.fontSize = oldSize;
		GUILayout.EndVertical();
		GUI.backgroundColor = col_egg;
		if(GUILayout.Button("Reset", elementBtn, GUILayout.Width(50))) {
			path = "";
			currentDir = null;
			hasDirectory = false;
		}
		GUILayout.EndHorizontal();
		
		//File Selection
		int labelSize = labelstyle.fontSize;
		if(hasDirectory) {
			GUILayout.BeginVertical("Box");
			FileInfo[] files = GetFileList();
			fileTypeScroll = GUILayout.BeginScrollView(fileTypeScroll, false, true, GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.Height(200));
			for(int i = 0; i < files.Length; i++) {
				if(save.file == files[i].Name) {
					GUI.backgroundColor = col_semiblack;
					labelstyle.normal.textColor = col_egg;
					labelstyle.fontSize = 15;
				}
				else {
					GUI.backgroundColor = col_egg;
					labelstyle.normal.textColor = col_semiblack;
					labelstyle.fontSize = 14;
				}

				if(!hasConversionMethod(files[i].Extension)) GUI.backgroundColor = col_blank;

				GUILayout.BeginHorizontal("box", GUILayout.Width(Screen.width / 1.5f));
				string name = files[i].Name.Split('.')[0];
				if(name.Length > 24) name = name.Substring(0, 24) + "...";
				if(GUILayout.Button(name, labelstyle)) {
					if(hasConversionMethod(files[i].Extension)) {
						save.file = files[i].Name;
						ScanFileType();
					}
				}
				TextAnchor oldAlignment = labelstyle.alignment;
				FontStyle oldStyle = labelstyle.fontStyle;
				labelstyle.fontStyle = FontStyle.Normal;
				labelstyle.fontSize = 10;
				
				if(save.file == files[i].Name) {
					GUI.backgroundColor = col_egg;
					labelstyle.normal.textColor = col_semiblack;
				} else {
					GUI.backgroundColor = col_semiblack;
					labelstyle.normal.textColor = col_egg;
				}
				if(!hasConversionMethod(files[i].Extension)) GUI.backgroundColor = col_red;

				GUILayout.BeginHorizontal("box", GUILayout.Width(40));
				labelstyle.alignment = TextAnchor.MiddleCenter;
				GUILayout.Box(files[i].Extension.ToUpper().Substring(1), labelstyle);
				GUILayout.EndHorizontal();

				labelstyle.fontSize = labelSize;
				labelstyle.alignment = oldAlignment;
				labelstyle.fontStyle = oldStyle;
				GUILayout.EndHorizontal();
			}
			GUILayout.EndScrollView();
			if(save.file != lastFile) SaveSettings();
			GUILayout.EndVertical();
		}
		GUILayout.EndVertical();

		//Reset style vars
		labelstyle.fontSize = labelSize;
		labelstyle.normal.textColor = col_egg;

		//GameType Generation Presets
		if(hasDirectory) {
			GUIStyle gameTypeBtn = elementBtn;
			gameTypeBtn.imagePosition = ImagePosition.ImageAbove;
			gameTypeBtn.fixedHeight = 105;
			gameTypeBtn.fixedWidth = 150;
			gameTypeBtn.padding = new RectOffset(0, 0, 10, 5);

			GUILayout.Space(10);
			GUI.backgroundColor = col_semiblack;
			GUILayout.BeginVertical("Box", GUILayout.Height(gameTypeBtn.fixedHeight + 50));
			GUILayout.Label("Game Type", EditorStyles.boldLabel);
			string[] names = new string[AnyWalker.AMOUNT_GAMEMODES()];
			for(int i = 0; i < names.Length; i++) names[i] = ((AnyWalker.GameType[])System.Enum.GetValues(typeof(AnyWalker.GameType)))[i].ToString().Replace('_', ' ');

			GUI.backgroundColor = col_egg;
			//IMAGES LOADING
			GUIContent[] contents = new GUIContent[AnyWalker.AMOUNT_GAMEMODES()];
			for(int i = 0; i < contents.Length; i++) {
				contents[i] = new GUIContent();
				Texture2D img = Resources.Load(TOOL_NAME + "/" + names[i].Replace(" ", "")) as Texture2D;
				//Complexity
				int complexity = (int)(((i + 1) / (float)contents.Length) * 3);
				int baseX = img.width / 3 * 2, baseY = img.height / 3 * 2;
				Color col = col_egg;
				if(complexity == 2) col = col_blank;
				else if(complexity >= 3) col = col_red;
				Color baseCol = col;
				//Complexity Icon Generation
				for(int c = complexity; c > 0; c--) {
					int si = 50;
					if(c > 1) col *= 0.5f - c/10;
					else col = baseCol;
					for(int mX = 0; mX < si; mX++)
						for(int mY = 0; mY < si; mY++) {
							img.SetPixel(baseX + mX + (c * si / 4), baseY + mY + (c * si / 4), col);
							img.SetPixel(baseX + mX + (si / 8) + (c * si / 4), baseY + (si / 8) + mY + (c * si / 4), col_semiblack);
						}
				}
				img.Apply();
				contents[i].image = img;
				contents[i].text = names[i];
			}
			//Horizontal Scrolling
			Vector2 scroll = new Vector2(0, 0);
			if(Event.current.isScrollWheel) scroll = new Vector2(Event.current.delta.y * 10, 0);
			gameTypeScroll = GUILayout.BeginScrollView(gameTypeScroll + scroll, true, false, GUI.skin.horizontalScrollbar, GUIStyle.none);

			save.mode = GUILayout.SelectionGrid(save.mode, contents, names.Length, gameTypeBtn);
			EditorGUILayout.EndScrollView();

			GUILayout.EndVertical();
			if(save.mode != lastMode) SaveSettings();
			GUILayout.Space(10);

			//Generate button
			GUI.backgroundColor = col_semiblack;
			GUILayout.BeginVertical("Box");
			GUILayout.BeginHorizontal();
			GUILayout.Label("Preview", EditorStyles.boldLabel);
			GUI.backgroundColor = col_egg;
			if(GUILayout.Button("Clear World",  resetButton, GUILayout.Width(85))) tar.Reset();
			GUI.backgroundColor = col_semiblack;
			GUILayout.EndHorizontal();
			GUI.backgroundColor = col_egg;
			if(GUILayout.Button("Generate", generateButton)) {
				if(preview == null && ScanFileType() != null) preview = Preview.Create(new Vector2(800, 620), tar, this);
			}
			GUILayout.EndVertical();
			GUILayout.Space(5);
		}
		labelstyle.normal.textColor = Color.black;
		labelstyle = oldLabel;
	}

	public FileInfo[] GetFileList() {
		List<FileInfo> files = new List<FileInfo>();
		foreach(FileInfo inf in currentDir.GetFiles()) {
			bool addFile = true;
			foreach(string s in UNSUPPORTED_TYPES) if(inf.Extension == s) {
				 addFile = false; 
				 break;
			}
			if(addFile) files.Add(inf);
		}
		return files.ToArray();
	}
	public bool hasConversionMethod(string ext) {
		string extension = ext.Replace('.', ' ').ToLower().Trim();
		try {
			foreach(string s in TEXT_TYPES) if(extension == s.Replace('.', ' ').ToLower().Trim()) return true;
			if(IConvert.FILETYPES[extension] != null) return IConvert.FILETYPES[extension] != null;
		}
		catch(KeyNotFoundException){}
		return false;
	}
	public IConvertType ScanFileType() {
		DirectoryInfo currentFile = new DirectoryInfo(save.dir + "/" + save.file);
		string extension = currentFile.Extension.Replace('.', ' ').ToLower().Trim();
		try {
			foreach(string s in TEXT_TYPES) if(extension == s.Replace('.', ' ').ToLower().Trim()) return IConvert.FILETYPES[TEXT_TYPES[0].Replace('.', ' ').ToLower().Trim()];
			if(IConvert.FILETYPES[extension] != null) return IConvert.FILETYPES[extension];
		}
		catch(KeyNotFoundException){}
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

public class Preview : EditorWindow {
	Editor editor;
	GameObject obj;
	Vector2 size;
	AnyWalker tar;
	AnyWalkerEditor genEdit;

	private IConvertType settings;
	private object[] values;
	private GUIStyle bgCol;

	public static Preview Create(Vector2 size, AnyWalker target, AnyWalkerEditor genEdit) {
		Preview preview = EditorWindow.GetWindowWithRect<Preview>(new Rect(Screen.width / 2 - size.x / 2, (Screen.height / 2) - (size.y + 120) / 2, size.x, size.y + 60));
		preview.size = size;
		preview.tar = target;
		preview.genEdit = genEdit;
		preview.settings = genEdit.ScanFileType();
		preview.settings.SetSettings(AnyWalker.GET_GAMEMODE(preview.genEdit.save.mode));

		//Base Material
		Material mat = Resources.Load(AnyWalkerEditor.TOOL_NAME + "/PreviewCube") as Material;
		mat.color = AnyWalkerEditor.col_egg;
		preview.tar.InitGenerator(mat, AnyWalkerEditor.col_egg, AnyWalkerEditor.col_semiblack);
		
		//Generation settings
		preview.values = new object[preview.settings.variables.Count];
		for(int i = 0; i < preview.values.Length; i++) preview.values[i] = preview.settings.variables[i].value;
		preview.obj = preview.settings.Convert(genEdit.GetCurrentPath(), preview.values, AnyWalker.GET_GAMEMODE(preview.genEdit.save.mode));
		if(preview.obj == null) {
			preview.Close();
			return null;
		}
		return preview;
	}

	void OnGUI() {
		GUIStyle generateButton = new GUIStyle(GUI.skin.button), label = EditorStyles.boldLabel;
		generateButton.active.textColor = AnyWalkerEditor.col_black;
		generateButton.normal.textColor = AnyWalkerEditor.col_semiblack;
		generateButton.onHover.textColor = AnyWalkerEditor.col_black;
		generateButton.fontStyle = FontStyle.Bold;
		generateButton.padding = new RectOffset(10, 10, 10, 5);

		label.normal.textColor = AnyWalkerEditor.col_egg;

		bgCol = new GUIStyle();
		bgCol.normal.background = Texture2D.blackTexture;

		if(obj == null) return;
		if(editor == null) editor = Editor.CreateEditor(obj);
		editor.OnInteractivePreviewGUI(GUILayoutUtility.GetRect(size.x, size.y - 30), bgCol);

		//Loaded File Display
		if(settings.file != null && settings.file.GetType() == typeof(Texture2D)) GUI.DrawTexture(new Rect(size.x - 70, size.y - 100, 60, 60), (Texture2D)settings.file);

		//Settings
		int settingAmount = settings.variables.Count + 1;
		int generateButtonWidth = 150;
		GUI.backgroundColor = AnyWalkerEditor.col_semiblack;
		GUILayout.BeginHorizontal();
		GUILayout.BeginVertical("Box", GUILayout.Width(size.x - (generateButtonWidth + 10)));
		GUILayout.Label("Settings", EditorStyles.boldLabel);
		GUI.backgroundColor = AnyWalkerEditor.col_egg;
		
		GUIStyle setting = EditorStyles.boldLabel;
		int oldFontSize = setting.fontSize;
		float elementX = (size.x - (generateButtonWidth + 10)) / settingAmount, elementY = 30;
		GUILayoutOption elementWidth = GUILayout.Width(elementX), elementHeight = GUILayout.Height(elementY);
		setting.fontSize = 10;
		GUILayout.BeginHorizontal();

		//Individual Settings
		EditorGUI.BeginChangeCheck();
		for(int i = 0; i < settings.variables.Count; i++) {
			GUI.backgroundColor = AnyWalkerEditor.col_black;
			GUILayout.BeginVertical("Box");
			GUI.backgroundColor = AnyWalkerEditor.col_egg;
			GUILayout.Label(settings.variables[i].name, setting, elementWidth);

			if(values[i].GetType() == typeof(float)) {
				values[i] = EditorGUILayout.Slider((float)values[i], (float)settings.variables[i].options[0], (float)settings.variables[i].options[1], elementWidth, GUILayout.Height(elementY/2)) ;
				GUILayout.Space(15);
			}
			else if(values[i].GetType() == typeof(int)) {
				values[i] = EditorGUILayout.IntSlider((int)values[i], (int)settings.variables[i].options[0], (int)settings.variables[i].options[1], GUILayout.Height(elementY/2));
				GUILayout.Space(15);
			}
			else if(values[i].GetType() == typeof(bool)) {
				int val = ((bool)values[i])? 1 : 0;
				val = GUILayout.SelectionGrid(val, new string[]{settings.variables[i].options[0].ToString(), settings.variables[i].options[1].ToString()}, 2);
				values[i] = (val == 0)? true : false;
				GUILayout.Space(12.5f);
			}
			else if(values[i].GetType() == typeof(string)) {
				values[i] = GUILayout.TextField((string)values[i], elementWidth, GUILayout.Height(elementY / 2));
				GUILayout.Space(15);
			}
			else if(values[i].GetType() == typeof(Color)) {
				values[i] = EditorGUILayout.ColorField((Color)values[i], GUILayout.Width(elementX / 1.6f), GUILayout.Height(elementY / 2));
			}
			//Color array
			else if(values[i].GetType() == typeof(AnyWalker.TerrainType)) {
				GUILayout.BeginHorizontal();
				AnyWalker.TerrainType cast = (AnyWalker.TerrainType)values[i];
				int count = (int)settings.variables[i].options[0];
				cast.Synchronize(count);
				for(int v = 0; v < count; v++) {
					GUILayout.BeginVertical();
					cast.colors[v] = EditorGUILayout.ColorField(cast.colors[v], GUILayout.Width(elementX / 2f), GUILayout.Height(elementY / 2));
					cast.heights[v] = EditorGUILayout.FloatField(cast.heights[v], GUILayout.Width(elementX / 2.8f), GUILayout.Height(elementY / 2));
					GUILayout.EndVertical();
				}
				GUILayout.EndHorizontal();
			}
			else if(values[i].GetType() == typeof(FilterMode)) {
				int selected = 0;
				if((FilterMode)values[i] == FilterMode.Point) selected = 0;
				else if((FilterMode)values[i] == FilterMode.Bilinear) selected = 1;
				else selected = 2;
				string[] names = {"Low", "Medium", "High"};
				selected = EditorGUILayout.Popup(selected, names, GUILayout.Width(elementX / 2));
				if(selected == 0) values[i] = FilterMode.Point;
				else if(selected == 1) values[i] = FilterMode.Bilinear;
				else values[i] = FilterMode.Trilinear;

				GUILayout.Space(15);
			}
			GUILayout.EndVertical();
		}
		if(EditorGUI.EndChangeCheck())  UpdateMesh();

		GUILayout.EndHorizontal();
		GUILayout.EndVertical();

		//Generate Button
		GUI.backgroundColor = AnyWalkerEditor.col_semiblack;
		GUILayout.BeginVertical("Box", GUILayout.Width(generateButtonWidth));
		GUILayout.Label("Finalise", EditorStyles.boldLabel);
		GUI.backgroundColor = AnyWalkerEditor.col_egg;
		GUILayout.Space(32);
		if(GUILayout.Button("Generate", generateButton)) {
			 tar.Generate(obj, settings.size);
			this.Close();
		}
		GUILayout.EndVertical();
		GUILayout.EndHorizontal();

		setting.fontSize = oldFontSize;
		label.normal.textColor = Color.black;
	}

	private void UpdateMesh() {
		foreach(Transform child in obj.transform) DestroyImmediate(child.gameObject);
		obj = settings.Convert(genEdit.GetCurrentPath(), values, AnyWalker.GET_GAMEMODE(genEdit.save.mode));
		Editor temp = Editor.CreateEditor(obj);
		DestroyImmediate(editor);
		editor = temp;
	}

	void OnDestroy() {
		DestroyImmediate(obj);
		obj = null;
	}
}
