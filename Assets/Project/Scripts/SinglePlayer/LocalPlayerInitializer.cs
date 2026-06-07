using UnityEngine;

public class LocalPlayerInitializer : MonoBehaviour
{
    private void Start()
    {
        var controller = GetComponent<Game.Player.PlayerController>();

        // 🔥 Enable control without networking
        controller.EnableLocalControl();
    }
}