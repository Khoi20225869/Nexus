using UnityEngine;

public class EnemyChase : MonoBehaviour
{
    private Transform player;
    private Rigidbody2D rb;

    [SerializeField] private float speed = 2f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void FixedUpdate()
    {
        if (player == null) return;

        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = direction * speed;
    }
}