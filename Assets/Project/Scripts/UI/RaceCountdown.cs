using UnityEngine;
using TMPro;
using System.Collections;

public class RaceCountdown : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text countdownText;

    [Header("Countdown Settings")]
    [SerializeField] private float interval = 1f;

    [Header("Player Reference")]
    [SerializeField] private PlayerController playerController;

    private void Start()
    {
        // Stop player at the beginning
        if (playerController != null)
            playerController.DisableControl();

        StartCoroutine(StartCountdownRoutine());
    }

    private IEnumerator StartCountdownRoutine()
    {
        countdownText.gameObject.SetActive(true);

        countdownText.text = "3";
        yield return new WaitForSeconds(interval);

        countdownText.text = "2";
        yield return new WaitForSeconds(interval);

        countdownText.text = "1";
        yield return new WaitForSeconds(interval);

        countdownText.text = "GO!";
        yield return new WaitForSeconds(0.5f);

        // Hide UI
        countdownText.gameObject.SetActive(false);

        // Start player run
        playerController.EnableControl();
    }
}
