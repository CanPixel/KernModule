using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sprout : MonoBehaviour {
	private Vector3[] verts;
	private int[] tris;

	private Mesh oldMesh;

	void Awake () {
		oldMesh = transform.GetComponent<MeshFilter>().mesh;

		generateBranch(transform, 8, 10, 1);

		MeshFilter[] meshFilters = transform.GetComponentsInChildren<MeshFilter>();
		CombineInstance[] combine = new CombineInstance[meshFilters.Length];
		for(int i = 0; i < meshFilters.Length; i++) {
			combine[i].mesh = meshFilters[i].mesh;
			combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
			if(i > 0) Destroy(meshFilters[i].gameObject);
		}

		transform.GetComponent<MeshFilter>().mesh = new Mesh();
		transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);

		gameObject.AddComponent<MeshCollider>();
	}

	private float height = 1f;
	private void generateBranch(Transform lastOBJ, int limit, float angle, int dir) {
		Transform firstOBJ = lastOBJ;
		for(int i = 2; i < limit+1; i++) {
			if(transform.lossyScale.x-(i*0.05f) < 0.45f) return;
			GameObject fr = GameObject.CreatePrimitive(PrimitiveType.Cube);
			fr.GetComponent<MeshFilter>().mesh = oldMesh;
			fr.transform.SetParent(firstOBJ);
			fr.transform.localPosition = new Vector3(0, height*i-(i*0.8f), 0);
			fr.transform.localScale = new Vector3(transform.localScale.x-(i*0.05f), transform.localScale.y-(i*0.05f), transform.localScale.z-(i*0.05f));
			Vector2 rot = new Vector2(Random.Range(0, 2) == 0? angle : 0, Random.Range(0, 2) == 0? angle : 0);
			fr.transform.localRotation = Quaternion.Euler(rot.x*dir, 0, rot.y*dir);
			firstOBJ = fr.transform;

			if(Random.Range(0, 2) == 0 && dir > 0) generateBranch(firstOBJ, 13, 20, -1);
		}
	}
}