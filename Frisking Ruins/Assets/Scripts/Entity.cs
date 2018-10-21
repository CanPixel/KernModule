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
		public GameObject target;

		protected SpriteRenderer texture;
		protected MeshRenderer meshParticle;

		[Header("Stats")]
		public int maxHealth;
		public float moveSpeed;
		public float strength;

		private float baseSpeed;

		private float frameDelay, frameTick;
		protected float damagedTick;
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

		[Space(5)]
		public string idleSoundName, hurtSoundName;

		protected bool isInDungeon = false;
		protected bool immortal = false;

		public bool glueToGround = true;
		public bool destroyOnKill = false;

		//Spawning animation
		private Vector3 baseScale;

		protected void Start () {
			Health = maxHealth;
			baseSpeed = moveSpeed;
			if(transform.Find("Sprite") != null) texture = transform.Find("Sprite").GetComponent<SpriteRenderer>();
			meshParticle = GetComponent<MeshRenderer>();
			baseScale = transform.localScale;
			transform.localScale = new Vector3(0, 0, 0);
		}

		public void Damage(float damage, float knockback, Transform source, string hitSound = "") {
			if(immortal) return;

			//Cap tegen constante spam van damage
			if(damagedTick > 0) return;
			damagedTick = 10;

			//Knockback
			Vector3 dir = (transform.position - source.position) * knockback;
			dir.y = 0;
			GetComponent<Rigidbody>().velocity += dir;

			//Sounds
			if(hitSound.Length > 0) SoundManager.PLAY_SOUND(hitSound, transform.position);
			if(hurtSoundName.Length > 0) SoundManager.PLAY_SOUND(hurtSoundName, transform.position);

			//Health Reduction + Death
			Health -= (int)damage;
			if(Health - damage <= 0) Die();

			//Particles
			if(meshParticle != null) ParticleSpawner.SPAWN(meshParticle.material.color, transform.position + new Vector3(0, 1.2f, 0), Quaternion.identity, Random.Range(0.2f, 0.9f));
		}

		public void Heal(int hp) {
			Health += hp;
			if(Health > maxHealth) Health = maxHealth;
		}

		public void Damage(Item item, Transform source) {
			Damage(item.damage, item.knockback, source, item.hitSound);
		}

		protected void FixedUpdate() {
			//Glue to ground / Clamp Y-axis
			if(glueToGround) transform.position = new Vector3(transform.position.x, -0.5f, transform.position.z);

			//Spawning animation
			transform.localScale = new Vector3(Mathf.Lerp(transform.localScale.x, baseScale.x, Time.deltaTime * 1), Mathf.Lerp(transform.localScale.y, baseScale.y, Time.deltaTime * 1), Mathf.Lerp(transform.localScale.z, baseScale.z, Time.deltaTime * 1));

			//Damage Delay + Texture Flickering
			if(damagedTick > 0) damagedTick -= 0.25f;
			if(texture != null) texture.enabled = (damagedTick % 2 == 0 | damagedTick == 0)? true : false;

			//Moving
			if(target != null && damagedTick <= 0) {
				float dist = Vector3.Distance(transform.position, target.transform.position);
				if(Mathf.Abs(dist) < 50) move((target.transform.position - transform.position).normalized);
			}

			//Sounds
			if(idleSoundName.Length > 0 && Time.time % 2 == 0 && Random.Range(0, 7) < 5) SoundManager.PLAY_SOUND(idleSoundName, transform.position);
		}

		virtual public void Die() {
			if(immortal) return;
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
			if(!destroyOnKill) {
				transform.Rotate(10, 90, 0);
				Destroy(GetComponent<Entity>());
			}
			else Destroy(gameObject);
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
					if(currentFrame < moveAnimation.Length - 1) {
						currentFrame++;
						if(currentFrame % 2 == 1) walk();
					}
					else currentFrame = 0;
					texture.sprite = moveAnimation[currentFrame];
					frameTick = 0;
				}
			}
			else texture.sprite = moveAnimation[0];
		}

		protected void walk() {
			walkSound();
			if(meshParticle != null) walkParticle();
		}

		protected virtual void walkParticle() {
			for(int i = 0; i < 3; i++) {
				GameObject part = ParticleSpawner.SPAWN(new Color(0.6f, 0.6f, 0.6f), transform.position + new Vector3(0, 0.9f, -0.8f), Quaternion.identity, Random.Range(0.05f, 0.3f));
				part.GetComponent<Particle>().upForce = 0.2f;
				part.GetComponent<Particle>().sideForce = 0;
			}
		}

		protected virtual void walkSound() {
			if(!isInDungeon) SoundManager.PLAY_SOUND("SandWalk", transform.position);
			else SoundManager.PLAY_SOUND("SolidWalk", transform.position);
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
				//Getten van de player d.m.v een vieze, hele vieze, afgrijselijke manier
				Player player = col.transform.parent.parent.parent.gameObject.GetComponent<Player>();
				if(col.transform.parent.parent == null || player == null) return;
				if(player.IsUsingItem()) Damage(player.GetCurrentItem(), col.transform);
			}
		}

		//Player hurt
		void OnCollisionEnter(Collision col) {
			if(col.gameObject.tag == "Player" && strength > 0) col.gameObject.GetComponent<Player>().Damage(strength, strength, transform);
		}

		public void setInDungeon(bool i) {
			isInDungeon = i;
		}

		public bool InDungeon() {
			return isInDungeon;
		}

		public void setImmortal(bool i) {
			immortal = i;
		}
	}
}
