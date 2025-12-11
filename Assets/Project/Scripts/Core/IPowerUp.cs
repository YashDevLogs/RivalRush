public interface IPowerUp
{   
    void OnPickup(PlayerController player);
    void OnUse(PlayerController player);
    void OnExpire(PlayerController player);
}
