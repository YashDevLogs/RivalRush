using UnityEngine;
using UnityEngine.InputSystem;

public sealed class HumanInputSource : MonoBehaviour, IInputSource
{
    public bool JumpPressed { get; private set; }
    public bool SlidePressed { get; private set; }
    public bool PowerUpPressed { get; private set; }

    private void Update()
    {
        JumpPressed = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
        SlidePressed = Keyboard.current != null && Keyboard.current.leftCtrlKey.wasPressedThisFrame;
        PowerUpPressed = Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
    }
}
