using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnEnable()
    {
        GameEvents.OnRaceFinished += HandleRaceFinished;
    }

    private void OnDisable()
    {
        GameEvents.OnRaceFinished -= HandleRaceFinished;
    }

    private void HandleRaceFinished()
    {
        Debug.Log("[GameManager] Race finished. Waiting for results UI.");

        // UI / analytics / progression hooks go here
        // DO NOT control race logic here
    }
}
