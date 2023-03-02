using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GUIManager : MonoBehaviour {
	public static GUIManager instance;

	public GameObject gameOverPanel;
	public Text yourScoreTxt;
	public Text highScoreTxt;

	public Text scoreTxt;
	public Text moveCounterTxt;
	public Text explosionCounterTxt;

	private int score;

	private int moveCounter;

	private int explosionCounter;

	public int Score
	{
		get
		{
			return score;
		}

		set
		{
			score = value;
			scoreTxt.text = score.ToString();
		}
	}

	public int MoveCounter
	{
		get
		{
			return moveCounter;
		}

		set
		{
			moveCounter = value;

			if (moveCounter <= 0)
			{
				moveCounter = 0;
				StartCoroutine(WaitForShifting());
			}

			moveCounterTxt.text = moveCounter.ToString();
		}
	}

	public int ExplosionCounter
	{
		get
		{
			return explosionCounter;
		}

		set
		{
			explosionCounter = value;

			if (explosionCounter <= 0)
			{
				explosionCounter = 0;
				StartCoroutine(WaitForShifting());
			}

			explosionCounterTxt.text = explosionCounter.ToString();

			if (explosionCounter > 1)
				Debug.Log($"{explosionCounter} Explosions left");
			else if (explosionCounter == 1)
				Debug.Log($"{explosionCounter} Explosion left");
			else if (explosionCounter < 1)
				Debug.Log($"{explosionCounter} Explosion");
		}
	}

	void Awake() {
		instance = GetComponent<GUIManager>();

		explosionCounter = 15;
		moveCounter = 25;
		explosionCounterTxt.text = explosionCounter.ToString();
		moveCounterTxt.text = moveCounter.ToString();

		//Debug.Log($"{explosionCounter} Explosions left");
	}

	// Show the game over panel
	public void GameOver() {
		gameOverPanel.SetActive(true);

		if (score > PlayerPrefs.GetInt("HighScore")) {
			PlayerPrefs.SetInt("HighScore", score);
			highScoreTxt.text = "New Best: " + PlayerPrefs.GetInt("HighScore").ToString();
		} else {
			highScoreTxt.text = "Best: " + PlayerPrefs.GetInt("HighScore").ToString();
		}

		yourScoreTxt.text = score.ToString();
	}

	public IEnumerator WaitForShifting()
	{
		yield return new WaitUntil(() => !BoardManager.instance.IsShifting);
		yield return new WaitForSeconds(.25f);
		GameOver();
	}
}
