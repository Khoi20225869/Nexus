using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.Features.Level.Gameplay
{
    [RequireComponent(typeof(Animator))]
    public sealed class PlayerLocomotionAnimatorDriver : MonoBehaviour
    {
        [SerializeField] private float walkSpeed = 2.5f;
        [SerializeField] private float runSpeed = 4.5f;
        [SerializeField] private KeyCode runKey = KeyCode.LeftShift;
        [SerializeField] private KeyCode attackKey = KeyCode.J;
        [SerializeField] private KeyCode hurtKey = KeyCode.H;
        [SerializeField] private KeyCode deadKey = KeyCode.O;
        [SerializeField] private KeyCode reviveKey = KeyCode.P;
        [SerializeField] [Range(0.1f, 1f)] private float deathAnimatorSpeed = 0.35f;

        private static readonly int MoveXHash = Animator.StringToHash("MoveX");
        private static readonly int MoveYHash = Animator.StringToHash("MoveY");
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int AttackHash = Animator.StringToHash("Attack");
        private static readonly int HurtHash = Animator.StringToHash("Hurt");
        private static readonly int DeadHash = Animator.StringToHash("Dead");
        private static readonly int DeathStateShortHash = Animator.StringToHash("Death");
        private static readonly int DeathStateFullHash = Animator.StringToHash("Base Layer.Death");

        private Animator _animator;
        private Vector2 _lastLookDirection = Vector2.down;
        private float _defaultAnimatorSpeed = 1f;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            if (_animator != null)
            {
                _defaultAnimatorSpeed = Mathf.Max(0.01f, _animator.speed);
            }

#if UNITY_EDITOR
            // Self-heal missing controller references in editor play mode.
            if (_animator != null && _animator.runtimeAnimatorController == null)
            {
                var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
                    "Assets/Animators/Character Controller.controller");
                if (controller != null)
                {
                    _animator.runtimeAnimatorController = controller;
                }
            }
#endif
        }

        private void Update()
        {
            if (_animator == null || !_animator.isInitialized || _animator.runtimeAnimatorController == null)
            {
                return;
            }

            if (WasKeyPressedThisFrame(reviveKey))
            {
                _animator.SetBool(DeadHash, false);
                _animator.speed = _defaultAnimatorSpeed;
            }

            if (WasKeyPressedThisFrame(deadKey))
            {
                _animator.SetBool(DeadHash, true);
                _animator.SetFloat(MoveXHash, _lastLookDirection.x);
                _animator.SetFloat(MoveYHash, _lastLookDirection.y);
                _animator.SetFloat(SpeedHash, 0f);
                _animator.speed = Mathf.Clamp(deathAnimatorSpeed, 0.1f, 1f);
                ForcePlayDeathState();
                return;
            }

            if (_animator.GetBool(DeadHash))
            {
                _animator.SetFloat(MoveXHash, _lastLookDirection.x);
                _animator.SetFloat(MoveYHash, _lastLookDirection.y);
                _animator.SetFloat(SpeedHash, 0f);
                return;
            }

            _animator.speed = _defaultAnimatorSpeed;

            var moveInput = ReadMoveInput();
            var hasMoveInput = moveInput.sqrMagnitude > 0.0001f;
            if (hasMoveInput)
            {
                _lastLookDirection = moveInput.normalized;
            }

            var isRunning = hasMoveInput && IsKeyPressed(runKey);
            var moveSpeed = isRunning ? runSpeed : walkSpeed;
            if (hasMoveInput)
            {
                transform.position += (Vector3)(moveInput.normalized * moveSpeed * Time.deltaTime);
            }

            _animator.SetFloat(MoveXHash, _lastLookDirection.x);
            _animator.SetFloat(MoveYHash, _lastLookDirection.y);
            _animator.SetFloat(SpeedHash, hasMoveInput ? (isRunning ? 1f : 0.5f) : 0f);

            if (WasKeyPressedThisFrame(attackKey))
            {
                _animator.SetTrigger(AttackHash);
            }

            if (WasKeyPressedThisFrame(hurtKey))
            {
                _animator.SetTrigger(HurtHash);
            }
        }

        private static Vector2 ReadMoveInput()
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return Vector2.zero;
            }

            var x = 0f;
            var y = 0f;

            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
            {
                x -= 1f;
            }

            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
            {
                x += 1f;
            }

            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
            {
                y -= 1f;
            }

            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
            {
                y += 1f;
            }

            return new Vector2(x, y);
#else
            return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
#endif
        }

        private static bool IsKeyPressed(KeyCode keyCode)
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return false;
            }

            if (!TryMapToInputSystemKey(keyCode, out var key))
            {
                return false;
            }

            return keyboard[key].isPressed;
#else
            return Input.GetKey(keyCode);
#endif
        }

        private static bool WasKeyPressedThisFrame(KeyCode keyCode)
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return false;
            }

            if (!TryMapToInputSystemKey(keyCode, out var key))
            {
                return false;
            }

            return keyboard[key].wasPressedThisFrame;
#else
            return Input.GetKeyDown(keyCode);
#endif
        }

#if ENABLE_INPUT_SYSTEM
        private static bool TryMapToInputSystemKey(KeyCode keyCode, out Key key)
        {
            switch (keyCode)
            {
                case KeyCode.LeftShift:
                    key = Key.LeftShift;
                    return true;
                case KeyCode.RightShift:
                    key = Key.RightShift;
                    return true;
            }

            return System.Enum.TryParse(keyCode.ToString(), true, out key);
        }
#endif

        private void ForcePlayDeathState()
        {
            if (_animator == null)
            {
                return;
            }

            _animator.ResetTrigger(AttackHash);
            _animator.ResetTrigger(HurtHash);

            if (_animator.HasState(0, DeathStateFullHash))
            {
                _animator.Play(DeathStateFullHash, 0, 0f);
            }
            else if (_animator.HasState(0, DeathStateShortHash))
            {
                _animator.Play(DeathStateShortHash, 0, 0f);
            }
        }
    }
}
