using GraviTrix.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GraviTrix.Runtime
{
    public sealed class GameInputRelay : MonoBehaviour
    {
        [SerializeField] private GameController controller;
        [SerializeField] private float swipeThreshold = 80f;
        [SerializeField] private float tapRotateMaxDuration = 0.25f;

        private bool touchActive;
        private Vector2 touchStartPosition;
        private float touchStartTime;

        private void Update()
        {
            ReadKeyboardInput();
            ReadTouchInput();
        }

        public void OnMoveLeft()
        {
            controller?.MoveLeft();
        }

        public void OnMoveRight()
        {
            controller?.MoveRight();
        }

        public void OnRotateLeft()
        {
            controller?.RotatePieceLeft();
        }

        public void OnRotateRight()
        {
            controller?.RotatePieceRight();
        }

        public void OnHoldPiece()
        {
            controller?.HoldPiece();
        }

        public void OnSoftDrop()
        {
            controller?.SoftDrop();
        }

        public void OnHardDrop()
        {
            controller?.HardDrop();
        }

        public void OnRestart()
        {
            controller?.RestartGame();
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
                controller?.MoveLeft();
            }

            if (keyboard.dKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame)
            {
                controller?.MoveRight();
            }

            if (keyboard.qKey.wasPressedThisFrame)
            {
                controller?.RotatePieceLeft();
            }

            if (keyboard.eKey.wasPressedThisFrame)
            {
                controller?.RotatePieceRight();
            }

            if (keyboard.wKey.wasPressedThisFrame || keyboard.upArrowKey.wasPressedThisFrame)
            {
                controller?.RotatePieceLeft();
            }

            if (keyboard.sKey.wasPressedThisFrame || keyboard.downArrowKey.wasPressedThisFrame)
            {
                controller?.SoftDrop();
            }

            if (keyboard.enterKey.wasPressedThisFrame)
            {
                controller?.HardDrop();
            }

            if (keyboard.rKey.wasPressedThisFrame)
            {
                controller?.RestartGame();
            }

            if (keyboard.cKey.wasPressedThisFrame || keyboard.leftShiftKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame)
            {
                controller?.HoldPiece();
            }
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
                if (UnityEngine.EventSystems.EventSystem.current != null && 
                    UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(touch.touchId.ReadValue()))
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

            if (swipeDelta.magnitude < swipeThreshold && duration <= tapRotateMaxDuration)
            {
                controller?.RotatePieceLeft();
                return;
            }

            if (Mathf.Abs(swipeDelta.x) > Mathf.Abs(swipeDelta.y))
            {
                if (swipeDelta.x < 0f)
                {
                    controller?.MoveLeft();
                }
                else
                {
                    controller?.MoveRight();
                }
            }
            else if (swipeDelta.y < 0f)
            {
                controller?.SoftDrop();
            }
            else
            {
                controller?.HardDrop();
            }
        }
    }
}