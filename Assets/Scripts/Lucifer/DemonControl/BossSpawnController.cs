using UnityEngine;
using System.Collections;

public class BossSpawnController : MonoBehaviour
{
    public GameObject bossObject;
    public Camera mainCamera;
    public PlayerMovement playerMovement;

    public TridentAttack tridentAttack;

    private Vector3 originalCameraPosition;
    private Transform originalCameraParent;
    public float phaseTransitionTime = 1f;
    public float phaseAnimationDuration = 2f;
    public Animator animator;
    public AudioManager audioManager;
    public AudioSource audioSource;
    public AudioClip appearSound;
    public Lava lava;

    void Start()
    {
        // Initialize boss as inactive
        bossObject.SetActive(false);
        tridentAttack.StopAllAttacks();
        tridentAttack.enabled = false;
        StartCoroutine(SpawnSequence());
        lava.StartLava();

    }

    private IEnumerator SpawnSequence()
    {
        bossObject.SetActive(true);
        // Store original camera state
        Transform originalCameraParent = mainCamera.transform.parent;
        Vector3 originalCameraLocalPosition = mainCamera.transform.localPosition;
        Vector3 originalCameraWorldPosition = mainCamera.transform.position;

        // Disable components
        bossObject.GetComponent<DemonAI>().enabled = false;
        bossObject.GetComponent<EnemyJumpAttack>().enabled = false;
        bossObject.GetComponent<DemonFB>().enabled = false;
        tridentAttack.StopAllAttacks();

        // Stop player movement and disable collider
        playerMovement.enabled = false;
        Collider2D playerCollider = playerMovement.GetComponent<Collider2D>();
        bool originalColliderState = false;

        if (playerCollider != null)
        {
            originalColliderState = playerCollider.enabled;
            playerCollider.enabled = false;
        }

        Rigidbody2D playerRb = playerMovement.GetComponent<Rigidbody2D>();
        if (playerRb != null) playerRb.linearVelocity = Vector2.zero;

        // Unparent camera and move to boss
        mainCamera.transform.parent = null;
        Vector3 targetPosition = bossObject.transform.position;
        targetPosition.z = originalCameraWorldPosition.z;

        // Camera transition to boss
        float elapsed = 0f;
        while (elapsed < phaseTransitionTime)
        {
            mainCamera.transform.position = Vector3.Lerp(originalCameraWorldPosition, targetPosition,
                elapsed / phaseTransitionTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Play phase transition animation
        animator.SetTrigger("Spawn");
        audioSource.clip = appearSound;
        audioSource.Play();
        yield return new WaitForSeconds(phaseAnimationDuration);

        // Calculate return position based on current player position
        Vector3 returnPosition = playerMovement.transform.position;
        returnPosition.z = originalCameraWorldPosition.z;

        // Camera transition back to player
        elapsed = 0f;
        while (elapsed < phaseTransitionTime)
        {
            mainCamera.transform.position = Vector3.Lerp(targetPosition, returnPosition,
                elapsed / phaseTransitionTime);
            elapsed += Time.deltaTime;
            yield return null;
        }
        animator.ResetTrigger("Spawn");
        audioManager.StartMusic();

        mainCamera.transform.parent = originalCameraParent;
        mainCamera.transform.localPosition = originalCameraLocalPosition;

        tridentAttack.enabled = true;
        bossObject.GetComponent<DemonAI>().enabled = true;
        bossObject.GetComponent<EnemyJumpAttack>().enabled = true;
        bossObject.GetComponent<DemonFB>().enabled = true;
        tridentAttack.StartAttacks();
        playerMovement.enabled = true;

        if (playerCollider != null)
        {
            playerCollider.enabled = originalColliderState;
        }
    }

}