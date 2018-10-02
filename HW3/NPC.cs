using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Poke;

public class NPC : MonoBehaviour, IInteractable {
	public string Name {
		get;set;
	}

	public Sprite Sprite {
		get;set;
	}

	public void OnInteract () {}
}
