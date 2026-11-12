using System.Collections;
using UnityEngine;

// Cuida das ações do jogador: andar, pular, atacar, perder vida e dar dash.
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController2D : MonoBehaviour
{
    [Header("Movimento")]
    [SerializeField] private float moveSpeed = 7f; // Velocidade para andar.
    [SerializeField] private float jumpForce = 12f; // Força do pulo.
    [SerializeField] private Transform groundCheck; // Pontinho nos pés usado para saber se está no chão.
    [SerializeField] private float groundCheckRadius = 0.18f; // Tamanho da área que procura o chão.
    [SerializeField] private LayerMask groundLayer; // Quais objetos contam como chão.

    [Header("Ataque")]
    [SerializeField] private Transform attackPoint; // Lugar onde o ataque acontece.
    [SerializeField] private float attackRadius = 0.65f; // Alcance do golpe.
    [SerializeField] private int attackDamage = 1; // Quanto de vida o golpe tira.
    [SerializeField] private float attackCooldown = 0.35f; // Tempo até poder atacar de novo.
    [SerializeField] private LayerMask enemyLayer; // Quais objetos podem receber o ataque.

    [Header("Vida")]
    [SerializeField] private int maxHealth = 5; // Vida máxima do jogador.

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 16f; // Velocidade durante o dash.
    [SerializeField] private float dashDuration = 0.16f; // Quanto tempo o dash dura.
    [SerializeField] private float dashCooldown = 0.7f; // Tempo até poder usar outro dash.

    private Rigidbody2D rb;
    private int currentHealth;
    private float horizontalInput;
    private float lastAttackTime = -999f;
    private float lastDashTime = -999f;
    private bool isDashing;
    private bool isDead;
    private int facingDirection = 1; // 1 = direita, -1 = esquerda.
    private float originalGravityScale;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead => isDead;

    private void Awake()
    {
        // Guardamos o Rigidbody2D para controlar o movimento pela física.
        rb = GetComponent<Rigidbody2D>();
        originalGravityScale = rb.gravityScale;
        currentHealth = maxHealth; // O jogador começa com a vida cheia.
    }

    private void Update()
    {
        // Se morreu, não pode continuar agindo.
        if (isDead) return;

        horizontalInput = Input.GetAxisRaw("Horizontal");

        // Guardamos para qual lado o personagem está olhando.
        if (horizontalInput > 0) facingDirection = 1;
        else if (horizontalInput < 0) facingDirection = -1;

        // Só pula quando está no chão e não está dando dash.
        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded() && !isDashing)
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);

        // Ataca quando aperta J, respeitando o tempo entre golpes.
        if (Input.GetKeyDown(KeyCode.J) && Time.time >= lastAttackTime + attackCooldown)
            Attack();

        // Shift esquerdo inicia o dash se ele já estiver recarregado.
        if (Input.GetKeyDown(KeyCode.LeftShift) && Time.time >= lastDashTime + dashCooldown && !isDashing)
            StartCoroutine(Dash());
    }

    private void FixedUpdate()
    {
        // Durante o dash, não mudamos a velocidade normal do jogador.
        if (isDead || isDashing) return;

        rb.velocity = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);
    }

    private bool IsGrounded()
    {
        // Sem o GroundCheck configurado, não temos como confirmar que tocou o chão.
        if (groundCheck == null) return false;

        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer) != null;
    }

    private void Attack()
    {
        lastAttackTime = Time.time;

        // Se não houver AttackPoint, usamos um ponto à frente do personagem.
        Vector2 position = attackPoint != null
            ? (Vector2)attackPoint.position
            : (Vector2)transform.position + Vector2.right * facingDirection * 0.7f;

        // Procuramos todos os inimigos dentro do alcance do golpe.
        Collider2D[] hits = Physics2D.OverlapCircleAll(position, attackRadius, enemyLayer);
        foreach (Collider2D hit in hits)
        {
            EnemyController2D enemy = hit.GetComponentInParent<EnemyController2D>();
            if (enemy != null)
                enemy.TakeDamage(attackDamage);
        }
    }

    private IEnumerator Dash()
    {
        isDashing = true;
        lastDashTime = Time.time;

        // Tiramos a gravidade por um instante para o dash seguir reto.
        rb.gravityScale = 0f;
        rb.velocity = new Vector2(facingDirection * dashSpeed, 0f);

        yield return new WaitForSeconds(dashDuration);

        // No fim do dash, a gravidade volta ao valor que tinha antes.
        rb.gravityScale = originalGravityScale;
        isDashing = false;
    }

    public void TakeDamage(int damage)
    {
        // Ignoramos dano inválido ou dano recebido depois da morte.
        if (isDead || damage <= 0) return;

        currentHealth = Mathf.Max(0, currentHealth - damage);

        if (currentHealth == 0)
        {
            isDead = true;
            rb.velocity = Vector2.zero;
            Debug.Log("O jogador morreu.");
            // Depois, você pode trocar esta mensagem por uma tela de Game Over.
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Estas áreas aparecem na Scene quando o objeto está selecionado.
        // Servem para ajustar o chão e o alcance do ataque sem ficar no chute.
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        Gizmos.color = Color.red;
        Vector3 point = attackPoint != null
            ? attackPoint.position
            : transform.position + Vector3.right * facingDirection * 0.7f;
        Gizmos.DrawWireSphere(point, attackRadius);
    }
}
