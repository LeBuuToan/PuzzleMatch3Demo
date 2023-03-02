using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class GameManager : MonoBehaviour {
	public static GameManager instance;

	public GameObject pauseMenuUI;

	public bool paused = false;

    void Awake()
    {
        instance = GetComponent<GameManager>();
        DOTween.Init();
    }
    

    public void ResumeButton()
    {
        paused = false;
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
    }

    public void PauseButton()
    {
        paused = true;
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;

    }

    public void ReplayButton()
    {
        SceneManager.LoadScene(Constant.Game);
    }
    

    public void PlayButton()
    {
        SceneManager.LoadScene(Constant.Game);
    }

    public void MenuButton()
    {
        SceneManager.LoadScene(Constant.Menu);
        Time.timeScale = 1f;
    }

    public void QuitGameButton()
    {
        Application.Quit();
    }
}
