using UnityEngine;
using Game.Core;

[RequireComponent(typeof(PowerUpController))]
public sealed class AIInputDriver : MonoBehaviour, IInputDriver
{
    public float Horizontal { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool UsePowerUpPressed { get; private set; }

    private PowerUpController powerUpController;

    [Header("Sensing")]
    [SerializeField] private float obstacleCheckDistance = 1.5f;
    [SerializeField] private LayerMask obstacleMask;

    private void Awake()
    {
        powerUpController = GetComponent<PowerUpController>();
    }

    private void Update()
    {
        Horizontal = 1f; // Always move forward in race

        JumpPressed = ShouldJump();
        UsePowerUpPressed = ShouldUsePowerUp();
    }

    private bool ShouldJump()
    {
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            Vector2.right,
            obstacleCheckDistance,
            obstacleMask
        );

        return hit.collider != null;
    }

    private bool ShouldUsePowerUp()
    {
        if (!powerUpController.HasPowerUp)
            return false;

        // Simple heuristic (expand later)
        return Random.value > 0.97f;
    }
}
