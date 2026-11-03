using System.Collections;
using TMPro;
using UnityEngine;

public enum GameState
{
    Ready,
    Playing,
    Pause,
    GameOver,
}


public class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<GameManager>();
            }

            return _instance;
        }
    }

    private GameState _state = GameState.Ready;
    public GameState GameState => _state;

    [SerializeField] private TextMeshProUGUI _stateText;
    [SerializeField] private GameObject _optionPopup;


    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this);
            return;
        }
        _instance = this;

        _state = GameState.Ready;
    }

    private void Start()
    {
        _optionPopup.SetActive(false);
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
                _state = GameState.Playing;
                Time.timeScale = 1.0f;
                _optionPopup.SetActive(false);

                CursorManager.Instance.LockCursor();

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

        _stateText.gameObject.SetActive(true);
        _stateText.text = "게임 오버!";
        _stateText.color = Color.red;
    }
}