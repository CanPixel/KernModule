using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Retro {
	public class Entity : MonoBehaviour, IHealth {
		public int Health {
			get;
			set;
		}

		public GameObject itemDropPrefab;

		protected SpriteRenderer texture;

		[Header("Health & Speed")]
		public int maxHealth;
		public float moveSpeed, baseSpeed;

		private float frameDelay, frameTick;
		private int currentFrame;

		[System.Serializable]
		public struct Loot {
			public Item item;
			public bool randomAmount;

			[Range(0, 10)]
			public int amount;

			[Range(0, 1)]
			public float rarity;
		}

		[Header("Loot & Items")]
		public Loot[] loot;

		[Header("Animation")]
		public Sprite[] moveAnimation;

		protected void Start () {
			Health = maxHealth;
			baseSpeed = moveSpeed;
		}

		public void Damage(float amount) {
			Health -= (int)amount;
			if(Health - amount <= 0) Die();

			//Particles
			ParticleSpawner.Instance.Spawn(GetComponent<MeshRenderer>().material.color, transform.position + new Vector3(0, 1, 0), Quaternion.identity, Random.Range(0.2f, 0.9f));
		}

		public void Die() {
			foreach(Loot l in loot) {
				if(l.amount == 0) break;
				int lAmount = l.amount;
				if(l.randomAmount) lAmount = Random.Range(1, l.amount);
				for(int i = 0; i < lAmount; i++) {
					if(Random.Range(0f, 1f) > l.rarity) {
						GameObject obj = Instantiate(itemDropPrefab, transform.position, Quaternion.identity);
						obj.transform.SetParent(World.instance.transform);
						float spawnRadius = 0.2f;
						obj.transform.localPosition = new Vector3(transform.position.x + Random.Range(-1, 1) * spawnRadius, 1, transform.position.z + Random.Range(-1, 1) * spawnRadius);
						obj.GetComponent<ItemDrop>().item = l.item;
					}
				}
			}
			Destroy(gameObject);
		}

		protected void move(Vector3 dir) {
			GetComponent<Rigidbody>().velocity = (dir * moveSpeed);
			if(dir.x > 0) texture.flipX = false;
			if(dir.x < 0) texture.flipX = true;

			frameDelay = 1 / (moveSpeed / 2);

			if(isMoving())
			{
				frameTick += Time.deltaTime;
				if(frameTick > frameDelay) {
					if(currentFrame < moveAnimation.Length - 1) currentFrame++;
					else currentFrame = 0;
					texture.sprite = moveAnimation[currentFrame];
					frameTick = 0;
				}
			}
			else texture.sprite = moveAnimation[0];
		}

		protected int GetDir() {
			return (texture.flipX)? -1 : 1;
		}

		protected void animate() {
			frameDelay += Time.deltaTime;
		}

		protected bool isMoving() {
			return GetComponent<Rigidbody>().velocity != Vector3.zero;
		}

		public void boost(float fac) {
			moveSpeed = baseSpeed + fac;
		}
		public void normalSpeed() {
			boost(0);
		}

		void OnTriggerEnter(Collider col) {
			if(col.gameObject.tag == "Weapon") {
				//Getting the player through a dirty, dirty, afgrijselijke way
				Player player = col.transform.parent.parent.parent.gameObject.GetComponent<Player>();
				if(col.transform.parent.parent == null || player == null) return;
				Item item = player.GetCurrentItem();
				if(player.IsUsingItem()) Damage(item.damage);
			}
		}
	}
}
