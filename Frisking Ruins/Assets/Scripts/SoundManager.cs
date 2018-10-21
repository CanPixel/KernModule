using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour {
	public AudioClip desertSong, dungeonSong;

	[HideInInspector]
	public AudioSource desert, dungeon;

	public static SoundManager instance;

	[Range(0f, 1f)]
	public float MAX_MUSIC_VOLUME;

	[Header("Sounds")]
	public AudioClip[] sounds;
	private string[] names;

	void Start () {
		instance = this;
		desert = gameObject.AddComponent<AudioSource>();
		dungeon = gameObject.AddComponent<AudioSource>();
		desert.clip = desertSong;
		dungeon.clip = dungeonSong;
		desert.loop = true;
		desert.volume = dungeon.volume = 0;
		desert.Play();
		names = new string[sounds.Length];
		for(int i = 0; i < sounds.Length; i++) names[i] = sounds[i].name;
	}

	public static void FADE_MUSIC(float val) {
		instance.desert.volume = Mathf.Lerp(instance.desert.volume, Mathf.Clamp(1f - val, 0, instance.MAX_MUSIC_VOLUME - 0.1f), Time.deltaTime * 2);
		instance.dungeon.volume = Mathf.Lerp(instance.dungeon.volume, Mathf.Clamp(val, 0, instance.MAX_MUSIC_VOLUME + 0.1f), Time.deltaTime * 2);

		instance.desert.pitch = Mathf.Lerp(instance.desert.pitch, Mathf.Clamp(1.5f - val, 0, 1), Time.deltaTime / 2);
	}

	public static void PLAY_DUNGEON_THEME() {
		instance.dungeon.Play();
	}

	public static void STOP_DUNGEON_THEME() {
		instance.dungeon.Stop();
	}

	public static void PLAY_STATIONARY_SOUND(string name, float volume = 10) {
		PLAY_SOUND(name, Camera.main.transform.position - new Vector3(0, 20, 0), volume);
	}

	public static void PLAY_SOUND(string name, Vector3 pos, float volume = 10) {
		if(instance == null) return;
		for(int i = 0; i < instance.sounds.Length; i++) {
			if(name == instance.names[i]) {
				AudioSource.PlayClipAtPoint(instance.sounds[i], new Vector3(pos.x, 1, pos.z), volume);
				return;
			}
		}
		Debug.LogError("Could not find '" + name + "' sound file!");
	}
}
