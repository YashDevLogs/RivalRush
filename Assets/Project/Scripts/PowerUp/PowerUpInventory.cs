public class PowerUpInventory
{
    public IPowerUp Current { get; private set; }
    public bool HasPowerUp => Current != null;

    public void Assign(IPowerUp powerUp) => Current = powerUp;
    public IPowerUp Consume()
    {
        var p = Current;
        Current = null;
        return p;
    }
    public void Clear() => Current = null;
}
