using System.Collections;
using UnityEngine;

public enum GameState
{
    Ready,
    Playing,
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
        StartCoroutine(StartSequence_Coroutine());
    }

    private IEnumerator StartSequence_Coroutine()
    {
        yield return new WaitForSeconds(5f);

        _state = GameState.Playing;
    }
}
