using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Reflection;

public class AnyWalker : MonoBehaviour {
	[System.Serializable]
	public enum GameType {
		Landscape, Runner, Layered
	};

	public class TerrainType {
		public List<float> heights = new List<float>();
		public List<Color> colors = new List<Color>();

		public void Synchronize(int count) {
			if(heights.Count != count || colors.Count != count) {
				heights.Clear();
				colors.Clear();
				for(int i = count; i > 0; i--) {
					heights.Add(1f / (i+1));
					colors.Add(new Color(1f / i, 1f / i, 1f / i));
				}
				heights[heights.Count-1] = 1f;
			}
		}
	}
	protected Material cubeMaterial;
	public GameObject parent;

	public const string levelName = "Level";
	public static AnyWalker Self;
	public static Color col_egg, col_semiblack;

	private Color baseCol, secondaryCol;
	private GameObject world;

	public void Generate(GameObject obj, Vector3 size) {
		Reset();
		world = Instantiate(obj);
		world.name = levelName;
		world.transform.SetParent(transform);
		world.transform.localRotation = Quaternion.Euler(90, 0, 0);
		world.transform.localPosition = new Vector3(-size.x / 2, -1, -size.y / 2);
	}

	public void Reset() {
		if(world == null && transform.Find(levelName) != null) world = transform.Find(levelName).gameObject;
		DestroyImmediate(world);
	}

	public void InitGenerator(Material material, Color egg, Color semiblack) {
		Self = this;
		col_egg = egg;
		col_semiblack = semiblack;
		cubeMaterial = material;
		parent = new GameObject("Preview Model");
		parent.hideFlags = HideFlags.HideAndDontSave;
		baseCol = Random.ColorHSV();
		secondaryCol = Random.ColorHSV();
		baseCol = new Color(Mathf.Clamp(baseCol.r, 0.3f, 0.8f), Mathf.Clamp(baseCol.g, 0.3f, 0.8f), Mathf.Clamp(baseCol.b, 0.3f, 0.8f));
		secondaryCol = new Color(Mathf.Clamp(secondaryCol.r, 0.3f, 0.8f), Mathf.Clamp(secondaryCol.g, 0.3f, 0.8f), Mathf.Clamp(secondaryCol.b, 0.3f, 0.8f));
	}

	public static GameObject CreateCube(Vector3 pos, int col = 0) {
		return CreateCube(pos, Vector3.one, col);
	}
	public static GameObject CreateCube(Vector3 pos, Vector3 offsetScale, int col = 0) {
		GameObject obj;
		obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
		MeshRenderer mesh = obj.GetComponent<MeshRenderer>();
		obj.name = "Part";

		Material mat = new Material(Self.cubeMaterial);
		float i = Random.Range(0.1f, 0.2f);
		if(col != 0) mat.color = Self.secondaryCol + new Color(i, i, i);
		else mat.color = Self.baseCol + new Color(i, i, i);
		mesh.sharedMaterial = mat;

		obj.transform.SetParent(Self.parent.transform);
		obj.transform.localPosition = new Vector3(pos.x *  obj.transform.localScale.x * offsetScale.x, pos.y * offsetScale.y *  obj.transform.localScale.y, pos.z * offsetScale.z *  obj.transform.localScale.z);
		mesh.hideFlags = HideFlags.HideAndDontSave;
		return obj;
	}

	public static void BindMesh(Mesh meshObj, Texture2D tex, int spacing) {
		GameObject level = new GameObject("Part");
		level.AddComponent<MeshFilter>();
		level.GetComponent<MeshFilter>().mesh = meshObj;
		MeshRenderer mesh = level.AddComponent<MeshRenderer>();
		Material mat = new Material(Shader.Find("Unlit/Texture"));
		mat.color = Color.white;
		mesh.sharedMaterial = mat;
		level.hideFlags = HideFlags.HideAndDontSave;
		mesh.hideFlags = HideFlags.HideAndDontSave;
		level.transform.SetParent(Self.parent.transform);
		level.transform.localRotation = Quaternion.Euler(-90, 0, 0);
		level.transform.localPosition = Vector3.zero;
		level.transform.position = new Vector3(-(tex.width*spacing) / 2, -(tex.width*spacing) / 2, 0);
		mesh.sharedMaterial.mainTexture = tex;
		mesh.sharedMaterial.mainTextureScale = new Vector2(tex.width / 4000f, tex.height / 4000f);
		MeshCollider col = level.AddComponent<MeshCollider>();
	}

	public static void ClearConsole() {
		var assembly = Assembly.GetAssembly(typeof(SceneView));
		var type = assembly.GetType("UnityEditor.LogEntries");
		var method = type.GetMethod("Clear");
		method.Invoke(new object(), null);
	}

	public static int AMOUNT_GAMEMODES() {
		return System.Enum.GetNames(typeof(GameType)).Length;
	}

	public static GameType GET_GAMEMODE(int i) {
		return (GameType)(System.Enum.GetValues(typeof(GameType))).GetValue(i);
	}
}
