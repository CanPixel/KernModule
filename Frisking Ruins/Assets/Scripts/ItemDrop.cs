using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDrop : MonoBehaviour {
	public Item item;
	
	private bool lootCap = false, colliding = false;

	private static float pickupDelay = 0;
	private float baseFactor;

	private SpriteRenderer spriteRenderer;

	void Start () {
		spriteRenderer = GetComponent<SpriteRenderer>();
		spriteRenderer.sprite = item.getTexture();
		baseFactor = spriteRenderer.material.GetFloat("_InvFade");
	}

	void FixedUpdate() {
		transform.rotation = Quaternion.Euler(90, transform.eulerAngles.y, transform.eulerAngles.z);

		if(pickupDelay > 0) pickupDelay += Time.deltaTime;
		if(pickupDelay > 1) pickupDelay = 0;

		if(colliding) spriteRenderer.material.SetFloat("_InvFade", Mathf.Lerp(spriteRenderer.material.GetFloat("_InvFade"), 0.5f + Mathf.Sin(Time.time * 15), Time.deltaTime * 4));
		else spriteRenderer.material.SetFloat("_InvFade", Mathf.Lerp(spriteRenderer.material.GetFloat("_InvFade"), baseFactor, Time.deltaTime * 6));
	}
	
	void OnTriggerStay(Collider col) {
		if(col.gameObject.tag == "Player_Pickup" && !lootCap) {
			colliding = true;
			Player player = col.transform.parent.GetComponent<Player>();
			int[] index = player.inventory.GetEmptySlot(item);
			if(index[0] < 0) return;
			player.inventory.HighlightCell(index);

			if(Input.GetButton("Loot") && !lootCap && pickupDelay <= 0) {
				lootCap = true;
				pickupDelay = 0.1f;
				player.pickupItem(item, index);
				Destroy(gameObject);
			}
		}
	}

	void OnTriggerExit(Collider col) {
		if(col.tag == "Player_Pickup") {
			col.transform.parent.GetComponent<Player>().inventory.HighlightCell(new int[]{-1, -1});
			colliding = false;
		}
	}
}
