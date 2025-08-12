using UnityEngine;

public class Snake101 : MonoBehaviour
{
    public float moveSpeed = 3f, detectionRange = 5f, attackRange = 1.5f, attackCooldown = 1f;
    public int attackDamage = 10, maxHealth = 50;

    int currentHealth;
    float lastAttackTime, patrolTimer;
    Transform player;
    Rigidbody2D rb;
    Vector2 patrolDirection, minBounds, maxBounds;
    Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;
        currentHealth = maxHealth;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        rb = GetComponent<Rigidbody2D>();
        ChooseNewPatrolDirection();

        Camera cam = Camera.main;
        if (cam)
        {
            float h = cam.orthographicSize, w = h * cam.aspect;
            Vector3 pos = cam.transform.position;
            minBounds = new Vector2(pos.x - w, pos.y - h);
            maxBounds = new Vector2(pos.x + w, pos.y + h);
        }
        else
        {
            minBounds = Vector2.one * -9999f;
            maxBounds = Vector2.one * 9999f;
        }
    }

    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;
        Vector2 movement = Vector2.zero;
        float dist = player ? Vector2.Distance(transform.position, player.position) : Mathf.Infinity;

        if (dist > detectionRange)
        {
            patrolTimer -= dt;
            movement = patrolDirection * moveSpeed;
            if (patrolTimer <= 0f) ChooseNewPatrolDirection();
            FaceDirection(patrolDirection);
            Debug.Log("Patrolling");
        }
        else if (dist <= attackRange && player)
        {
            Attack();
            FacePlayer();
            Debug.Log("Attacking");
        }
        else if (player)
        {
            movement = ((Vector2)player.position - (Vector2)transform.position).normalized * moveSpeed;
            FacePlayer();
            Debug.Log("Chasing");
        }

        Vector2 target = rb.position + movement * dt;
        rb.MovePosition(new Vector2(Mathf.Clamp(target.x, minBounds.x, maxBounds.x), Mathf.Clamp(target.y, minBounds.y, maxBounds.y)));
    }

    void FaceDirection(Vector2 dir) => transform.localScale = (dir.x > 0.01f) ? originalScale : (dir.x < -0.01f ? new Vector3(-originalScale.x, originalScale.y, originalScale.z) : transform.localScale);
    void FacePlayer() { if (player) FaceDirection(player.position - transform.position); }

    void Attack()
    {
        if (player && Time.time >= lastAttackTime + attackCooldown)
        {
            player.GetComponent<PlayerController>()?.TakeDamage(attackDamage);
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
        currentHealth = Mathf.Max(currentHealth - damage, 0);
        if (currentHealth <= 0) Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (Camera.main)
        {
            float h = Camera.main.orthographicSize, w = h * Camera.main.aspect;
            Vector3 pos = Camera.main.transform.position;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(new Vector3(pos.x, pos.y, transform.position.z), new Vector3(w * 2f, h * 2f, 0.1f));
        }
    }
}