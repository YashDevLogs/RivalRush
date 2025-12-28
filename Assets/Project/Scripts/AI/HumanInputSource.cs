using UnityEngine;
using UnityEngine.InputSystem;

public sealed class HumanInputSource : MonoBehaviour, IInputSource
{
    public bool JumpPressed { get; private set; }
    public bool DashPressed { get; private set; }
    public bool PowerUpPressed { get; private set; }

    private void Update()
    {
        JumpPressed = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
        DashPressed = Keyboard.current != null && Keyboard.current.shiftKey.wasPressedThisFrame;
        PowerUpPressed = Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
    }
}
