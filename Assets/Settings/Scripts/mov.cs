
using UnityEngine;

public class mov : MonoBehaviour
{
    [Header("Movimentação")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;

    [Header("Sistema de Pulo")]
    public int maxJumps = 1;
    private int jumpsRemaining;

    [Header("Ground Detector")]
    public Transform groundDetector;
    public float groundRadius = 0.15f;
    public LayerMask groundLayer;

    [Header("Estado")]
    public bool isGrounded;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private bool isDead;
    private bool wasGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        maxJumps = Mathf.Max(1, maxJumps);
        jumpsRemaining = maxJumps;
    }

    void Update()
    {
        if (isDead)
            return;

        CheckGround();
        Movement();
        Jump();
        Attack();
        UpdateAnimator();
    }

    // DETECTOR DE CHÃO
    void CheckGround()
    {
        bool touchingGround = Physics2D.OverlapCircle(
            groundDetector.position,
            groundRadius,
            groundLayer
        );

        isGrounded = touchingGround && rb.velocity.y <= 0.05f;

        // Recupera os pulos somente ao aterrissar
        if (isGrounded && !wasGrounded)
        {
            jumpsRemaining = maxJumps;
        }

        wasGrounded = isGrounded;
    }

    // MOVIMENTAÇÃO
    void Movement()
    {
        float input = Input.GetAxisRaw("Horizontal");

        rb.velocity = new Vector2(
            input * moveSpeed,
            rb.velocity.y
        );

        if (input < 0)
            spriteRenderer.flipX = true;
        else if (input > 0)
            spriteRenderer.flipX = false;
    }

    // PULO
    void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && jumpsRemaining > 0)
        {
            rb.velocity = new Vector2(
                rb.velocity.x,
                jumpForce
            );

            jumpsRemaining--;

            isGrounded = false;
            wasGrounded = false;

            animator.SetBool("IsGrounded", false);
        }
    }

    // ANIMAÇÕES
    void UpdateAnimator()
    {
        animator.SetBool("IsGrounded", isGrounded);

        animator.SetFloat(
            "Velocidade",
            Mathf.Abs(rb.velocity.x)
        );
    }

    // ATAQUE
    void Attack()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            animator.SetTrigger("Attack");
        }
    }

    // MORTE
    public void Die()
    {
        if (isDead)
            return;

        isDead = true;

        rb.velocity = Vector2.zero;

        animator.SetTrigger("Dead");
    }

    // VISUALIZAÇÃO DO DETECTOR
    void OnDrawGizmosSelected()
    {
        if (groundDetector == null)
            return;

        Gizmos.color = Color.green;

        Gizmos.DrawWireSphere(
            groundDetector.position,
            groundRadius
        );
    }
}
