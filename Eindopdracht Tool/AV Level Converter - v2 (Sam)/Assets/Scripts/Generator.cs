using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generator : MonoBehaviour {
	public Texture load;

	[System.Serializable]
	public enum GameType {
		Top_Down_2D, Sidescroller_2D
	};

	[SerializeField]
	public GameType gameType;

	public void Generate() {

	}

	public static int AMOUNT_GAMEMODES() {
		return System.Enum.GetNames(typeof(GameType)).Length;
	}
}
