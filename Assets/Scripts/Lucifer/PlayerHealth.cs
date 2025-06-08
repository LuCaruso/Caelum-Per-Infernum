using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public int health = 5;
    public int maxHealth;
    public float invincibilityDuration = 1f;
    public bool isInvincible = false;

    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private bool died = false;
    private Animator playerAnimator;
    private Rigidbody2D rb;
    private PlayerMovement playerMovement;
    [SerializeField] private Collider2D mainCollider;

    void Start()
    {
        if (GameManager.Instance != null) health = GameManager.Instance.playerHealth;
        maxHealth = health;
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerAnimator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Collider2D selfCollider = GetComponent<Collider2D>();
        bool selfIsTrigger = selfCollider.isTrigger;
        bool otherIsTrigger = other.isTrigger;

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
        GameManager.Instance.playerHealth -= damage;
        health -= damage;
        health = Mathf.Max(health, 0);
        Debug.Log($"Player hit! Health: {health}/{maxHealth}");
        if (died)
        {
            return;
        }

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

        // Optional: flash the sprite
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
        PlayerSlash playerSlash = GetComponent<PlayerSlash>();
        playerSlash.enabled = false;
        Slash slash = GetComponent<Slash>();
        slash.enabled = false;
        animator = GetComponent<Animator>();
        animator.SetTrigger("Death");
        playerMovement.enabled = false;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        playerAnimator.SetBool("Moving", false);
        died = true;
    }


    public void RevivePlayer()
    {
        // Restaura a vida
        health = maxHealth;
        GameManager.Instance.playerHealth = health;

        // Reseta flags de morte
        isInvincible = false;
        died = false;

        // Reativa movimentação e física
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        if (mainCollider != null) mainCollider.enabled = true;

        if (playerMovement != null) playerMovement.enabled = true;

        // Reativa scripts de ataque
        PlayerSlash playerSlash = GetComponent<PlayerSlash>();
        if (playerSlash != null) playerSlash.enabled = true;

        Slash slash = GetComponent<Slash>();
        if (slash != null) slash.enabled = true;

        // Reseta o Animator
        animator.ResetTrigger("Death");
        animator.SetBool("IsDead", false);
        animator.SetBool("Moving", false);
        animator.Rebind();
        animator.Update(0f);

        // Reposiciona o jogador
        transform.position = Vector3.zero;
        Time.timeScale = 1f;
        Debug.Log("Player revivido!");
    }




}
