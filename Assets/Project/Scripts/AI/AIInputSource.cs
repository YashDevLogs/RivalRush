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
        JumpPressed = false;
        DashPressed = false;
        PowerUpPressed = false;

        bool shouldJump = sensor.ShouldJump();
        bool shouldDash = sensor.ShouldDash();

        if (shouldJump)
        {
            Debug.Log($"[AIInputSource] Jump triggered by sensor on {name}");
            JumpPressed = true;
        }

        if (shouldDash)
        {
            Debug.Log($"[AIInputSource] Dash triggered by sensor on {name}");
            DashPressed = true;
        }

        if (powerUpBrain.ShouldUsePowerUp())
        {
            Debug.Log($"[AIInputSource] PowerUp triggered on {name}");
            PowerUpPressed = true;
        }
    }

}
