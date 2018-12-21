using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class AnyWalker : MonoBehaviour {
	public Texture load;

	[System.Serializable]
	public enum GameType {
		Landscape, Runner, Layered
	};

	protected Material cubeMaterial;
	public GameObject parent;

	public static Color col_egg, col_semiblack;

	private Color baseCol;

	public static AnyWalker Self;
	private GameObject world;

	public static MonoBehaviour coroutineHandler;

	public const string levelName = "Level";

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
		baseCol = new Color(Mathf.Clamp(baseCol.r, 0.3f, 0.8f), Mathf.Clamp(baseCol.g, 0.3f, 0.8f), Mathf.Clamp(baseCol.b, 0.3f, 0.8f));
		if(coroutineHandler == null) coroutineHandler = this;
	}

	public static GameObject CreateCube(Vector3 pos) {
		return CreateCube(pos, Vector3.one);
	}

	public static GameObject CreateCube(Vector3 pos, Vector3 offsetScale) {
		GameObject obj;
		obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
		MeshRenderer mesh = obj.GetComponent<MeshRenderer>();
		obj.name = "Part";
		
		Material mat = new Material(Self.cubeMaterial);
		float i = Random.Range(0.1f, 0.2f);
		mat.color = Self.baseCol + new Color(i, i, i);
		mesh.sharedMaterial = mat;

		obj.transform.SetParent(Self.parent.transform);
		obj.transform.localPosition = new Vector3(pos.x *  obj.transform.localScale.x * offsetScale.x, pos.y * offsetScale.y *  obj.transform.localScale.y, pos.z * offsetScale.z *  obj.transform.localScale.z);
		obj.hideFlags = HideFlags.HideAndDontSave;
		mesh.hideFlags = HideFlags.HideAndDontSave;
		return obj;
	}

	public static void BindMesh(Mesh meshObj) {
		GameObject level = new GameObject("Part");
		level.AddComponent<MeshFilter>();
		level.GetComponent<MeshFilter>().mesh = meshObj;
		level.AddComponent<MeshRenderer>();
		MeshRenderer mesh = level.GetComponent<MeshRenderer>();
		//Material mat = new Material(Self.cubeMaterial);
		Material mat = new Material(Shader.Find("Diffuse"));
		float i = Random.Range(0.1f, 0.2f);
		mat.color = Self.baseCol + new Color(i, i, i);
		mesh.sharedMaterial = mat;
		level.hideFlags = HideFlags.HideAndDontSave;
		mesh.hideFlags = HideFlags.HideAndDontSave;
		level.transform.SetParent(Self.parent.transform);
		level.transform.localRotation = Quaternion.Euler(-90, 0, 0);
	}

	public static int AMOUNT_GAMEMODES() {
		return System.Enum.GetNames(typeof(GameType)).Length;
	}

	public static GameType GET_GAMEMODE(int i) {
		return (GameType)(System.Enum.GetValues(typeof(GameType))).GetValue(i);
	}
}
