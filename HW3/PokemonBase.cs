using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PokemonBase {
	public bool fainted = false;
	
	public int DexNum {
		get;set;
	}
	public int Health {
		get;set;
	}
	public int Attack {
		get;set;
	}
	public int Defense {
		get;set;
	}
	public int Level {
		get;set;
	}
	public int Exp {
		get;set;
	}
	public bool Shiny {
		get;
		protected set;
	}
	public string Name {
		get;set;
	}

	//Can-toevoeging :D
	private const float shinyRarity = 8192;
	public int MaxHealth {
		get;set;
	}
	//

	public PokemonBase(bool shiny) {
		Shiny = shiny;
	}

	//Eigen interpretatie
	public PokemonBase NewPokemon() {
		return new PokemonBase(Random.Range(0, shinyRarity) == 0 ? true : false);
	}

	public void SetStats() {
		Health = MaxHealth;
	}
}
