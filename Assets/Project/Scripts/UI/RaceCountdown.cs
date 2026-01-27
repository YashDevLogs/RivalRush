using UnityEngine;
using TMPro;
using System.Collections;

public sealed class RaceCountdown : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text countdownText;

    [Header("Countdown Settings")]
    [SerializeField] private float interval = 1f;

    private PlayerController playerController;

    private IEnumerator Start()
    {
        // Wait until local player exists
        yield return new WaitUntil(() => PlayerMarker.Local != null);

        playerController = PlayerMarker.Local.GetComponent<PlayerController>();

        if (playerController == null)
        {
            Debug.LogError("[RaceCountdown] PlayerController not found on local player.");
            yield break;
        }

        // Ensure player is initialized
        yield return null;

        playerController.DisableControl();
        yield return StartCoroutine(StartCountdownRoutine());
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

        countdownText.gameObject.SetActive(false);

        playerController.EnableControl();
    }
}
