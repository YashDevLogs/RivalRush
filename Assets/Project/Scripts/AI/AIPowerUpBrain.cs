using UnityEngine;

public sealed class AIPowerUpBrain : MonoBehaviour
{
    private PowerUpController powerUpController;
    private AISensor sensor;
    private RaceManager raceManager;

    [Header("Race Phase")]
    [SerializeField] private float expectedRaceDuration = 30f;

    [Header("Decision Timing")]
    [SerializeField] private float minDecisionDelay = 0.4f;
    [SerializeField] private float maxDecisionDelay = 1.2f;
    [SerializeField] private float usageCooldown = 2.0f;

    private float nextDecisionTime;
    private float lastUseTime;

    private void Awake()
    {
        powerUpController = GetComponent<PowerUpController>();
        sensor = GetComponent<AISensor>();
        raceManager = FindFirstObjectByType<RaceManager>();
    }

    public bool ShouldUsePowerUp()
    {
        if (!powerUpController.HasPowerUp)
            return false;

        if (Time.time < nextDecisionTime)
            return false;

        if (Time.time < lastUseTime + usageCooldown)
            return false;

        float intentScore = CalculateIntentScore();

        if (Random.value < intentScore)
        {
            lastUseTime = Time.time;
            ScheduleNextDecision();
            Debug.Log($"[AIPowerUpBrain] Using power-up ({GetRacePhase()})");
            return true;
        }

        ScheduleNextDecision();
        return false;
    }

    private void ScheduleNextDecision()
    {
        nextDecisionTime = Time.time + Random.Range(minDecisionDelay, maxDecisionDelay);
    }

    private float CalculateIntentScore()
    {
        float score = GetPhaseBias();

        if (sensor != null && sensor.IsInDanger())
            score += 0.2f;

        return Mathf.Clamp01(score);
    }

    private float GetPhaseBias()
    {
        if (raceManager == null)
            return 0.4f;

        float t = Mathf.Clamp01(raceManager.RaceElapsedTime / expectedRaceDuration);

        if (t < 0.3f) return 0.25f; // early
        else if (t < 0.7f) return 0.45f; // mid
        else return 0.75f; // late
    }

    private string GetRacePhase()
    {
        float t = raceManager.RaceElapsedTime / expectedRaceDuration;

        if (t < 0.3f) return "Early";
        if (t < 0.7f) return "Mid";
        return "Late";
    }
}
