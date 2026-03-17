using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    private Transform player;
    private Rigidbody2D rb;

    [SerializeField] private float speed = 2f;
    [SerializeField] private float attackRange = 1.5f;

    [SerializeField] private GameObject attackHitbox;
    [SerializeField] private float attackDelay = 0.5f;
    [SerializeField] private float attackDuration = 0.3f;
    [SerializeField] private float cooldown = 2f;

    private bool isAttacking;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void FixedUpdate()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        
        if (distance <= attackRange)
        {
            rb.linearVelocity = Vector2.zero;

            if (!isAttacking)
            {
                StartCoroutine(Attack());
            }
        }
        else
        {
            
            Vector2 direction = (player.position - transform.position).normalized;
            rb.linearVelocity = direction * speed;
        }
    }

    IEnumerator Attack()
    {
        isAttacking = true;

        yield return new WaitForSeconds(attackDelay);

        attackHitbox.SetActive(true);

        yield return new WaitForSeconds(attackDuration);

        attackHitbox.SetActive(false);

        yield return new WaitForSeconds(cooldown);

        isAttacking = false;
    }
}