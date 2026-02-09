using UnityEngine;

[RequireComponent(typeof(AISensor))]
[RequireComponent(typeof(AIPowerUpBrain))]
public sealed class AIInputSource : MonoBehaviour, IInputSource
{
    private AISensor sensor;
    private AIPowerUpBrain powerUpBrain;

    public bool JumpPressed { get; private set; }
    public bool SlidePressed { get; private set; }
    public bool PowerUpPressed { get; private set; }

    private void Awake()
    {
        sensor = GetComponent<AISensor>();
        powerUpBrain = GetComponent<AIPowerUpBrain>();
    }

    private void Update()
    {
        JumpPressed = false;
        SlidePressed = false;
        PowerUpPressed = false;

        bool shouldSlide = sensor.ShouldSlide();
        bool shouldJump = !shouldSlide && sensor.ShouldJump();

        if (shouldJump)
        {
            JumpPressed = true;
        }

        if (shouldSlide)
        {
            SlidePressed = true;
        }

        if (powerUpBrain.ShouldUsePowerUp())
        {
            PowerUpPressed = true;
        }
    }

}
