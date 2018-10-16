using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "Data", menuName = "Components/Item", order = 1)]
public class Item : ScriptableObject {
	public int ID;
	public string itemName;
	public Sprite texture;
	public Use itemUse;

	public bool craftingItem = false;

	public enum Use {
		NULL, MELEE_SWING, MELEE_SHANK
	}

	[HideInInspector]
	public float damage, speed, range, cooldown;

	[HideInInspector]
	public int ammo = 1;

	public string getName() {
		return itemName;
	}

	public Sprite getTexture() {
		return texture;
	}
}

#if UNITY_EDITOR
[CustomEditor(typeof(Item))]
public class Item_Editor : Editor {
	public override void OnInspectorGUI() {
		DrawDefaultInspector();
		Item item = (Item)target;
		switch(item.itemUse) 
		{
			default:
			case Item.Use.NULL:
				break;
			case Item.Use.MELEE_SWING:
				item.range = EditorGUILayout.FloatField("Swinging Range", item.range);
				item.speed = EditorGUILayout.Slider("Swinging Speed", item.speed, 0.1f, 10);
				item.damage = EditorGUILayout.FloatField("Damage", item.damage);
				item.cooldown = EditorGUILayout.Slider("Cooldown", item.cooldown, 0.1f, 2);
				break;
			case Item.Use.MELEE_SHANK:
				item.range = EditorGUILayout.FloatField("Shank Range", item.range);
				item.speed = EditorGUILayout.Slider("Shank Speed", item.speed, 0.1f, 10);
				item.damage = EditorGUILayout.FloatField("Damage", item.damage);
				item.cooldown = EditorGUILayout.Slider("Cooldown", item.cooldown, 0.1f, 2);
				break;
		}
	}
}
#endif