using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : Retro.Entity {
	[Space(5)]
	public Inventory inventory;

	private Transform cam;
	private float baseY, baseFOV, time;
	
	private GameObject hold;
	private Item currentItem;
	private Vector3 baseHoldingItemScale, baseHoldingItemPos;
	private float baseHoldingItemRot;
	private float itemCooldown = 0, itemTime = 0;
	private bool usingItem = false;

	protected new void Start () {
		base.Start();
		cam = transform.Find("Camera").transform;
		baseY = cam.localPosition.y;
		baseFOV = cam.GetComponent<Camera>().fieldOfView;
		texture = transform.Find("Sprite").GetComponent<SpriteRenderer>();

		hold = texture.transform.Find("Holding").gameObject;
		baseHoldingItemScale = hold.transform.GetChild(0).localScale;
		baseHoldingItemRot = hold.transform.GetChild(0).localRotation.z;
		baseHoldingItemPos = Vector3.zero;

		//Base items
		inventory.setItem(0, "SWORD");
		//inventory.setItem(1, "CACTUS SHANK");
	}

	void Update () {
		float x = 0, z = 0;
		if(Input.GetKey(KeyCode.W)) z = 1;
		if(Input.GetKey(KeyCode.S)) z = -1;
		if(Input.GetKey(KeyCode.A)) x = -1;
		if(Input.GetKey(KeyCode.D)) x = 1;
		Vector3 dir = new Vector3(x, 0, z);
		move(dir);

		//Clamp Y
		transform.position = new Vector3(transform.position.x, -0.5f, transform.position.z);

		if(isMoving())
		{
			time += Time.deltaTime;
			cam.localPosition = new Vector3(cam.localPosition.x, Mathf.Lerp(cam.localPosition.y, baseY + Mathf.Sin(time*20)/1.5f, Time.deltaTime * 6), cam.localPosition.z);
			cam.GetComponent<Camera>().fieldOfView = Mathf.Lerp(cam.GetComponent<Camera>().fieldOfView, baseFOV + Mathf.Sin(time*20)/1.5f, Time.deltaTime * 4);
		
			//UI Anim
			inventory.setInventorySpin(Mathf.Sin(time*moveSpeed)*8);
		}
		else
		{
			time = 0;
			cam.localPosition = new Vector3(cam.localPosition.x, Mathf.Lerp(cam.localPosition.y, baseY, Time.deltaTime * 3), cam.localPosition.z);
			cam.GetComponent<Camera>().fieldOfView = Mathf.Lerp(cam.GetComponent<Camera>().fieldOfView, baseFOV, Time.deltaTime * 2);

			//UI Anim
			inventory.setInventorySpin(Mathf.Sin(Time.time*2)*8);
		}

		//Inventory Controls
		if(Input.GetButtonDown("Inventory [Right]")) inventory.nextSelection();
		if(Input.GetButtonDown("Inventory [Left]")) inventory.prevSelection();
		updateHoldingItem();
	}

	void FixedUpdate() {
		if(currentItem != null && itemCooldown < currentItem.cooldown && !usingItem) itemCooldown += Time.deltaTime;
	}

	private void updateHoldingItem() {
		Transform itm = hold.transform.GetChild(0);
		currentItem = inventory.getSelectedItem();
		if(currentItem == null || currentItem.getTexture() == null) 
		{
			itm.localScale = new Vector3(Mathf.Lerp(itm.localScale.x, 0, Time.deltaTime * 5), Mathf.Lerp(itm.localScale.y, 0, Time.deltaTime * 5), Mathf.Lerp(itm.localScale.z, 0, Time.deltaTime * 5));
			return;
		}
		itm.localScale = new Vector3(Mathf.Lerp(itm.localScale.x, baseHoldingItemScale.x, Time.deltaTime * 8), Mathf.Lerp(itm.localScale.y, baseHoldingItemScale.y, Time.deltaTime * 8), Mathf.Lerp(itm.localScale.z, baseHoldingItemScale.z, Time.deltaTime * 8));
		itm.GetComponent<SpriteRenderer>().sprite = currentItem.getTexture();
		itm.name = currentItem.getName();

		hold.transform.localRotation = Quaternion.Euler(hold.transform.localEulerAngles.x, Mathf.LerpAngle(hold.transform.localEulerAngles.y, (texture.flipX)? 180 : 0, Time.deltaTime * 8), hold.transform.localEulerAngles.z);
	
		if(Input.GetKeyDown(KeyCode.Space) && canUseItem() && !usingItem) useItem();

		//Item specific updates
		if(usingItem) itemUpdate();
		else itemIdle();

		//Drop item
		if(Input.GetButton("Drop")) dropItem();
	}

	public void dropItem() {
		GameObject item = Instantiate(itemDropPrefab, transform.position, Quaternion.identity);
		item.GetComponent<ItemDrop>().item = inventory.getSelectedItem();
		inventory.removeSelection();
	}

	public void useItem() {
		switch(currentItem.itemUse) 
		{
			case Item.Use.NULL:
				break;
			default:
				itemTime = 0;
				usingItem = true;
				break;
		}
	}

	protected void itemIdle() {
		hold.transform.rotation = Quaternion.Euler(hold.transform.eulerAngles.x, hold.transform.eulerAngles.y, Mathf.LerpAngle(hold.transform.eulerAngles.z, baseHoldingItemRot+2, Time.deltaTime * 6));
		hold.transform.localPosition = new Vector3(Mathf.Lerp(hold.transform.localPosition.x, baseHoldingItemPos.x, Time.deltaTime * 3), Mathf.Lerp(hold.transform.localPosition.y, baseHoldingItemPos.y, Time.deltaTime * 3), Mathf.Lerp(hold.transform.localPosition.z, baseHoldingItemPos.z, Time.deltaTime * 3));
	}

	protected void itemUpdate() {
		switch(currentItem.itemUse) 
		{
			default:
			case Item.Use.NULL:
				break;
			case Item.Use.MELEE_SWING:
				hold.transform.rotation = Quaternion.Euler(hold.transform.eulerAngles.x, hold.transform.eulerAngles.y, Mathf.Sin(itemTime*5*currentItem.speed) * currentItem.range);
				break;
			case Item.Use.MELEE_SHANK:
				hold.transform.rotation = Quaternion.Euler(hold.transform.eulerAngles.x, hold.transform.eulerAngles.y, -45);
				hold.transform.localPosition = new Vector3(hold.transform.localPosition.x + (GetDir()*(currentItem.range/100)), hold.transform.localPosition.y, hold.transform.localPosition.z);
				break;
		}
		itemTime += Time.deltaTime;
		if(itemTime > (1 / currentItem.speed)) finishItemUse();
	}

	public Item GetCurrentItem() {
		return currentItem;
	}

	private void finishItemUse() {
		usingItem = false;
		itemCooldown = 0.001f;
	}

	public bool IsUsingItem() {
		return usingItem;
	}

	protected bool canUseItem() {
		return currentItem.ammo > 0 && itemCooldown >= currentItem.cooldown;
	}

	public void pickupItem(Item item, int[] slot) {
		if(slot[1] == 0) inventory.setItem(slot[0], item.ID);
		else inventory.setCraftingItem(slot[0], item);
	}
}
