using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particle : MonoBehaviour {
	public float upForce = 1f;
	public float sideForce = .1f;

	private MeshRenderer renderer;

	void Start () {
		Vector3 force = new Vector3(Random.Range(-sideForce, sideForce), Random.Range(upForce / 2f, upForce), Random.Range(-sideForce, sideForce));
		GetComponent<Rigidbody>().velocity = force;

		renderer = GetComponent<MeshRenderer>();
	}

	void FixedUpdate() {
		renderer.material.color = new Color(renderer.material.color.r, renderer.material.color.g, renderer.material.color.b, Mathf.Lerp(renderer.material.color.a, 0.1f, Time.deltaTime * 0.2f));
	}
}
