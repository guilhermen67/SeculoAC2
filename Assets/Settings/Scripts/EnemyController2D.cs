using UnityEngine;

// Faz o inimigo procurar o jogador, perseguir, atacar e perder vida.
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyController2D : MonoBehaviour
{
    [Header("Alvo")]
    [SerializeField] private Transform player; // Quem o inimigo deve perseguir.
    [SerializeField] private float detectionRange = 8f; // Distância máxima para perceber o jogador.

    [Header("Movimento e ataque")]
    [SerializeField] private float moveSpeed = 2.5f; // Velocidade de perseguição.
    [SerializeField] private float attackRange = 1.1f; // Distância em que consegue bater.
    [SerializeField] private int attackDamage = 1; // Quanto de vida o ataque tira.
    [SerializeField] private float attackCooldown = 1f; // Tempo entre ataques.

    [Header("Vida")]
    [SerializeField] private int maxHealth = 3; // Vida máxima do inimigo.

    private Rigidbody2D rb;
    private int currentHealth;
    private float lastAttackTime = -999f;
    private bool isDead;
    private int facingDirection = -1;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead => isDead;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth; // O inimigo começa com a vida cheia.
    }

    private void Start()
    {
        // Se você não arrastou o jogador para o campo Player, tentamos encontrá-lo.
        if (player == null)
        {
            PlayerController2D playerController = FindObjectOfType<PlayerController2D>();
            if (playerController != null)
                player = playerController.transform;
        }
    }

    private void FixedUpdate()
    {
        if (isDead) return;

        // Sem um jogador definido, o inimigo fica parado.
        if (player == null)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
            return;
        }

        float distance = Vector2.Distance(transform.position, player.position);

        // Só começa a agir quando o jogador entra no alcance de detecção.
        if (distance > detectionRange)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
            return;
        }

        // Descobrimos de que lado o jogador está.
        float direction = Mathf.Sign(player.position.x - transform.position.x);
        if (direction != 0)
            facingDirection = direction > 0 ? 1 : -1;

        if (distance > attackRange)
        {
            // Ainda está longe para atacar, então vai atrás do jogador.
            rb.velocity = new Vector2(direction * moveSpeed, rb.velocity.y);
        }
        else
        {
            // Chegou perto: para de andar e tenta atacar.
            rb.velocity = new Vector2(0f, rb.velocity.y);

            if (Time.time >= lastAttackTime + attackCooldown)
                AttackPlayer();
        }
    }

    private void AttackPlayer()
    {
        lastAttackTime = Time.time;

        // Chama o método de dano do jogador, se ele tiver o script esperado.
        PlayerController2D playerController = player.GetComponent<PlayerController2D>();
        if (playerController != null)
            playerController.TakeDamage(attackDamage);
    }

    public void TakeDamage(int damage)
    {
        // Não deixa aplicar dano negativo ou continuar ferindo um inimigo morto.
        if (isDead || damage <= 0) return;

        currentHealth = Mathf.Max(0, currentHealth - damage);

        if (currentHealth == 0)
            Die();
    }

    private void Die()
    {
        isDead = true;
        rb.velocity = Vector2.zero;
        Debug.Log($"{gameObject.name} morreu.");

        // Por enquanto, o inimigo desaparece quando a vida chega a zero.
        // Depois, dá para trocar isso por uma animação de morte.
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        // Amarelo = alcance para perceber o jogador.
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Vermelho = alcance do ataque.
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
