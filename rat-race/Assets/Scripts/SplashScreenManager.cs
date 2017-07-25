using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class SplashScreenManager : MonoBehaviour {
	public Slider slider;
	public Text highScoreText;
	public Text difficultyText;

	void Start () {
		int highScore = PlayerPrefs.GetInt("highScore");
		slider.value = PlayerPrefs.GetInt("difficulty") != 0 ? (int)PlayerPrefs.GetInt("difficulty") : 1;
		if (highScore > 0) {
			this.highScoreText.text = string.Format("Highest Level: {0}", highScore);
			slider.maxValue = (float)highScore;
		} else {
			this.highScoreText.text = "";
		}
		this.UpdateDifficultyText();
	}

	void Update () {
	}

	private void UpdateDifficultyText() {
		int sliderValue = (int)this.slider.value;
		this.difficultyText.text = string.Format("Difficulty: {0}", sliderValue);
	}

	public void OnDifficultySliderUpdated() {
		int sliderValue = (int)this.slider.value;
		PlayerPrefs.SetInt("difficulty", sliderValue);
		this.UpdateDifficultyText();
	}

	public void LoadGame() {
		Application.LoadLevel("Main");
	}
}
