using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particle : MonoBehaviour {
	public float upForce = 1f;
	public float sideForce = .1f;

	void Start () {
		Vector3 force = new Vector3(Random.Range(-sideForce, sideForce), Random.Range(upForce / 2f, upForce), Random.Range(-sideForce, sideForce));
		GetComponent<Rigidbody>().velocity = force;
	}
}
