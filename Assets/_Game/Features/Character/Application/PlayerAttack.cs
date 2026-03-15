using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private GameObject attackHitbox;
    [SerializeField] private float attackDuration = 0.2f;
    [SerializeField] private float attackDistance = 0.6f;

    private bool isAttacking;
    private Vector2 lastDirection = Vector2.down;

    void Update()
    {
        UpdateAttackDirection();

        if (Keyboard.current.jKey.wasPressedThisFrame && !isAttacking)
        {
            StartAttack();
        }
    }

    void UpdateAttackDirection()
    {
        Vector2 dir = Vector2.zero;

        if (Keyboard.current.wKey.isPressed) dir += Vector2.up;
        if (Keyboard.current.sKey.isPressed) dir += Vector2.down;
        if (Keyboard.current.aKey.isPressed) dir += Vector2.left;
        if (Keyboard.current.dKey.isPressed) dir += Vector2.right;

        if (dir != Vector2.zero)
        {
            lastDirection = dir.normalized;
        }

        attackHitbox.transform.localPosition = lastDirection * attackDistance;
    }

    void StartAttack()
    {
        isAttacking = true;

        attackHitbox.SetActive(true);

        Invoke(nameof(EndAttack), attackDuration);
    }

    void EndAttack()
    {
        attackHitbox.SetActive(false);
        isAttacking = false;
    }
}