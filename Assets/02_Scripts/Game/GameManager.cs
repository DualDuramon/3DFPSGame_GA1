using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    Ready,
    Playing,
    Pause,
    GameOver,
}


public class GameManager : SingletonBehaviour<GameManager>
{
    private GameState _state = GameState.Ready;
    public GameState GameState => _state;

    [SerializeField] private TextMeshProUGUI _stateText;
    [SerializeField] private GameObject _optionPopup;
    [SerializeField] private GameObject _gameOverPopup;


    // 씬의 StartText/OptionPopup을 직접 참조하므로 씬 재시작 시 새로 만들어져야 한다
    protected override bool PersistAcrossScenes => false;

    protected override void Awake()
    {
        base.Awake();
        _state = GameState.Ready;
    }

    private void Start()
    {
        _optionPopup.SetActive(false);
        _gameOverPopup.SetActive(false);
        StartCoroutine(StartSequence_Coroutine());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_state == GameState.Playing)
            {
                _state = GameState.Pause;
                Time.timeScale = 0.0f;

                _optionPopup.SetActive(true);
                CursorManager.Instance.UnlockCursor();
            }
            else if (_state == GameState.Pause)
            {
                Resume();
            }
        }
    }

    private IEnumerator StartSequence_Coroutine()
    {
        _stateText.gameObject.SetActive(true);
        _stateText.text = "준비";
        yield return new WaitForSeconds(3f);

        _stateText.text = "시작!";
        yield return new WaitForSeconds(1f);
        _stateText.gameObject.SetActive(false);

        _state = GameState.Playing;
    }

    public void GameOver()
    {
        _state = GameState.GameOver;

        _gameOverPopup.SetActive(true);
        CursorManager.Instance.UnlockCursor();
    }

    public void Resume()
    {
        _state = GameState.Playing;
        Time.timeScale = 1.0f;
        _optionPopup.SetActive(false);

        CursorManager.Instance.LockCursor();

    }

    public void Restart()
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Quit()
    {
# if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}