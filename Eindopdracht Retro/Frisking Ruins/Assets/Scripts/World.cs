using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralToolkit.Examples;

public class World : MonoBehaviour {
	[SerializeField]
	private GameObject prefabTile, prefabCube, sun, prefabSprout, vignette, player;
	private float baseVignetteAlpha;

	private MeshRenderer terrainRenderer;
	private Color terrainBaseCol;

	public static World instance;

	[Header("World")]
	[SerializeField]
    private int width;
	[SerializeField]
	private int height;
	private const int MAP_SAMPLE_SIZE = 300;
	[SerializeField]
	private float treeDensity = 80;

	[SerializeField]
	private float scale;

	private float blockScale = 0.6f;
	private bool[,] map;

	[SerializeField]
	private float chanceToStartAlive;
	[SerializeField]
	private int steps = 5;

	private Vector3 startRot;

	[SerializeField]
	[Header("Dungeons")]
	private Vector2 dungeonSize;
	[SerializeField]
	private GameObject prefabTileOfLife;
	private List<GameObject> dungeons = new List<GameObject>();

	[Header("Tile Materials")]
	public Material[] materials;

	private List<GameObject> cullingObjects = new List<GameObject>();

	[Header("All Enemies")]
	public GameObject[] entities;

	public class Tile {
		public static Tile Floor = new Tile(0, "Floor");
		public static Tile Wall = new Tile(1, "Wall");

		private int ID;

		public Tile(int ID, string name) {
			this.ID = ID;
			this.tileName = name;
		}

		public string tileName {
			get;
			protected set;
		}
		public Sprite texture {
			get;
			protected set;
		}

		public int getID() {
			return ID;
		}

		public void assignSolidity(GameObject instance) {
			if(tileName == "Floor") Destroy(instance.GetComponent<Collider>());
		}

		public static Material findMaterial(GameObject instance, string name) {
			for(int i = 0; i < instance.transform.parent.parent.GetComponent<World>().materials.Length; i++) {
				Material curMat = instance.transform.parent.parent.GetComponent<World>().materials[i];
				if(curMat.name == name) return curMat;
			}
			return null;
		}
	}

    void Awake() {
		map = new bool[width, height];

		GenerateMap(map);

		vignette = GameObject.Find("Canvas/Vignette").gameObject;
		baseVignetteAlpha = vignette.GetComponent<UnityEngine.UI.Image>().color.a;
		vignette.SetActive(false);

		transform.Find("Floor").localScale = new Vector3(width, height);
		terrainRenderer = transform.Find("Terrain").GetComponent<MeshRenderer>();
		terrainBaseCol = terrainRenderer.material.color;
		startRot = sun.transform.right;

		instance = this;
	}

	void FixedUpdate() {
		//Day/night cycle
		float day = Mathf.Sin(Time.time / 10) * 40 + 92;
		sun.transform.rotation = Quaternion.AngleAxis(day, startRot);
	
		float dayCol = Mathf.Clamp(day / 70 - 0.3f, 0, 1);
		terrainRenderer.material.color = new Color(terrainRenderer.material.color.r * dayCol, terrainRenderer.material.color.g * dayCol, terrainRenderer.material.color.b * dayCol);

		//Only render on-screen objects
		if(Time.time % 1.5f == 0) CalcFrustumCulling();

		//Visual transition entering dungeon / Vignette Fade
		float colorFact = 0.3f, fadeSpeed = 3f;
		bool dungeonActive = IsPlayerInDungeon();
		if(dungeonActive) terrainRenderer.material.color = new Color(Mathf.Lerp(terrainRenderer.material.color.r, terrainBaseCol.r - colorFact, Time.deltaTime * fadeSpeed), Mathf.Lerp(terrainRenderer.material.color.g, terrainBaseCol.g - colorFact, Time.deltaTime * fadeSpeed), Mathf.Lerp(terrainRenderer.material.color.b, terrainBaseCol.b - colorFact, Time.deltaTime * fadeSpeed));
		else terrainRenderer.material.color = new Color(Mathf.Lerp(terrainRenderer.material.color.r, terrainBaseCol.r, Time.deltaTime * fadeSpeed), Mathf.Lerp(terrainRenderer.material.color.g, terrainBaseCol.g, Time.deltaTime * fadeSpeed), Mathf.Lerp(terrainRenderer.material.color.b, terrainBaseCol.b, Time.deltaTime * fadeSpeed));
		
		vignette.SetActive(dungeonActive);
		vignette.GetComponent<UnityEngine.UI.Image>().color = new Color(0, 0, 0, Mathf.Lerp(vignette.GetComponent<UnityEngine.UI.Image>().color.a, (vignette.activeSelf)? baseVignetteAlpha : 0, Time.deltaTime * fadeSpeed));
	
		//Entity Gen
		if(Random.Range(0, 20) < 5 && Time.time % 2 == 0) {
			Vector3 randPos = Camera.main.ViewportToWorldPoint(new Vector3((Random.Range(0, 2) == 0)?-0.3f:1.3f, (Random.Range(0, 2) == 0)?-0.3f:1.3f, 20));
			SpawnEntity(randPos);
		}
	}

	protected void SpawnEntity(Vector3 pos) {
		GameObject ent = Instantiate(entities[Random.Range(0, entities.Length)], pos, Quaternion.Euler(30, 180, 0), transform);
		ent.GetComponent<Retro.Entity>().target = player;
	}

	protected bool IsPlayerInDungeon() {
		foreach(GameObject dun in dungeons) if(dun.GetComponent<Dungeon>() != null && dun.GetComponent<Dungeon>().IsInDungeon()) return true;
		return false;
	}

	private void CalcFrustumCulling() {
		Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
		planes[0].distance += 16;
		planes[1].distance += 16;
		planes[2].distance += 16;
		planes[3].distance += 16;
		foreach(GameObject obj in cullingObjects) {
			if(obj == null || obj.GetComponent<Collider>() == null) continue;
			if(GeometryUtility.TestPlanesAABB(planes, obj.GetComponent<Collider>().bounds) && player.GetComponent<Player>() != null && !player.GetComponent<Player>().InDungeon()) obj.SetActive(true);
			else obj.SetActive(false);
		}
	}

	private bool[,] InitMap(bool[,] map, int w, int h) {
		for(int x = 0; x < w; x++)
			for(int y = 0; y < h; y++) {
				if(Random.Range(0f, 1f) < chanceToStartAlive) map[x, y] = true;
			}
			return map;
	}
	private bool[,] GenerateMap(bool[,] map) {
		GameObject cacti = new GameObject("Cacti");
		cacti.transform.SetParent(transform);

		//Planes
		int sizeNeeded = (width / MAP_SAMPLE_SIZE)-1;
		GameObject terrain = transform.Find("Terrain").gameObject;
		for(int val = 0; val < sizeNeeded; val++) 
			for(int i = -1; i < 2; i++) 
				for(int j = -1; j < 2; j++) {
					if(i == 0 && j == 0) continue;
					GameObject newTerra = Instantiate(terrain, new Vector3(MAP_SAMPLE_SIZE*val+(i*MAP_SAMPLE_SIZE), terrain.transform.position.y, MAP_SAMPLE_SIZE*val+(j*MAP_SAMPLE_SIZE)), Quaternion.identity);
					newTerra.transform.SetParent(transform);
					newTerra.name = "Terrain " + val;
				}

		//Borders
		for(int l = 0; l < 2; l++) for(int x = 0; x < width; x++) for(int m = 0; m < 2; m++) GenCube(cacti, x, l*height+(m*2-1), true);
		for(int l = 0; l < 2; l++) for(int x = 0; x < height; x++) for(int m = 0; m < 2; m++) GenCube(cacti, l*width+(m*2-1), x, true);
		int totalW = width*(sizeNeeded+1);
		int totalH = height*(sizeNeeded+1);

		bool[,] cellmap = new bool[totalW, totalH];
		cellmap = InitMap(cellmap, totalW, totalH);
		for(int i = 0; i < steps; i++) cellmap = DoStep(cellmap);

		//Cactus
		int startX = 0, startY = 0;
		for(int x = 0; x < totalW; x++)
			for(int y = 0; y < totalH; y++)
			{
				try {
					if(cellmap[x, y]) GenCube(cacti, x+startX, y+startY);
				}
				catch(System.IndexOutOfRangeException) {}
			}
		transform.Find("Floor").transform.localScale *= scale;

		//Dungeons
		GenDungeon(0, 10, Dungeon.Type.GAME_OF_LIFE);
		GenDungeon(-80, 10, Dungeon.Type.TETRIS);

		//Sprout
		GameObject sprouts = new GameObject("Sprouts");
		sprouts.transform.SetParent(transform);
		for(int i = 0; i < treeDensity; i++) {
			int randX = Random.Range(0, width)-width/2;
			int randY = Random.Range(0, height)-height/2;
			GenSprout(randX, randY, sprouts.transform);
		}

		return cellmap;
	}

	private bool[,] DoStep(bool[,] oldMap) {
		bool[,] newMap = new bool[width, height];

		int deathLimit = 5;
		int birthLimit = 5;

		for(int x = 0; x < width; x++)
			for(int y = 0; y < height; y++)
			{
				int nbs = CountAliveNeighbours(oldMap, x, y);
				if(!oldMap[x, y]) {
					if(nbs < deathLimit) newMap[x, y] = false;
					else newMap[x, y] = true;
				}
				else {
					if(nbs > birthLimit) newMap[x, y] = true;
					else newMap[x, y] = false;
				}
			}
			return newMap;
	}
	private int CountAliveNeighbours(bool[,] map, int x, int y) {
		int count = 0;
		for(int i = -1; i < 2; i++) {
			for(int j = -1; j < 2; j++) {
				int neighbour_x = x + i;
				int neighbour_y = y + j;
				if(i == 0 && j == 0) {}
				else if(neighbour_x < 0 || neighbour_y < 0 || neighbour_x >= width || neighbour_y >= height) count++;
				else if(map[neighbour_x, neighbour_y]) count++;
			}
		}
		return count;
	}

	private void GenCube(GameObject parent, float x, float y) {GenCube(parent, x, y, false);}
	private void GenCube(GameObject parent, float x, float y, bool immortal) {
		float gray = Random.Range(0.2f, 1f);
		float heightRange = Random.Range(0.4f, 0.8f);
		GameObject cub = Instantiate(prefabCube, new Vector3(x * scale, 0, y * scale) * scale - new Vector3(width*scale / 2, 4, height*scale / 2)*scale, Quaternion.Euler(90, 0, 0), transform);

		cub.transform.localScale = new Vector3(Random.Range(0.3f, 1.1f) * blockScale, heightRange * blockScale / gray, (cub.transform.localScale.z * Random.Range(0.1f, 1.1f)) + 2);
		cub.name = "Cactus";
		cub.transform.SetParent(parent.transform);

		Color finalCol = new Color(gray*1.3f, gray*1.45f, gray*1.1f);
		cub.GetComponent<MeshRenderer>().material.color = finalCol;

		if(immortal) {
			Destroy(cub.GetComponent<Retro.Entity>());
			Destroy(cub.GetComponent<Rigidbody>());
		}

		cullingObjects.Add(cub);
	}
	private void GenSprout(int x, int y, Transform par) {
		GameObject sprout = Instantiate(prefabSprout, new Vector3(x, 0, y)/5, Quaternion.identity);
		sprout.transform.SetParent(par);
		sprout.name = "Sprout";
		float offs = 90;
		sprout.transform.localRotation = Quaternion.Euler(0, Random.Range(-offs, offs), 0);
		float scale = Random.Range(1.6f, 3.1f);
		sprout.transform.localScale = new Vector3(scale, scale, scale);
		sprout.transform.localPosition = new Vector3(sprout.transform.localPosition.x, -3, sprout.transform.localPosition.z);
		cullingObjects.Add(sprout);
	}

	private void GenDungeon(int x, int y, Dungeon.Type type) {
		float W = dungeonSize.x, H = dungeonSize.y;
		GameObject dungeon = new GameObject("Dungeon");
		dungeon.transform.SetParent(transform);

		//Dungeon Init
		dungeon.AddComponent<Dungeon>();
		dungeon.GetComponent<Dungeon>().Generate(prefabTile, new Vector2(W, H));
		dungeon.tag = "Dungeon";
		
		//Assign Dungeon Triggers
		BoxCollider coll = dungeon.AddComponent<BoxCollider>();
		coll.isTrigger = true;
		coll.center = new Vector3(W/2, 0, H/2);
		coll.size = new Vector3(W-2, 16, H-2);
		Rigidbody rb = dungeon.AddComponent<Rigidbody>();
		rb.useGravity = false;
		rb.isKinematic = true;
		dungeon.GetComponent<Dungeon>().Init(this, prefabTileOfLife, dungeon.transform, type);
		dungeon.transform.localPosition = new Vector3(x, 0, y);
		dungeons.Add(dungeon);
	}

	public Vector2 getWorldSize() {
		return new Vector2(width, height);
	}

	public List<GameObject> GetDungeons() {
		return dungeons;
	}

	public class Dungeon : MonoBehaviour {
		public GameObject[,] tiles;
		private List<GameObject> doors = new List<GameObject>();  
		private GameObject prefabTile;
		private World world;
		
		public bool[,] alive, nextState, landed;
		private int W, H;

		public float dungeonWinTime = 24;

		private float gridDelay = 0, dungeonTimer = 0, spawnDelay = 0, gliderDelay = 0;
		private float dungeonSpeed = 0.05f;
		private bool active = false, cleared = false;

		private Type type;
		public enum Type {
			GAME_OF_LIFE, TETRIS
		}

		public abstract class EntityOfLife {
			public int[,] shape;
			public int rarity;
			public string name;
			public Vector2 size;

			public static EntityOfLife[] ENTITIES_OF_LIFE = {new Glider(), new Blinker(), new Flower()};
			public static EntityOfLife[] ENTITIES_TETRIS = {new TetrisL(), new TetrisL2(), new TetrisL3(), new TetrisL4(), new Block(), new Blinker()};

			public EntityOfLife(int[,] shape, string name, int rarity, Vector2 size) {
				this.shape = shape;
				this.name = name;
				this.rarity = rarity;
				this.size = size;
			}

			public int[,] GetFlippedShape() {
				int[,] flip = new int[(int)size.y, (int)size.x];
				for(int i = 0; i < size.x; i++)
				for(int j = 0; j < size.y; j++) {
					flip[j, i] = shape[i, j];
				}
				return flip;
			}

			public class TetrisL : EntityOfLife {
				public TetrisL() : base(new int[,]{{1, 1, 1}, {0, 0, 1}, {0, 0, 1}}, "L-Shape", 20, new Vector2(3, 3)) {}
			}
			public class TetrisL2 : EntityOfLife {
				public TetrisL2() : base(new int[,]{{0, 0, 1}, {0, 0, 1}, {1, 1, 1}}, "L-Shape", 20, new Vector2(3, 3)) {}
			}
			public class TetrisL3 : EntityOfLife {
				public TetrisL3() : base(new int[,]{{1, 0, 0}, {1, 0, 0}, {1, 1, 0}}, "L-Shape", 20, new Vector2(3, 3)) {}
			}
			public class TetrisL4 : EntityOfLife {
				public TetrisL4() : base(new int[,]{{0, 1, 1}, {0, 0, 1}, {0, 0, 1}}, "L-Shape", 20, new Vector2(3, 3)) {}
			}
			public class Block : EntityOfLife {
				public Block() : base(new int[,]{{1, 1}, {1, 1}}, "Block", 20, new Vector2(2, 2)) {}
			}

			public class Glider : EntityOfLife {
				public Glider() : base(new int[,]{{0, 1, 0}, {0, 0, 1}, {1, 1, 1}}, "Glider", 20, new Vector2(3, 3)) {}
			}
			public class Blinker : EntityOfLife {
				public Blinker() : base(new int[,]{{1, 1, 1}}, "Blinker", 50, new Vector2(3, 1)) {}
			}
			public class Flower : EntityOfLife {
				public Flower() : base(new int[,]{{0, 0, 1, 0, 0},
												{0, 1, 0, 1, 0},
												{1, 0, 0, 0, 1},
												{1, 0, 0, 0, 1},
												{1, 0, 0, 0, 1},
												{0, 1, 0, 1, 0},
												{0, 0, 1, 0, 0}}, "Flower", 40, new Vector2(5, 7)) {}
			}

		}

		public class TileOfLife : MonoBehaviour {
			void OnCollisionEnter(Collision col) {
				if(col.gameObject.tag == "Player") col.gameObject.GetComponent<Player>().Damage(10, 2, transform);
			}
		}

		public void Generate(GameObject prefabTile, Vector2 size) {
			this.prefabTile = prefabTile;
			W = (int)size.x;
			H = (int)size.y;
			for(int i = 0; i < W; i++)
				for(int j = 0; j < H; j++) {
					GameObject tile = Instantiate(prefabTile, new Vector3(i, -2.2f, j), Quaternion.Euler(90, 0, 0));
					tile.transform.SetParent(transform);

					Tile spawnedTile = Tile.Floor;
					if(i == 0 || i == W-1 || j == 0 || j == H-1) spawnedTile = Tile.Wall;
					
					//Entrances
					for(int l = -3; l < 3; l++) for(int m = 0; m < W; m += W-1) if(i == m && j == H/2+l) spawnedTile = Tile.Floor;
					for(int l = -3; l < 3; l++) for(int m = 0; m < H; m += H-1) if(i == W/2+l && j == m) spawnedTile = Tile.Floor;

					InitTile(spawnedTile, tile);
				}
		}
		
		private void InitTile(Tile tileInfo, GameObject tile) {
			tile.name = tileInfo.tileName;
			tile.GetComponent<MeshRenderer>().material = Tile.findMaterial(tile, tile.name);
			tileInfo.assignSolidity(tile);
		}

		public void Init(World world, GameObject prefab, Transform parent, Type type) {
			this.world = world;
			this.type = type;
			Vector2 pos = new Vector2(parent.position.x, parent.position.z);
			tiles = new GameObject[W, H];
			alive = new bool[W, H];
			landed = new bool[W, H];
			nextState = new bool[W, H];
			for(int i = 0; i < W; i++) 
				for(int j = 0; j < H; j++){
					alive[i, j] = false;
					nextState[i, j] = false;
					tiles[i, j] = Instantiate(prefab);
					tiles[i, j].name = "Tile of Life";
					tiles[i, j].AddComponent<TileOfLife>();
					tiles[i, j].transform.SetParent(parent);
					tiles[i, j].transform.localPosition = new Vector3(i, 0, j);
					setTileState(i, j, false);
				}
		}

		public void ActivateDungeon() {
			if(!active) {
				for(int x = -1; x <= W; x++)
					for(int y = -1; y <= H; y++) {
						if(x > 0 && y > 0 && x < W-2 && y < H-2) continue;
						GameObject door = Instantiate(prefabTile, new Vector3(x + transform.position.x, 1, y + transform.position.z), Quaternion.Euler(90, transform.eulerAngles.y, transform.eulerAngles.z));
						door.transform.SetParent(transform);
						door.name = "Dungeon Door";
						doors.Add(door);
					}
				SoundManager.PLAY_DUNGEON_THEME();
			}			
			switch(type) {
				case Type.GAME_OF_LIFE:
					gameOfLifeTick();
					break;
				case Type.TETRIS:
					tetrisTick();
					break;
			}

			//Entities
			if(Random.Range(0, 15) < 5 && Time.time % 2 == 0) {
				Vector3 randPos = new Vector3(Random.Range(0, W), 2, Random.Range(0, H));
				world.SpawnEntity(randPos);
			}

			active = true;
		}
		public void DeactivateDungeon() {
			for(int i = 0; i < W; i++) for(int j = 0; j < H; j++) setTileState(i, j, false);
			active = false;
		}

		IEnumerator Clear() {
			active = false;
			for(int i = 0; i < W; i++) for(int j = 0; j < H; j++) Destroy(tiles[i,j]);
			foreach(GameObject obj in doors) {
				Destroy(obj);
				yield return new WaitForSeconds(0.02f);
			}
		}

		private void tetrisTick() {
			if(cleared || !active) return;
			if(dungeonTimer < dungeonWinTime) {
				//Tiles
				if(Random.Range(0, 100) < 10) {
					int x = Random.Range(0, W);
					EntityOfLife life = EntityOfLife.ENTITIES_TETRIS[Random.Range(0, EntityOfLife.ENTITIES_TETRIS.Length)];
					if(Random.Range(0, 100) > life.rarity) PlaceEntity(x, H - 3, life);
				}
			}

			//Killing
			if(dungeonTimer > dungeonWinTime) {
				for(int i = 0; i < 20; i++) {
					int randX = Random.Range(0, W);
					int randY = Random.Range(0, H);
					alive[randX, randY] = false;
				}
				if(dungeonTimer > dungeonWinTime + 5 && !cleared) {
					cleared = true;
					StartCoroutine(Clear());
				}
			}
		}

		private void gameOfLifeTick() {
			if(cleared || !active) return;
			if(dungeonTimer < dungeonWinTime) {
				//Corners
				if(Random.Range(0, 100) < 5) {
					for(int i = 0; i < 12; i++) {
						EntityOfLife life = EntityOfLife.ENTITIES_OF_LIFE[Random.Range(0, EntityOfLife.ENTITIES_OF_LIFE.Length)];
						if(Random.Range(0, 100) > life.rarity) PlaceEntity((W/2)*((i%2)+1) - (W/4), (H/2)*((i%2)+1) - (H/4), life);
					}
				}
				//Center
				if(spawnDelay <= 0) {
					for(int i = 0; i < 3; i++) { 
						EntityOfLife life = EntityOfLife.ENTITIES_OF_LIFE[Random.Range(0, EntityOfLife.ENTITIES_OF_LIFE.Length)];
						 if(Random.Range(0, 100) > life.rarity) PlaceEntity((W/2)*((i%2)+1), H/2, life);
						else 
						{
							i--;
							continue;
						}
					}
					spawnDelay = 1;
				}
				//Gliders
				if(Random.Range(0, 100) < 10 && (int)Time.time % 2 == 0 && gliderDelay <= 0){
					 for(int i = 0; i < 2; i++) PlaceEntity((W/2)*((i%2)+1) - (W/4), (H/2)*((i%2)+1) - (H/4), EntityOfLife.ENTITIES_OF_LIFE[0], (i == 1));
					gliderDelay = 1;
				}
			}

			//Killing
			if(dungeonTimer > dungeonWinTime) {
				for(int i = 0; i < 20; i++) {
					int randX = Random.Range(0, W);
					int randY = Random.Range(0, H);
					alive[randX, randY] = false;
				}
				if(dungeonTimer > dungeonWinTime + 5 && !cleared) {
					cleared = true;
					StartCoroutine(Clear());
				}
			}
		}

		public bool IsActive() {
			return active;
		}

		private void PlaceEntity(int x, int y, EntityOfLife ent) {
			PlaceEntity(x, y, ent, false);
		}
		private void PlaceEntity(int x, int y, EntityOfLife ent, bool flip) {
			int[,] entit = ent.shape;
			if(flip) entit = ent.GetFlippedShape();
			try {
				for(int i = 0; i < ent.size.x; i++) for(int j = 0; j < ent.size.y; j++) if(entit[i, j] == 1) alive[x+i, y+j] = true;
			}
			catch(System.IndexOutOfRangeException){}
		}

		void OnTriggerStay(Collider col) {
			if(col.tag == "Cacti" || col.tag == "Sprout") Destroy(col.gameObject);
			if(col.tag == "Player" && dungeonTimer > 0 && !cleared) {
				dungeonTimer += Time.deltaTime;
				if(dungeonTimer > 2) ActivateDungeon();
				Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, 25, Time.deltaTime * 4);
				Camera.main.transform.rotation = Quaternion.Euler(Camera.main.transform.eulerAngles.x, Camera.main.transform.eulerAngles.y, Mathf.Sin(Time.time*4.5f));
				col.GetComponent<Player>().setInDungeon(true);
			}
		}
		void OnTriggerEnter(Collider col) {
			if(col.tag == "Player") {
				Camera.main.orthographic = true;
				Camera.main.orthographicSize = 20;
				dungeonTimer = 0.1f;
				col.GetComponent<Player>().boost(7);
			}
		}
		void OnTriggerExit(Collider col) {
			if(col.tag == "Player") {
				Camera.main.orthographic = false;
				Camera.main.fieldOfView = 115;
				dungeonTimer = 0;
				col.GetComponent<Player>().normalSpeed();
				Camera.main.transform.rotation = Quaternion.Euler(Camera.main.transform.eulerAngles.x, Camera.main.transform.eulerAngles.y, 0);
				col.GetComponent<Player>().setInDungeon(false);
				SoundManager.STOP_DUNGEON_THEME();
			}
		}

		void FixedUpdate() {
			if(cleared || !active) return;

			if(spawnDelay > 0) spawnDelay += Time.deltaTime;
			if(spawnDelay > 1) spawnDelay = 0;

			if(gliderDelay > 0) gliderDelay += Time.deltaTime;
			if(gliderDelay > 1) gliderDelay = 0;

			//Game Logic
			gridDelay += Time.deltaTime;
			if(gridDelay > dungeonSpeed && dungeonTimer < dungeonWinTime + 4) {
				gridDelay = 0;
				updateCells();
			}
			for(int i = 0; i < W; i++) for(int j = 0; j < H; j++) setTileState(i, j, alive[i, j]);
		}

		private void updateCells() {
			for(int i = 0; i < W; i++) for(int j = 0; j < H; j++) obeysRules(i, j);
			for(int i = 0; i < W; i++) for(int j = 0; j < H; j++) alive[i, j] = nextState[i, j];
		}
		private int countNeighbors(int x, int y) {
			int n = 0;
			for(int i = -1; i < 2; i++)
				for(int j = -1; j < 2; j++) {
					if((x + i) > W-1 || (y + j) > H-1 || 
					(x + i) < 0 || (y + j) < 0 || 
					(i == 0 && j == 0)) continue;
					if(alive[x+i, y+j]) n++;
				}
			return n;
		}

		private void obeysRules(int i, int j) {
			switch(type) {
				case Type.GAME_OF_LIFE:
					int neigh = countNeighbors(i, j);
					if((neigh < 2 || neigh > 3)) nextState[i, j] = false;
					if(!alive[i, j] && neigh == 3) nextState[i, j] = true;
					if(alive[i, j] && (neigh == 2 || neigh == 3)) nextState[i, j] = true;
					break;
				case Type.TETRIS:
					try {
						if(alive[i, j]) {
							if(!landed[i, j-1]) {
								nextState[i, j-1] = true;
								nextState[i, j] = false;
							}
							else {
								landed[i, j] = true;
								for(int m = -3; m < 2; m++)
									for(int l = -3; l < 2; l++) if(alive[i+m, j+l]) landed[i+m, j+l] = true;
							}
							if(j-1 <= 0) landed[i, j] = true;
						}
					}
					catch(System.IndexOutOfRangeException){}
					break;
			}
		}

		private void setTileState(int x, int y, bool activ) {
			try {
				tiles[x, y].GetComponent<BoxCollider>().enabled = activ;
				tiles[x, y].GetComponent<MeshRenderer>().enabled = activ;
			}
			catch(MissingReferenceException){}
		}

		public bool IsInDungeon() {
			return active && !cleared;
		}

		public Vector2 GetSize() {
			return new Vector2(W, H);
		}
	}
}