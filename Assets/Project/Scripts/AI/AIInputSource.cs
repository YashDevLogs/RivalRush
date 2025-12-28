using UnityEngine;

[RequireComponent(typeof(AISensor))]
[RequireComponent(typeof(AIPowerUpBrain))]
public sealed class AIInputSource : MonoBehaviour, IInputSource
{
    private AISensor sensor;
    private AIPowerUpBrain powerUpBrain;

    public bool JumpPressed { get; private set; }
    public bool DashPressed { get; private set; }
    public bool PowerUpPressed { get; private set; }

    private void Awake()
    {
        sensor = GetComponent<AISensor>();
        powerUpBrain = GetComponent<AIPowerUpBrain>();
    }

    private void Update()
    {
        // Reset every frame (button-press semantics)
        JumpPressed = false;
        DashPressed = false;
        PowerUpPressed = false;

        // Movement decisions
        if (sensor.ShouldJump())
            JumpPressed = true;

        if (sensor.ShouldDash())
            DashPressed = true;

        // Power-up decision (delegated to brain)
        if (powerUpBrain.ShouldUsePowerUp())
            PowerUpPressed = true;
    }
}
