using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CraftingSlot : MonoBehaviour {
	public Item item {
		get;
		private set;
	}

	public GameObject textPrefab, selectPrefab;
	private GameObject select, itemOBJ, text, itemText;

	public bool selected {
		private set;
		get;
	}

	public int ID {
		get;
		private set;
	}

	void Awake() {
		select = Instantiate(selectPrefab, new Vector3(0, 0, 0), Quaternion.identity);
		select.transform.SetParent(transform);
		select.transform.localPosition = new Vector3(0, 0, 0);
		select.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
		select.name = "Select";
	}

	void FixedUpdate() {
		if(itemOBJ != null) {
			if(itemOBJ.GetComponent<Image>() != null) itemOBJ.GetComponent<Image>().sprite = item.texture;
			itemOBJ.name = item.name;
			setItemText(item.itemName);
		}

		//Hide on blankItem
		itemText.SetActive(isRealItem() & selected);

		//Selection
		select.SetActive(selected);
	}

	public void Init(int id, Inventory inv) {
		ID = id;
		item = inv.items[Inventory.ID_NULL];

		text = Instantiate(textPrefab);
		text.transform.SetParent(transform);
		text.GetComponent<Text>().text = (id+1).ToString();
		text.name = "Cell Number";
		text.transform.localPosition = new Vector3(1.9f, 2.9f, 0);
		text.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

		itemOBJ = new GameObject(item.getName());
		itemOBJ.AddComponent<Image>();
		itemOBJ.transform.SetParent(transform);
		itemOBJ.GetComponent<Image>().sprite = item.getTexture();
		itemOBJ.transform.localPosition = new Vector3(-0.1f, 0.1f, 0);
		itemOBJ.GetComponent<RectTransform>().sizeDelta = new Vector2(5, 5);
		itemOBJ.transform.localScale = new Vector3(0.9f, 0.8f, 1);

		itemText = Instantiate(textPrefab);
		itemText.transform.SetParent(transform);
		itemText.name = "Item Name";
		itemText.transform.localPosition = new Vector3(0, 6, 0);
		itemText.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
		itemText.GetComponent<RectTransform>().sizeDelta = new Vector2(150, 50);
		itemText.GetComponent<Text>().color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
		itemText.GetComponent<Text>().fontSize = 20;
	}

	private bool isRealItem() {
		return item.ID != 0;
	}

	public bool isEmpty() {
		return !isRealItem();
	}

	private void setItemText(string txt) {
		itemText.GetComponent<Text>().text = txt;
	}

	public void setItem(Item item) {
		this.item = item;
	}

	public void setSelect(bool i) {
		selected = i;
	}
}
