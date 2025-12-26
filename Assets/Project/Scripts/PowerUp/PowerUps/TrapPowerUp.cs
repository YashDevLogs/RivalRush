using UnityEngine;

public sealed class TrapPowerUp : IPowerUp
{
    public PowerUpId Id => PowerUpId.Trap;
    public float Duration => 0f; // instant

    private readonly GameObject trapPrefab;

    public TrapPowerUp(GameObject trapPrefab)
    {
        this.trapPrefab = trapPrefab;
    }

    public void Activate(PowerUpContext context)
    {
        Vector3 dropPos = context.PlayerTransform.position
                        - context.PlayerTransform.right * 1f;

        GameObject trap = Object.Instantiate(trapPrefab, dropPos, Quaternion.identity);

        trap.GetComponent<TrapController>()
            .Initialize(context.PlayerTransform.gameObject); // Pass the owner GameObject
    }


    public void Deactivate() { }
}
