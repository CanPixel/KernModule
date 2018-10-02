using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Poke;

public class Computer : MonoBehaviour, IInteractable {
	public List<PokemonBase> pcList;

	public void OnInteract() {}

	//Het is niet duidelijk of je een pokemon uit de lijst pakt (en dus verwijdert), of 
	//deze functie moet de opgevraagde pokemon returnen, maar dan is de return type niet 'void'
	public void Withdraw(int index) {
		pcList.RemoveAt(index);
	}

	//Waarvan is deze index? Van pcList, of van de pokemons in je team 
	//(Want nu is er geen manier om te bepalen welke pokemon je in de lijst wilt gooien.)
	public void Deposit(int index/*, Pokemon poke */) {}
}
