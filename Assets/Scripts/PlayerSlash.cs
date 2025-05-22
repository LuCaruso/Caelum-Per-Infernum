using UnityEngine;
using System.Collections;

public class PlayerSlash : MonoBehaviour
{
    private bool isSlashing;
    private bool canSlash = true;

    public Animator animator;
    public Slash    slashScript;
    public AudioSource SlashSource;

    private Vector2 lastDirection = Vector2.right;
    private PlayerMovement playerMovement;

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        playerMovement = GetComponent<PlayerMovement>();
        if (playerMovement == null)
            Debug.LogError("PlayerSlash: PlayerMovement não encontrado!");
    }

    void Update()
    {
        // Atualiza a direção de ataque
        lastDirection = playerMovement.LastMoveDirection;

        // Slash por teclado
        if (Input.GetKeyDown(KeyCode.Z) && canSlash)
        {
            ExecuteSlash();
        }
    }

    private void ExecuteSlash()
    {
        slashScript.SlashPre(lastDirection.normalized);
        StartCoroutine(SlashRoutine());
    }

    private IEnumerator SlashRoutine()
    {
        canSlash   = false;
        isSlashing = true;

        animator.SetFloat("AttackX", lastDirection.x);
        animator.SetFloat("AttackY", lastDirection.y);
        animator.SetBool("Slashing", true);
        SlashSource?.Play();

        yield return new WaitForSeconds(0.4f);

        animator.SetBool("Slashing", false);
        isSlashing = false;

        yield return new WaitForSeconds(0.1f);
        canSlash = true;
    }

    /// <summary>
    /// Chamado pelo HUDController ao clicar no botão de ataque.
    /// </summary>
    public bool TrySlash()
    {
        if (canSlash)
        {
            ExecuteSlash();
            return true;
        }
        return false;
    }
}
