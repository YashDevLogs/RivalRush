using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        GameEvents.OnPlayerDied += HandlePlayerDied;
        GameEvents.OnRaceFinished += HandleRaceFinished;
    }

    private void OnDisable()
    {
        GameEvents.OnPlayerDied -= HandlePlayerDied;
        GameEvents.OnRaceFinished -= HandleRaceFinished;
    }

    private void HandlePlayerDied()
    {
        // analytics, stats, UI update
        Debug.Log("GameManager: Player died.");
    }

    private void HandleRaceFinished()
    {
        Debug.Log("GameManager: Race finished. Show results.");
        // handle UI / results / stopping gameplay
    }
}
