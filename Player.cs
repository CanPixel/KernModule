using UnityEngine;
using Assets.Pixelation.Example.Scripts;
using System.Collections;

public class Player : MonoBehaviour {
	//REVOLUTIE VOOR DE HAAKJES PLZ!!

	public bool lockMouseMovement;
	public MouseLook look;

	public Transform weapon;

    public int health {
		get;
		set;
	}

	private Vector3 lastPos;
    private int hurtDelay = 0;

	void Start() {
		health = 5;
		lastPos = transform.position;
	}

	void Update() {
        if (hurtDelay > 0) hurtDelay++;
        if (hurtDelay > 20) hurtDelay = 0;

        if (lockMouseMovement) look.enabled = false;
		else {
			look.enabled = true;
			if(weapon.GetComponent<Weapon>()) weapon.GetComponent<Weapon>().WeaponUpdate();
		}
	}

	void FixedUpdate() {
		if(!lockMouseMovement && weapon.GetComponent<Weapon>()) weapon.GetComponent<Weapon>().FixedWeaponUpdate();
	}

	public bool isMoving() {
		Vector3 disp = transform.position - lastPos;
		lastPos = transform.position;
		return disp.magnitude > 0.001;
	}

    public void hurt(int i) {
        hurtDelay = 1;
        health -= i;

        if (health <= 0) {
            health = 0;
            die();
        }
    }

    public void die() {
        lockMouseMovement = true;
        GetComponent<CharacterMotor>().canControl = false;
        GetComponent<MouseLook>().enabled = false;

        transform.Find("Camera/Main Camera/Gun Camera").gameObject.SetActive(false);
    }
}