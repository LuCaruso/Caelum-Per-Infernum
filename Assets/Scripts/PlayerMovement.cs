using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 5f;

    [Header("Dash Settings")]
    public float dashingPower    = 24f;
    public float dashingTime     = 0.2f;
    public float dashingCooldown = 1f;
    public AudioSource dashSound;
    [SerializeField] private TrailRenderer tr;

    [Header("References")]
    public Animator       animator;
    public SimpleJoystick joystick;    // Arraste aqui o seu JoystickBG

    // Internals
    private Rigidbody2D rb;
    private Vector2     moveInput;
    private Vector2     lastMoveDirection = Vector2.down;
    private bool        canDash  = true;
    private bool        isDashing;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    void Update()
    {
        // 1) Input híbrido: joystick tem prioridade
        Vector2 js = joystick != null
            ? new Vector2(joystick.Horizontal, joystick.Vertical)
            : Vector2.zero;

        float h = js.sqrMagnitude > 0.01f
            ? js.x
            : Input.GetAxisRaw("Horizontal");
        float v = js.sqrMagnitude > 0.01f
            ? js.y
            : Input.GetAxisRaw("Vertical");
        moveInput = new Vector2(h, v).normalized;

        // 2) Movimento e dash por teclado
        if (!isDashing)
        {
            rb.linearVelocity = moveInput * speed;

            if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
                StartCoroutine(Dash());
        }

        // 3) Animação
        Animate();
    }

    private IEnumerator Dash()
    {
        canDash   = false;
        isDashing = true;

        rb.linearVelocity = moveInput * dashingPower;
        if (tr != null) tr.emitting = true;
        dashSound?.Play();

        yield return new WaitForSeconds(dashingTime);

        if (tr != null) tr.emitting = false;
        isDashing = false;

        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
    }

    private void Animate()
    {
        bool moving = moveInput.magnitude > 0.1f;
        if (moving)
            lastMoveDirection = moveInput;

        animator.SetBool("Moving", moving);
        animator.SetFloat("x", lastMoveDirection.x);
        animator.SetFloat("y", lastMoveDirection.y);
    }

    /// <summary>
    /// Chamado pelo HUDController ao clicar no botão de Dash.
    /// </summary>
    public bool TryDash()
    {
        if (!isDashing && canDash)
        {
            StartCoroutine(Dash());
            return true;
        }
        return false;
    }

    /// <summary>
    /// Expõe a direção para o PlayerSlash.
    /// </summary>
    public Vector2 LastMoveDirection => lastMoveDirection;
}
