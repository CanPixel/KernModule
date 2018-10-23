using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SplashText : MonoBehaviour {
	private Text text, shadow;

	private float lifeTime = 0;
	public float lifeSpan;

	private float baseY, baseRot;
	
	void Start () {
		text = GetComponent<Text>();
		baseY = transform.localPosition.y;
		baseRot = transform.localRotation.z;
		shadow = transform.GetComponentInChildren<Text>();
	}
	
	void FixedUpdate () {
		lifeTime += Time.deltaTime;

		transform.localPosition = new Vector3(transform.localPosition.x, baseY + Mathf.Sin(Time.time * 5)/5, transform.localPosition.z);
		transform.localRotation = Quaternion.Euler(transform.localRotation.x, transform.localRotation.y, baseRot + Mathf.Sin(Time.time * 10));

		if(lifeTime > lifeSpan) {
			text.color = new Color(text.color.r, text.color.g, text.color.b, Mathf.Lerp(text.color.a, 0, Time.deltaTime*2));
			if(text.color.a < 0.1f) Destroy(gameObject);
		}

		shadow.color = new Color(shadow.color.r, shadow.color.g, shadow.color.b, text.color.a);
	}

	public void setText(string txt) {
		if(text == null) return;
		text.text = txt;
		shadow.text = txt;
	}
}
