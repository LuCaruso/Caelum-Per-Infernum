using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public int health = 5;
    public int maxHealth;
    public float invincibilityDuration = 1f;
    public bool isInvincible = false;

    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private bool died = false;
    private Rigidbody2D rb;
    private PlayerMovement playerMovement;
    [SerializeField] private Collider2D mainCollider;

    void Start()
    {
        if (GameManager.Instance != null) health = GameManager.Instance.playerHealth;
        maxHealth = health;
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isInvincible && other.CompareTag("EnemyAttack"))
        {
            if (other.IsTouching(mainCollider))
            {
                TakeDamage(1);
            }
        }
    }

    private void TakeDamage(int damage)
    {
        if (died) return; // Não perde vida se já morreu

        GameManager.Instance.playerHealth -= damage;
        health -= damage;
        health = Mathf.Max(health, 0);

        if (health <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(InvincibilityCoroutine());
        }
    }

    private IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;
        float flashInterval = 0.1f;
        float elapsed = 0f;

        while (elapsed < invincibilityDuration)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(flashInterval);
            elapsed += flashInterval;
        }
        spriteRenderer.enabled = true;
        isInvincible = false;
    }

    private void Die()
    {
        died = true;

        PlayerSlash playerSlash = GetComponent<PlayerSlash>();
        if (playerSlash != null) playerSlash.enabled = false;

        Slash slash = GetComponent<Slash>();
        if (slash != null) slash.enabled = false;

        animator.SetTrigger("Death");
        playerMovement.enabled = false;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        animator.SetBool("Moving", false);

        // Aqui avisamos o GameManager que o player morreu, se quiser
        // ou você pode avisar o HUDController via evento.
    }

    public void RevivePlayer()
    {
        health = maxHealth;
        GameManager.Instance.playerHealth = health;

        isInvincible = false;
        died = false;

        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        playerMovement.enabled = true;

        PlayerSlash playerSlash = GetComponent<PlayerSlash>();
        if (playerSlash != null) playerSlash.enabled = true;

        Slash slash = GetComponent<Slash>();
        if (slash != null) slash.enabled = true;

        animator.ResetTrigger("Death");
        // Se tiver bool IsDead, desative aqui:
        animator.SetBool("IsDead", false);

        animator.SetBool("Moving", false);

        // Força voltar para Idle
        animator.Rebind();
        animator.Update(0f);
        animator.Play("Idle");

        transform.position = Vector3.zero;
        Debug.Log("Player revivido!");
    }


}
