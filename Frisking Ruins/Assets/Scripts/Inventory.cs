using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

//DJ Beathoven & MC Trucifer
public class Inventory : MonoBehaviour {
	[HideInInspector]
	public Item[] items;
	private Recipe[] recipes;

	[HideInInspector]
	public Object[] loadedRecipes, loadedItems;

	public static int ID_NULL = 0, ID_SWORD = 1;

	[Space(10)]
	public int cells;
	protected Item[] cellItems;

	public Sprite slotSprite, buckleSprite, buckleBackSprite, pinSprite;

	private Transform belt;
	private GameObject[] itemSlots;
	private GameObject buckle, backBuckle, pin, health;
	private Text[] healthText;

	public GameObject itemDropPrefab, splashTextPrefab;

	private int selection = 0, highlight = -1;

	[HideInInspector]
	public Crafting crafting;

	void Awake () {
		crafting = new Crafting(transform, this, splashTextPrefab);
		belt = transform.Find("Belt");
		health = GameObject.Find("Canvas/Health").gameObject;
		healthText = new Text[]{health.GetComponent<Text>(), health.transform.GetChild(0).GetComponent<Text>()};
		cellItems = new Item[cells];
		itemSlots = new GameObject[cells];
		for(int i = 0; i < cellItems.Length; i++) cellItems[i] = items[ID_NULL];
		loadUI();
		loadedItems = Resources.LoadAll("Items/Info", typeof(Item));
		items = new Item[loadedItems.Length];
		for(int i = 0; i < items.Length; i++) items[i] = (Item)loadedItems[i];
		loadedRecipes = Resources.LoadAll("Recipes", typeof(Recipe));
		recipes = new Recipe[loadedRecipes.Length];
		for(int i = 0; i < recipes.Length; i++) recipes[i] = (Recipe)loadedRecipes[i];
	}

	void Update () {
		buckle.GetComponent<Image>().color = new Color(1, 1, 1, 1);
		backBuckle.GetComponent<Image>().color = new Color(1, 1, 1, 1);

		for(int i = 0; i < cells; i++) updateCell(i);
		crafting.update();
	}

	void FixedUpdate () {
		buckle.transform.localPosition = new Vector3(Mathf.Lerp(buckle.transform.localPosition.x, itemSlots[selection].transform.localPosition.x, Time.deltaTime * 7), buckle.transform.localPosition.y, buckle.transform.localPosition.z);
		backBuckle.transform.position = buckle.transform.position - new Vector3(8, 0, 0);
		backBuckle.transform.localScale = buckle.transform.localScale*3;
		pin.transform.position = backBuckle.transform.position - new Vector3(40, 0, 0);

		//Health Info
		health.transform.rotation = buckle.transform.rotation;
	}

	private void updateCell(int i) {
		//Transparancy of Cells
		if(i == selection || i == highlight)
		{
			itemSlots[i].GetComponent<Image>().color = new Color(1, 1, 1, Mathf.Lerp(itemSlots[i].GetComponent<Image>().color.a, belt.gameObject.GetComponent<Image>().color.a + 0.2f, Time.deltaTime * 4));
			itemSlots[i].transform.GetChild(0).GetComponent<Image>().color = new Color(itemSlots[i].transform.GetChild(0).GetComponent<Image>().color.r, itemSlots[i].transform.GetChild(0).GetComponent<Image>().color.g, itemSlots[i].transform.GetChild(0).GetComponent<Image>().color.b, Mathf.Lerp(itemSlots[i].transform.GetChild(0).GetComponent<Image>().color.a, belt.gameObject.GetComponent<Image>().color.a, Time.deltaTime * 4));
		}
		else itemSlots[i].GetComponent<Image>().color = new Color(1, 1, 1, belt.gameObject.GetComponent<Image>().color.a - 0.4f);
		
		//Scaling animation
		float scaleFactor = Mathf.Lerp(itemSlots[i].transform.localScale.x, 1 * ((i == highlight)? 1.1f : 1), Time.deltaTime * 6);
		itemSlots[i].transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);

		//Color
		if(cellItems[i].ID == 0) itemSlots[i].transform.GetChild(0).GetComponent<Image>().color = new Color(itemSlots[i].transform.GetChild(0).GetComponent<Image>().color.r, itemSlots[i].transform.GetChild(0).GetComponent<Image>().color.g, itemSlots[i].transform.GetChild(0).GetComponent<Image>().color.b, 0);
		else if(i != selection && i != highlight) itemSlots[i].transform.GetChild(0).GetComponent<Image>().color = new Color(itemSlots[i].transform.GetChild(0).GetComponent<Image>().color.r, itemSlots[i].transform.GetChild(0).GetComponent<Image>().color.g, itemSlots[i].transform.GetChild(0).GetComponent<Image>().color.b, belt.gameObject.GetComponent<Image>().color.a - 0.6f);

		//Item names & sprites
		itemSlots[i].transform.GetChild(0).name = cellItems[i].getName();
		itemSlots[i].transform.GetChild(0).GetComponent<Image>().sprite = cellItems[i].getTexture();
	}

	public void setHealth(int i) {
		foreach(Text t in healthText) t.text = "Health: " + i;
	}

	private void loadUI() {
		//Cells
		for(int i = 0; i < cells; i++)
		{
			GameObject cell = new GameObject("Inventory Slot " + i);
			cell.AddComponent<Image>();
			cell.transform.SetParent(belt);
			cell.GetComponent<Image>().sprite = slotSprite;
			cell.transform.localPosition = Vector3.zero;
			cell.GetComponent<RectTransform>().sizeDelta = new Vector2(5, 5);
			cell.transform.localScale = new Vector3(1, 1, 1);

			float baseX = (belt.GetComponent<RectTransform>().rect.max.x / belt.GetComponent<RectTransform>().localScale.x) / 2 + 0.18f*cells*2;
			float posX = baseX + (i * -(belt.GetComponent<RectTransform>().rect.max.x)/cells) / 2;
			cell.transform.localPosition -= new Vector3(posX, 0, 0);
			itemSlots[i] = cell;

			//Item Place
			GameObject itm = new GameObject(cellItems[i].getName());
			itm.AddComponent<Image>();
			itm.transform.SetParent(itemSlots[i].transform);
			itm.GetComponent<Image>().sprite = cellItems[i].getTexture();
			itm.transform.localPosition = new Vector3(0.2f, 0.1f, 0);
			itm.GetComponent<RectTransform>().sizeDelta = new Vector2(5, 5);
			itm.transform.localScale = new Vector3(1.3f, 1.1f, 1);
		}
		//Buckle (Selection)
		buckle = new GameObject("Belt Buckle");
		buckle.AddComponent<Image>();
		buckle.transform.SetParent(belt);
		buckle.GetComponent<Image>().sprite = buckleSprite;
		buckle.transform.localPosition = new Vector3(0, 0.1f, 0);
		buckle.GetComponent<RectTransform>().sizeDelta = new Vector2(8, 8);
		buckle.transform.localScale = new Vector3(1, 1, 1);

		//Pin
		pin = new GameObject("Pin");
		pin.AddComponent<Image>();
		pin.transform.SetParent(belt);
		pin.GetComponent<Image>().sprite = pinSprite;
		pin.transform.localPosition = new Vector3(0, 0.1f, 0);
		pin.GetComponent<RectTransform>().sizeDelta = new Vector2(3, 2);
		pin.transform.localScale = new Vector3(1.2f, 1, 1);

		backBuckle = Instantiate(buckle);
		backBuckle.name = "Belt Buckle Back";
		backBuckle.transform.SetParent(belt.parent);
		backBuckle.transform.localPosition = new Vector3(0, 0, 0);
		backBuckle.GetComponent<Image>().sprite = buckleBackSprite;
		backBuckle.transform.localScale = new Vector3(1, 1, 1);
		backBuckle.transform.SetAsFirstSibling();
	}

	public void setSelection(int i) {
		selection = i;
	}

	public void nextSelection() {
		if(selection >= cells-1) return;
		selection++;
		SoundManager.PLAY_STATIONARY_SOUND("BeltSwitch");
	}
	public void prevSelection() {
		if(selection <= 0) return;
		selection--;
		SoundManager.PLAY_STATIONARY_SOUND("BeltSwitch");
	}

	public void setItem(int slot, int itemID) {
		cellItems[slot] = items[itemID];
		highlight = -1;
	}
	public void setItem(int slot, string itemName) {
		foreach(Item i in items) if(i.itemName.ToLower() == itemName.ToLower()) {
			cellItems[slot] = i;
			return;
		}
	}
	public void setCraftingItem(int slot, Item ID)  {
		crafting.setItem(slot, ID);
	}
	
	public void removeItem(int slot) {
		cellItems[slot] = items[ID_NULL];
	}
	public void removeSelection() {
		removeItem(selection);
	}

	public Item getItem(int slot) {
		return cellItems[slot];
	}
	public Item getSelectedItem() {
		return cellItems[selection];
	}

	public void setInventorySpin(float deg) {
		transform.localRotation = Quaternion.Euler(transform.localRotation.x, transform.localRotation.y, Mathf.LerpAngle(transform.localRotation.z, deg, Time.deltaTime * 2));
	}

	//Returns whether the item has to be in which crafting inventory (1) cell, or which regular cell (0)
	public int[] GetEmptySlot(Item item) {
		if(item.craftingItem) return new int[]{crafting.GetEmptySlot(), 1};
		else if(crafting.getSelectedSlot() != null && crafting.getSelectedSlot().isEmpty()) return new int[]{crafting.getSelectedSlot().ID, 1};
		for(int i = 0; i < cellItems.Length; i++) if(cellItems[i] == items[ID_NULL]) return new int[]{i, 0};
		return new int[]{-1, -1};
	}

	public void HighlightCell(int[] i) {
		highlight = i[0];
		if(i[0] < 0 || i[0] > cells || i[1] == 1) highlight = -1;
	}

	public bool hasEmptySlot() {
		for(int i = 0; i < cellItems.Length; i++) if(cellItems[i].ID == 0) return true;
		return false;
	}

	//*******************************\\
	public class Crafting {
		private GameObject menu, splashTextPrefab;

		//Crafting slot contents
		protected CraftingSlot[] slots;
		private Inventory inventory;

		private float craftingDelay = 0;
		private bool isCrafting = false;

		public Crafting(Transform trans, Inventory inv, GameObject splash) {
			this.splashTextPrefab = splash;
			this.inventory = inv;
			menu = trans.Find("Crafting").gameObject;
			slots = menu.transform.GetComponentsInChildren<CraftingSlot>();
			for(int i = 0; i < slots.Length; i++) slots[i].Init(i, inv);
		}

		public void setItem(int slot, Item ID) {
			slots[slot].setItem(ID);
			slots[slot].setSelect(false);
		}

		public void update() {
			//Crafting slots input
			for(int i = 0; i < slots.Length; i++) if(Input.GetKeyDown((i+1).ToString())){
				if(CanSelect()) slots[i].setSelect(!slots[i].selected);
				else {
					slots[i].setSelect(false);
					SoundManager.PLAY_STATIONARY_SOUND("Nope");
				}
			}

			//Crafting slots transparancy when (itemID == 0)
			for(int i = 0; i < slots.Length; i++) {
				if(slots[i].item.ID == 0) slots[i].transform.GetChild(2).GetComponent<Image>().color = new Color(slots[i].GetComponent<Image>().color.r, slots[i].GetComponent<Image>().color.g, slots[i].GetComponent<Image>().color.b, 0);
				else slots[i].transform.GetChild(2).GetComponent<Image>().color = new Color(slots[i].GetComponent<Image>().color.r, slots[i].GetComponent<Image>().color.g, slots[i].GetComponent<Image>().color.b, 1);
			}

			if(!CanSelect()) CalculateCraftingRecipe();
		}

		private void CalculateCraftingRecipe() {
			Item[] curItems = new Item[2];
			int[] curSlots = new int[2];
			int select = 0;
			for(int i = 0; i < slots.Length; i++) {
				if(slots[i].selected && select < 2) {
					curItems[select] = slots[i].item;
					curSlots[select] = i;
					select++;
				}
			}

			if(inventory.hasEmptySlot()) {
				foreach(Recipe rec in inventory.recipes) if(rec.matchesRecipe(curItems)) {
					if(!isCrafting) inventory.StartCoroutine(doRecipe(rec, curSlots));
					break;
				}
			}
		}

		protected IEnumerator doRecipe(Recipe recipe, int[] slotsToWipe) {
			isCrafting = true;
			yield return new WaitForSeconds(0.2f);
			for(int i = 0; i < slotsToWipe.Length; i++) removeItem(slotsToWipe[i]);
			inventory.setItem(inventory.GetEmptySlot(recipe.result)[0], recipe.result.ID);

			GameObject obj = Instantiate(splashTextPrefab);
			obj.name = "Splash Text";
			obj.transform.SetParent(menu.transform);
			obj.transform.localPosition = new Vector3(slots[slotsToWipe[0]].transform.localPosition.x, slots[slotsToWipe[0]].transform.localPosition.y + slots[slotsToWipe[0]].GetComponent<RectTransform>().sizeDelta.y - 3, 0);
			float textScale = 0.1f;
			obj.transform.localScale = new Vector3(textScale, textScale, textScale);
			obj.GetComponent<SplashText>().lifeSpan = 1.2f;
			obj.GetComponent<SplashText>().setText(recipe.actionName);
			SoundManager.PLAY_STATIONARY_SOUND("Item");
			isCrafting = false;
		}

		private bool CanSelect() {
			int i = 0; 
			foreach(CraftingSlot craft in slots) if(craft.selected) i++;
			return i < 2;
		}

		public int GetEmptySlot() {
			for(int i = 0; i < slots.Length; i++) if(slots[i].item == inventory.items[ID_NULL]) return i;
			return -1;
		}

		public CraftingSlot getSelectedSlot() {
			for(int i = 0; i < slots.Length; i++) if(slots[i].selected) return slots[i];
			return null;
		}

		public void removeItem(int slot) {
			slots[slot].setItem(inventory.items[ID_NULL]);
			slots[slot].setSelect(false);
		}

		public void removeSelection() {
			for(int i = 0; i < slots.Length; i++) if(slots[i].selected) {
				removeItem(i);
				break;
			}
		}
	}
}