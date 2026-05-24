using UnityEngine;
using System;

public enum GameState
{
    Preparation,    // Defender placing towers (or single player idle)
    AttackerTurn,   // PvP attacker selecting enemies
    WaveRunning,    // Enemies are currently spawning and moving
    GameOver,       // Base destroyed
    GameWin         // All waves survived
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameState CurrentState { get; private set; }
    public bool IsPaused { get; private set; }

    // Event triggered whenever the state changes, so other scripts can listen to it
    public event Action<GameState> OnStateChanged;
    public event Action<bool> OnPauseToggled;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // Determine initial state based on mode
        int mode = PlayerPrefs.GetInt("GameMode", 0);
        if (mode == 2) // PvP Mode
        {
            ChangeState(GameState.AttackerTurn);
        }
        else
        {
            ChangeState(GameState.Preparation);
        }
    }

    public void ChangeState(GameState newState)
    {
        if (CurrentState == newState) return;

        CurrentState = newState;
        Debug.Log("Game State changed to: " + newState);

        OnStateChanged?.Invoke(newState);
    }

    public void TogglePause()
    {
        SetPause(!IsPaused);
    }

    public void SetPause(bool pause)
    {
        if (IsPaused == pause) return;
        IsPaused = pause;
        OnPauseToggled?.Invoke(IsPaused);
    }
}
