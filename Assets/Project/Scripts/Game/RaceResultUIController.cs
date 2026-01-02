using UnityEngine;
using Game.Core;
using TMPro;
using UnityEngine.SceneManagement;

public sealed class RaceResultUIController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text[] rankTexts;

    [Header("Colors")]
    [SerializeField] private Color localPlayerColor = Color.yellow;
    [SerializeField] private Color defaultColor = Color.white;

    private void Awake()
    {
        panel.SetActive(false);
    }

    private void OnEnable()
    {
        GameEvents.OnRaceFinished += ShowResults;
    }

    private void OnDisable()
    {
        GameEvents.OnRaceFinished -= ShowResults;
    }

    private void ShowResults()
    {
        var raceManager = RaceManager.Instance;
        var results = raceManager.GetFinishOrder();

        panel.SetActive(true);

        int localPlayerRank = -1;

        for (int i = 0; i < rankTexts.Length; i++)
        {
            if (i >= results.Count)
            {
                rankTexts[i].text = string.Empty;
                continue;
            }

            IPlayerController racer = results[i];
            var racerBehaviour = racer as MonoBehaviour;

            if (racerBehaviour == null)
            {
                rankTexts[i].text = $"{i + 1}. Unknown";
                rankTexts[i].color = defaultColor;
                continue;
            }

            var marker = racerBehaviour.GetComponent<PlayerMarker>();
            bool isLocal = marker != null && marker == PlayerMarker.Local;

            if (isLocal)
                localPlayerRank = i + 1;

            rankTexts[i].text = $"{i + 1}. {(isLocal ? "You" : "AI")}";
            rankTexts[i].color = isLocal ? localPlayerColor : defaultColor;
        }

        UpdateTitle(localPlayerRank);
    }

    // ---------------- TITLE ----------------

    private void UpdateTitle(int rank)
    {
        if (rank == 1)
        {
            titleText.text = "YOU WON!";
        }
        else if (rank > 1)
        {
            titleText.text = $"YOU FINISHED {GetOrdinal(rank)}";
        }
        else
        {
            titleText.text = "RACE FINISHED";
        }
    }

    private string GetOrdinal(int number)
    {
        int rem100 = number % 100;
        if (rem100 >= 11 && rem100 <= 13)
            return number + "TH";

        switch (number % 10)
        {
            case 1: return number + "ST";
            case 2: return number + "ND";
            case 3: return number + "RD";
            default: return number + "TH";
        }
    }

    // ---------------- RESTART ----------------

    public void OnRestartButtonPressed()
    {
        Debug.Log("[RaceResultUI] Restarting race");

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }
}
