using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSpawner : MonoBehaviour {
	[System.Serializable]
	public class Pool {
		public string tag;
		public GameObject prefab;
		public int size;
	}

	public static ParticleSpawner Instance;

	void Awake() {
		Instance = this;
	}

	public List<Pool> pools;
	public Dictionary<string, Queue<GameObject>> poolDictionary;

	void Start () {
		poolDictionary = new Dictionary<string, Queue<GameObject>>();

		GameObject baseLoc = new GameObject("Particles");
		baseLoc.transform.SetParent(GameObject.Find("Map").transform);

		foreach(Pool pool in pools) {
			Queue<GameObject> objectPool = new Queue<GameObject>();

			for(int i = 0; i < pool.size; i++) {
				GameObject obj = Instantiate(pool.prefab);
				obj.name = "Particle";
				obj.transform.SetParent(baseLoc.transform); 
				obj.SetActive(false);
				objectPool.Enqueue(obj);
			}

			poolDictionary.Add(pool.tag, objectPool);
		}
	}

	public static GameObject SPAWN(string tag, Vector3 pos, Quaternion rot, Vector3 scale) {
		if(!Instance.poolDictionary.ContainsKey(tag)) return null;
		GameObject spwn = Instance.poolDictionary[tag].Dequeue();
		spwn.SetActive(true);
		spwn.transform.position = pos;
		spwn.transform.rotation = rot;
		spwn.transform.localScale = scale;
		Instance.poolDictionary[tag].Enqueue(spwn);
		return spwn;
	}

	public static GameObject SPAWN(Color color, Vector3 pos, Quaternion rot, Vector3 scale) {
		GameObject obj = SPAWN("Sand Particle", pos, rot, scale);
		obj.GetComponent<MeshRenderer>().material.color = color;
		return obj;
	}

	public static GameObject SPAWN(string tag, Vector3 pos, Quaternion rot, float scale) {
		return SPAWN(tag, pos, rot, new Vector3(scale, scale, scale));
	}

	public static GameObject SPAWN(Color color, Vector3 pos, Quaternion rot, float scale) {
		return SPAWN(color, pos, rot, new Vector3(scale, scale, scale));
	}
}
