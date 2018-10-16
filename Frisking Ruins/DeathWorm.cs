using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathWorm : Retro.Entity {
	private float swoopTime = 0;
	private Vector3 direction;

	private int segments = 6;
	private GameObject[] bodySegments;

	private float swoopDelay = 0;

	void Awake() {
		initWorm(segments);
	}

	new void Start () {
		swoop();
	}

	public new void Die() {}

	private void initWorm(int segments) {
		this.segments = segments;
		bodySegments = new GameObject[segments];
		for(int i = 0; i < segments; i++) bodySegments[i] = transform.Find("Segment_" + (i+1)).gameObject;
	}
	
	void Update () {
		//Animate body
		for(int i = 0; i < segments; i++) 
		{
			bodySegments[i].transform.localPosition = new Vector3(bodySegments[i].transform.localPosition.x, Mathf.Sin(swoopTime * 4 + moveSpeed * i)/10*i, bodySegments[i].transform.localPosition.z);
			if(i % 2 == 0) {
				bodySegments[i].transform.localRotation = Quaternion.Euler(0, 0, Time.time * 1500 * moveSpeed);

				if(i % 4 == 0) {
					float scale = Mathf.Sin(swoopTime*6)*0.3f + 0.8f;
					bodySegments[i].transform.localScale = new Vector3(scale, scale, scale);
				}
			}
		}

		//Swoop
		if(swoopDelay > 0) swoopDelay += Time.deltaTime;
		if(swoopDelay > 1.5f && swoopTime <= 0) swoopTime = 0.1f;
		if(swoopTime > 0){
			swoopTime += Time.deltaTime;
			transform.position = new Vector3(swoopTime * direction.x, Mathf.Sin(swoopTime * 0.7f) * 50 - 40, swoopTime * direction.z);
			transform.rotation = Quaternion.Euler(Mathf.Sin(swoopTime * 0.2f)*-250+100, transform.eulerAngles.y, transform.eulerAngles.z);
		}

		//End Swoop
		if(swoopTime > 2 && transform.position.y < -40) resetSwoop();
	}

	public void swoop() {
		moveSpeed = Random.Range(5, 110);
		transform.rotation = Quaternion.Euler(90, Random.Range(0, 360), 0);
		transform.position = new Vector3(transform.position.x, -70, transform.position.z);
		float move = moveSpeed / 5;
		direction = new Vector3(Random.Range(0, 1)*move, 0, Random.Range(0, 1)*move);
		swoopDelay = 0.1f;
	}

	private void resetSwoop() {
		transform.rotation = Quaternion.Euler(90, 0, 0);
		swoopTime = swoopDelay = 0;

		swoop();
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
			GameObject part = ParticleSpawner.Instance.Spawn("Sand Particle", new Vector3(transform.position.x, 0, transform.position.z), Quaternion.identity, Random.Range(0.2f, 0.7f));
			if(part == null) return;
			part.GetComponent<Particle>().upForce = 0.5f;
		}
	}
}
