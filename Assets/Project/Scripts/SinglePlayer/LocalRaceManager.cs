using UnityEngine;
using TMPro;

namespace Game.Systems
{
    public class LocalRaceManager : MonoBehaviour
    {
        [SerializeField] private TMP_Text countdownText;

        private float startTime;
        private bool raceStarted;

        public void StartCountdown()
        {
            startTime = Time.time + 3f;
            countdownText.gameObject.SetActive(true);
        }

        private void Update()
        {
            if (raceStarted) return;

            float timeLeft = startTime - Time.time;

            if (timeLeft > 0)
            {
                countdownText.text = Mathf.CeilToInt(timeLeft).ToString();
            }
            else
            {
                raceStarted = true;
                countdownText.text = "GO!";
                Invoke(nameof(HideText), 1f);
            }
        }

        private void HideText()
        {
            countdownText.gameObject.SetActive(false);
        }
    }
}