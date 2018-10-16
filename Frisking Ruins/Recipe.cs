using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "Data", menuName = "Components/Recipe", order = 2)]
public class Recipe : ScriptableObject {
	public string actionName;
	public Item[] recipe = new Item[2];
	public Item result;

	public bool matchesRecipe(Item[] itms) {
		return (itms[0].ID == recipe[0].ID) && (itms[1].ID == recipe[1].ID) || (itms[0].ID == recipe[1].ID) && (itms[1].ID == recipe[0].ID);
	}
}
