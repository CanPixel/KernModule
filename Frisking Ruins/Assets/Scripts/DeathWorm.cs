using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathWorm : Retro.Entity {
	private float swoopTime = 0;
	private Vector3 direction, swoopPos;

	private int segments = 6;
	private GameObject[] bodySegments;

	private float swoopDelay = 0;
	private float lifeTime = 0;

	private bool swooping = false;

	void Awake() {
		initWorm(segments);
	}

	new void Start () {}
	public new void Die() {}

	private void initWorm(int segments) {
		this.segments = segments;
		bodySegments = new GameObject[segments];
		for(int i = 0; i < segments; i++) bodySegments[i] = transform.Find("Segment_" + (i+1)).gameObject;
	}
	
	void Update () {
		lifeTime += Time.deltaTime;
		if(lifeTime > 0.5f && !swooping) {
			swoop();
			swooping = true;
		}
		
		//Animate body
		for(int i = 0; i < segments; i++) 
		{
			bodySegments[i].transform.localPosition = new Vector3(bodySegments[i].transform.localPosition.x, Mathf.Sin(swoopTime * 4 + moveSpeed * i)/10*i, bodySegments[i].transform.localPosition.z);
			if(i % 2 == 0) {
				bodySegments[i].transform.localRotation = Quaternion.Euler(0, 0, Time.time * 1500 * moveSpeed);

				if(i % 4 == 0) {
					float scale = Mathf.Sin(swoopTime*6) * 0.3f + 0.8f;
					bodySegments[i].transform.localScale = new Vector3(scale, scale, scale);
				}
			}
		}

		//Swoop
		if(swoopDelay > 0) swoopDelay += Time.deltaTime;
		if(swoopDelay > 1.5f && swoopTime <= 0) swoopTime = 0.1f;
		if(swoopTime > 0){
			swoopTime += Time.deltaTime;
			transform.position = new Vector3(swoopPos.x + swoopTime * direction.x, Mathf.Sin(swoopTime * 0.7f) * 50 - 40, swoopPos.z + swoopTime * direction.z);
			transform.rotation = Quaternion.Euler(Mathf.Sin(swoopTime * 0.2f)*-250+100, transform.eulerAngles.y, transform.eulerAngles.z);
		}

		//End Swoop
		if(swoopTime > 2 && transform.position.y < -40) resetSwoop();
	}

	new void FixedUpdate() {
		//Sounds
		if(idleSoundName.Length > 0 && Time.time % 2 == 0 && Random.Range(0, 7) < 5) SoundManager.PLAY_SOUND(idleSoundName, transform.position);
	}

	public void swoop() {
		moveSpeed = Random.Range(5, 110);
		transform.rotation = Quaternion.Euler(90, Random.Range(0, 360), 0);
		float randX = transform.position.x;//Random.Range(-(World.instance.getWorldSize().x/2)/4, World.instance.getWorldSize().x/8);
		swoopPos = transform.position = new Vector3(randX, -70, transform.position.z);
		float move = moveSpeed / 5;
		direction = new Vector3(Random.Range(0, 1)*move, 0, Random.Range(0, 1)*move);
		StartCoroutine(chargeWorm());
		swoopDelay = 0.1f;
	}

	private void resetSwoop() {
		transform.rotation = Quaternion.Euler(90, 0, 0);
		swoopTime = swoopDelay = 0;

		swoop();
	}

	IEnumerator chargeWorm() {
		for(int i = 0; i < 4; i++) {
			SoundManager.PLAY_SOUND("WormAppear", transform.position);
			if(i % 2 != 0) continue;
			for(int ang = 0; ang < 360; ang += 6) {
				float posX = 0.2f * Mathf.Sin(ang * Mathf.Deg2Rad);
				float posY = 0.2f * Mathf.Cos(ang * Mathf.Deg2Rad);
				GameObject part = ParticleSpawner.SPAWN(new Color(0.2f, 0.0f, 0.0f), new Vector3(transform.position.x + posX, 2, transform.position.z + posY), Quaternion.identity, Random.Range(0.1f, 0.4f));
				part.GetComponent<Particle>().upForce = 0f;
				part.GetComponent<Particle>().sideForce = 0;
			}
			yield return new WaitForSeconds(0.1f);
		}
	}

	//Shake Environment
	void OnTriggerStay(Collider col) {
		if(col.tag == "Cacti") {
			float amp = 10;
			float dist = Mathf.Clamp(Vector3.Distance(col.transform.position, transform.position), -amp, amp);
			float anim = Mathf.Sin(Time.time * dist/2)*2+2;
			col.transform.localScale = new Vector3(col.transform.localScale.x, col.transform.localScale.y, anim);
		}
		if(col.tag == "Sprout") {
			float amp = 1;
			float dist = Mathf.Clamp(Vector3.Distance(col.transform.position, transform.position), -amp, amp);
			float anim = Mathf.Sin(Time.time * dist)*2+2;
			col.transform.localRotation = Quaternion.Euler(anim, col.transform.localEulerAngles.y, col.transform.localEulerAngles.z);
		}
	}

	void OnTriggerEnter(Collider col) {
		if(col.tag == "Terrain") for(int i = 0; i < Random.Range(35, 52); i++) {
			for(int ang = 0; ang < 360; ang++) {
				float posX = Mathf.Sin(ang * Mathf.Deg2Rad);
				float posY = Mathf.Cos(ang * Mathf.Deg2Rad);
				GameObject part = ParticleSpawner.SPAWN("Sand Particle", new Vector3(transform.position.x + posX, 2, transform.position.z + posY), Quaternion.identity, Random.Range(0.2f, 0.7f));
				part.GetComponent<Particle>().upForce = 0.5f;
				part.GetComponent<Particle>().sideForce = 0;
			}
			SoundManager.PLAY_SOUND("WormAppear", transform.position, 0.2f);
		}
		//Test
		if(col.tag == "Player") {
			col.GetComponent<Player>().Damage(10, 5, transform);
		}
	}
}
