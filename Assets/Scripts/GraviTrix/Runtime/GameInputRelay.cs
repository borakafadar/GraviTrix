using GraviTrix.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GraviTrix.Runtime
{
    public sealed class GameInputRelay : MonoBehaviour
    {
        [SerializeField] internal GameController controller;
        [SerializeField] private float swipeThreshold = 80f;
        [SerializeField] private float tapRotateMaxDuration = 0.25f;

        private bool touchActive;
        private Vector2 touchStartPosition;
        private float touchStartTime;

        // Every keyboard, touch and mobile-button action goes through this, so nothing reaches
        // the game while it is paused (PauseMenuView pauses by setting timeScale to 0).
        private GameController Active => Time.timeScale == 0f ? null : controller;

        private void Update()
        {
            ReadKeyboardInput();
            ReadTouchInput();
        }

        public void OnMoveLeft()
        {
            Active?.MoveLeft();
        }

        public void OnMoveRight()
        {
            Active?.MoveRight();
        }

        public void OnRotateLeft()
        {
            Active?.RotatePieceLeft();
        }

        public void OnRotateRight()
        {
            Active?.RotatePieceRight();
        }

        public void OnHoldPiece()
        {
            Active?.HoldPiece();
        }

        public void OnSoftDrop()
        {
            Active?.SoftDrop();
        }

        public void OnHardDrop()
        {
            Active?.HardDrop();
        }

        private void ReadKeyboardInput()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            if (keyboard.aKey.wasPressedThisFrame || keyboard.leftArrowKey.wasPressedThisFrame)
            {
                Active?.MoveLeft();
            }

            if (keyboard.dKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame)
            {
                Active?.MoveRight();
            }

            if (keyboard.qKey.wasPressedThisFrame)
            {
                Active?.RotatePieceLeft();
            }

            if (keyboard.eKey.wasPressedThisFrame)
            {
                Active?.RotatePieceRight();
            }

            if (keyboard.wKey.wasPressedThisFrame || keyboard.upArrowKey.wasPressedThisFrame)
            {
                Active?.RotatePieceLeft();
            }

            if (keyboard.sKey.wasPressedThisFrame || keyboard.downArrowKey.wasPressedThisFrame)
            {
                Active?.SoftDrop();
            }

            if (keyboard.enterKey.wasPressedThisFrame)
            {
                Active?.HardDrop();
            }

            if (keyboard.rKey.wasPressedThisFrame)
            {
                Active?.RestartGame();
            }

            if (keyboard.cKey.wasPressedThisFrame || keyboard.leftShiftKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame)
            {
                Active?.HoldPiece();
            }
        }

        private static bool IsOverUI(UnityEngine.InputSystem.Controls.TouchControl touch)
        {
            var eventSystem = UnityEngine.EventSystems.EventSystem.current;
            return eventSystem != null && eventSystem.IsPointerOverGameObject(touch.touchId.ReadValue());
        }

        private void ReadTouchInput()
        {
            Touchscreen touchscreen = Touchscreen.current;
            if (touchscreen == null || touchscreen.primaryTouch == null)
            {
                return;
            }

            var touch = touchscreen.primaryTouch;

            if (touch.press.wasPressedThisFrame)
            {
                // Ignore touch if it's over a UI element (like our mobile buttons)
                if (IsOverUI(touch))
                {
                    return;
                }

                touchActive = true;
                touchStartPosition = touch.position.ReadValue();
                touchStartTime = Time.unscaledTime;
            }

            if (!touchActive || !touch.press.wasReleasedThisFrame)
            {
                return;
            }

            Vector2 touchEndPosition = touch.position.ReadValue();
            Vector2 swipeDelta = touchEndPosition - touchStartPosition;
            float duration = Time.unscaledTime - touchStartTime;

            touchActive = false;

            // Check again on release: if this script ever updates before the EventSystem (order -1000),
            // the press-frame check misses a new touch on a button, and the tap would also rotate.
            // The UI module keeps a touch's pointer for one frame after release, so this check holds.
            if (IsOverUI(touch))
            {
                return;
            }

            if (swipeDelta.magnitude < swipeThreshold && duration <= tapRotateMaxDuration)
            {
                Active?.RotatePieceLeft();
                return;
            }

            if (Mathf.Abs(swipeDelta.x) > Mathf.Abs(swipeDelta.y))
            {
                if (swipeDelta.x < 0f)
                {
                    Active?.MoveLeft();
                }
                else
                {
                    Active?.MoveRight();
                }
            }
            else if (swipeDelta.y < 0f)
            {
                Active?.SoftDrop();
            }
            else
            {
                Active?.HardDrop();
            }
        }
    }
}