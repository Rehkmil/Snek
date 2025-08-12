using UnityEngine;

public class SnakeController : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float detectionRange = 5f;
    public float attackRange = 1.5f;
    public int attackDamage = 10;
    public float attackCooldown = 1f;
    public int maxHealth = 50;
    public int lowHealthThreshold = 15;

    private int currentHealth;
    private float lastAttackTime;
    private Transform player;
    private Rigidbody2D rb;
    private Vector2 patrolDirection;
    private float patrolTimer;
    private Vector2 minBounds;
    private Vector2 maxBounds;
    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;
        currentHealth = maxHealth;
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
        rb = GetComponent<Rigidbody2D>();
        ChooseNewPatrolDirection();
        Camera cam = Camera.main;
        if (cam != null)
        {
            float camHeight = cam.orthographicSize;
            float camWidth = camHeight * cam.aspect;
            minBounds = new Vector2(cam.transform.position.x - camWidth, cam.transform.position.y - camHeight);
            maxBounds = new Vector2(cam.transform.position.x + camWidth, cam.transform.position.y + camHeight);
        }
        else
        {
            minBounds = new Vector2(-9999f, -9999f);
            maxBounds = new Vector2(9999f, 9999f);
        }
    }

    void FixedUpdate()
    {
        Vector2 movement = Vector2.zero;
        float dt = Time.fixedDeltaTime;
        float distanceToPlayer = (player != null) ? Vector2.Distance(transform.position, player.position) : Mathf.Infinity;

        if (currentHealth <= lowHealthThreshold && player != null)
        {
            Vector2 dirAway = ((Vector2)transform.position - (Vector2)player.position).normalized;
            movement = dirAway * moveSpeed;
            FacePlayer();
        }
        else if (distanceToPlayer > detectionRange)
        {
            patrolTimer -= dt;
            movement = patrolDirection * moveSpeed;
            if (patrolTimer <= 0f) ChooseNewPatrolDirection();
            FaceDirection(patrolDirection);
            Debug.Log("Patrolling");
        }
        else if (distanceToPlayer <= attackRange && player != null)
        {
            Attack();
            movement = Vector2.zero;
            FacePlayer();
            Debug.Log("Attacking");
        }
        else if (player != null)
        {
            Vector2 dir = ((Vector2)player.position - (Vector2)transform.position).normalized;
            movement = dir * moveSpeed;
            FacePlayer();
            Debug.Log("Chasing");
        }

        Vector2 targetPos = rb.position + movement * dt;
        targetPos.x = Mathf.Clamp(targetPos.x, minBounds.x, maxBounds.x);
        targetPos.y = Mathf.Clamp(targetPos.y, minBounds.y, maxBounds.y);
        rb.MovePosition(targetPos);
    }

    void FaceDirection(Vector2 dir)
    {
        if (dir.x > 0.01f) transform.localScale = originalScale;
        else if (dir.x < -0.01f) transform.localScale = new Vector3(-originalScale.x, originalScale.y, originalScale.z);
    }

    void FacePlayer()
    {
        if (player == null) return;
        Vector2 dir = player.position - transform.position;
        if (dir.x > 0.01f) transform.localScale = originalScale;
        else if (dir.x < -0.01f) transform.localScale = new Vector3(-originalScale.x, originalScale.y, originalScale.z);
    }

    void Attack()
    {
        if (player == null) return;
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            var pc = player.GetComponent<PlayerController>();
            if (pc != null) pc.TakeDamage(attackDamage);
            lastAttackTime = Time.time;
        }
    }

    void ChooseNewPatrolDirection()
    {
        patrolDirection = Random.insideUnitCircle.normalized;
        if (patrolDirection.sqrMagnitude < 0.01f) patrolDirection = Vector2.up;
        patrolTimer = Random.Range(1.5f, 3.5f);
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);
        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        if (Camera.main != null)
        {
            float camH = Camera.main.orthographicSize;
            float camW = camH * Camera.main.aspect;
            Vector3 camPos = Camera.main.transform.position;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(new Vector3(camPos.x, camPos.y, transform.position.z), new Vector3(camW * 2f, camH * 2f, 0.1f));
        }
    }
}
