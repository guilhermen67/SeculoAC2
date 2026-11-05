
using UnityEngine;

public class mov : MonoBehaviour
{
    [Header("Movimentação")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;

    [Header("Estado do personagem")]
    public bool isGrounded;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private bool isDead = false;
    private bool isAttacking = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (isDead)
            return;

        Movement();
        Jump();
        Attack();

        UpdateAnimator();
    }

    // ATUALIZA AS ANIMAÇÕES
    void UpdateAnimator()
    {
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetFloat("Velocidade", Mathf.Abs(rb.velocity.x));
    }

    // MOVIMENTAÇÃO
    void Movement()
    {
        float moveInput = Input.GetAxisRaw("Horizontal");

        rb.velocity = new Vector2(
            moveInput * moveSpeed,
            rb.velocity.y
        );

        if (moveInput < 0)
        {
            spriteRenderer.flipX = true;
        }
        else if (moveInput > 0)
        {
            spriteRenderer.flipX = false;
        }
    }

    // PULO
    void Jump()
    {

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.velocity = new Vector2(
                rb.velocity.x,
                jumpForce
            );

            isGrounded = false;

            //animator.SetTrigger("Jump");
        }
    }

    // ATAQUE
    void Attack()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            animator.SetTrigger("Attack");
        }
    }

    // DETECTAR CHÃO
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
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
}
