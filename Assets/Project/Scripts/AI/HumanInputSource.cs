using Game.Systems;
using Game.Core;
using Game.Input;
using Game.Player;
using Game.AI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Input
{
    public sealed class HumanInputSource : MonoBehaviour, IInputSource
    {
        public float Horizontal { get; private set; }
        public bool JumpPressed { get; private set; }
        public bool SlidePressed { get; private set; }
        public bool PowerUpPressed { get; private set; }

        private void Update()
        {
            Horizontal = GetHorizontalInput();
            JumpPressed = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
            SlidePressed = Keyboard.current != null && Keyboard.current.leftCtrlKey.wasPressedThisFrame;
            PowerUpPressed = Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
        }

        private static float GetHorizontalInput()
        {
            if (Keyboard.current == null)
            {
                return 0f;
            }

            float horizontal = 0f;

            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            {
                horizontal -= 1f;
            }

            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            {
                horizontal += 1f;
            }

            return Mathf.Clamp(horizontal, -1f, 1f);
        }
    }

}