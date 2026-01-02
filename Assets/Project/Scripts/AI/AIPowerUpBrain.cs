using UnityEngine;

public sealed class AIPowerUpBrain : MonoBehaviour
{
    [Header("Personality")]
    [SerializeField] private AIPersonality personality = AIPersonality.Balanced;

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

    // ----------------------------------------------------

    private void Awake()
    {
        powerUpController = GetComponent<PowerUpController>();
        sensor = GetComponent<AISensor>();
        raceManager = FindFirstObjectByType<RaceManager>();
    }

    // ----------------------------------------------------

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

            Debug.Log($"[AIPowerUpBrain] {personality} using power-up ({GetRacePhase()})");
            return true;
        }

        ScheduleNextDecision();
        return false;
    }

    // ----------------------------------------------------
    // DECISION SCHEDULING
    // ----------------------------------------------------

    private void ScheduleNextDecision()
    {
        nextDecisionTime = Time.time + Random.Range(minDecisionDelay, maxDecisionDelay);
    }

    // ----------------------------------------------------
    // INTENT LOGIC
    // ----------------------------------------------------

    private float CalculateIntentScore()
    {
        float score = GetPhaseBias();
        score += GetPersonalityBias();

        // Defensive bias when in danger
        if (sensor != null && sensor.IsInDanger())
            score += GetDangerBias();

        return Mathf.Clamp01(score);
    }

    // ----------------------------------------------------
    // BIASES
    // ----------------------------------------------------

    private float GetPhaseBias()
    {
        if (raceManager == null)
            return 0.35f;

        float t = Mathf.Clamp01(raceManager.RaceElapsedTime / expectedRaceDuration);

        if (t < 0.3f) return 0.25f; // Early
        if (t < 0.7f) return 0.45f; // Mid
        return 0.75f;               // Late
    }

    private float GetPersonalityBias()
    {
        return personality switch
        {
            AIPersonality.Aggressive => 0.15f,
            AIPersonality.Defensive => -0.05f,
            AIPersonality.Risky => 0.25f,
            _ => 0f, // Balanced
        };
    }

    private float GetDangerBias()
    {
        return personality switch
        {
            AIPersonality.Defensive => 0.35f,
            AIPersonality.Aggressive => 0.15f,
            AIPersonality.Risky => 0.05f,
            _ => 0.2f,
        };
    }

    // ----------------------------------------------------
    // DEBUG / READABILITY
    // ----------------------------------------------------

    private string GetRacePhase()
    {
        if (raceManager == null)
            return "Unknown";

        float t = raceManager.RaceElapsedTime / expectedRaceDuration;

        if (t < 0.3f) return "Early";
        if (t < 0.7f) return "Mid";
        return "Late";
    }
}
