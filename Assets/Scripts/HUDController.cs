using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Necromancer.Tower;

public class HUDController : MonoBehaviour
{
    [Header("Referências - Player")]
    public GameObject player;
    public GameObject heartPrefab;
    public Transform heartsParent;

    [Header("Boss UI")]
    public GameObject boss;
    public GameObject bossHealthBar;
    public Image bossFillImage;

    [Header("Endgame UI")]
    public GameObject endGamePanelVictory;
    public GameObject endGamePanelDefeat;
    public bool eh_finalBoss;

    [Header("Cooldown UI")]
    public Image dashImage;
    public Image attackImage;
    public float cooldownFillDuration = 1f;

    [Header("Pause UI")]
    public GameObject buttonPause;
    public GameObject pauseLight;
    public GameObject pausePanel;
    public GameObject pauseCloseLight;
    public GameObject controlsPauseLight;
    public GameObject configPauseLight;
    public GameObject exitPauseLight;

    [Header("Popups")]
    public GameObject controlsPopup;
    public GameObject controlsCloseLight;
    public GameObject settingsPopup;
    public GameObject settingsCloseLight;

    [Header("Configurações de Áudio")]
    public Slider volumeSlider;

    [Header("Cenas")]
    public string mainMenuSceneName = "MainMenu";

    [Header("Joystick Virtual")]
    public SimpleJoystick joystick;

    [Header("AdScreen")]
    public GameObject adScreen;
    public GameObject btnReiniciar;

    private PlayerMovement playerMovement;
    private PlayerSlash playerSlash;
    private PlayerHealth playerHealth;
    private List<Image> hearts = new List<Image>();
    private FieldInfo currentHealthField;
    private TowerSpawner towerSpawner;
    private Health bossHealth;
    private BossController bossController;
    private int maxTotalHealth;
    private bool maxHealthInitialized = false;

    private bool defeatTriggered = false;
    private bool victoryTriggered = false;
    private bool isPaused = false;

    private Coroutine dashRoutine;
    private Coroutine attackRoutine;

    private const string VOLUME_KEY = "GlobalVolume";
    private const string DEATHCOUNT_KEY = "PlayerDeathCount";
    private bool adScreenActive = false;

    // PlayerPrefs para persistir entre cenas e garantir o fluxo correto
    private int playerDeathCount
    {
        get => PlayerPrefs.GetInt(DEATHCOUNT_KEY, 0);
        set => PlayerPrefs.SetInt(DEATHCOUNT_KEY, value);
    }

    void Awake()
    {
        playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth == null)
            Debug.LogError("HUDController: PlayerHealth não encontrado!");

        playerMovement = player.GetComponent<PlayerMovement>();
        playerSlash = player.GetComponent<PlayerSlash>();
        if (playerMovement == null || playerSlash == null)
            Debug.LogError("HUDController: PlayerMovement ou PlayerSlash não encontrados!");

        currentHealthField = typeof(Health).GetField("currentHealth", BindingFlags.NonPublic | BindingFlags.Instance);

        if (boss != null)
        {
            towerSpawner = boss.GetComponent<TowerSpawner>();
            bossHealth = boss.GetComponent<Health>();
            bossController = boss.GetComponent<BossController>();
        }

        if (dashImage != null)
        {
            dashImage.type = Image.Type.Filled;
            dashImage.fillMethod = Image.FillMethod.Vertical;
            dashImage.fillOrigin = (int)Image.OriginVertical.Bottom;
            dashImage.fillAmount = 1f;
        }
        if (attackImage != null)
        {
            attackImage.type = Image.Type.Filled;
            attackImage.fillMethod = Image.FillMethod.Vertical;
            attackImage.fillOrigin = (int)Image.OriginVertical.Bottom;
            attackImage.fillAmount = 1f;
        }
    }

    void Start()
    {
        Time.timeScale = 1f;

        if (boss == null && bossHealthBar != null)
            bossHealthBar.SetActive(false);

        for (int i = 0; i < playerHealth.health; i++)
        {
            var go = Instantiate(heartPrefab, heartsParent);
            hearts.Add(go.GetComponent<Image>());
        }

        if (bossFillImage != null)
            bossFillImage.fillAmount = 1f;

        pauseLight?.SetActive(false);
        pausePanel?.SetActive(false);
        pauseCloseLight?.SetActive(false);
        controlsPauseLight?.SetActive(false);
        configPauseLight?.SetActive(false);
        exitPauseLight?.SetActive(false);
        controlsPopup?.SetActive(false);
        controlsCloseLight?.SetActive(false);
        settingsPopup?.SetActive(false);
        settingsCloseLight?.SetActive(false);

        endGamePanelVictory?.SetActive(false);
        endGamePanelDefeat?.SetActive(false);
        adScreen?.SetActive(false);

        if (btnReiniciar != null)
            btnReiniciar.SetActive(true); // Garante que começa ativo

        float savedVol = PlayerPrefs.GetFloat(VOLUME_KEY, 1f);
        AudioListener.volume = savedVol;
        if (volumeSlider != null)
        {
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;
            volumeSlider.value = savedVol;

            volumeSlider.onValueChanged.RemoveAllListeners();
            volumeSlider.onValueChanged.AddListener(vol =>
            {
                AudioListener.volume = vol;
                PlayerPrefs.SetFloat(VOLUME_KEY, vol);
                PlayerPrefs.Save();
            });
        }
    }

    void Update()
    {
        if (dashImage != null && Input.GetKeyDown(KeyCode.LeftShift))
        {
            if (playerMovement.TryDash())
            {
                if (dashRoutine != null) StopCoroutine(dashRoutine);
                dashRoutine = StartCoroutine(CooldownRoutine(dashImage));
            }
        }

        if (attackImage != null && Input.GetKeyDown(KeyCode.Z))
        {
            if (playerSlash.TrySlash())
            {
                if (attackRoutine != null) StopCoroutine(attackRoutine);
                attackRoutine = StartCoroutine(CooldownRoutine(attackImage));
            }
        }

        AtualizaQuantidadeDeCoracoes();
        AtualizaBossBar();
    }

    public void OnDashButton()
    {
        if (playerMovement.TryDash())
        {
            if (dashRoutine != null) StopCoroutine(dashRoutine);
            dashRoutine = StartCoroutine(CooldownRoutine(dashImage));
        }
    }

    public void OnAttackButton()
    {
        if (playerSlash.TrySlash())
        {
            if (attackRoutine != null) StopCoroutine(attackRoutine);
            attackRoutine = StartCoroutine(CooldownRoutine(attackImage));
        }
    }

    private IEnumerator CooldownRoutine(Image img)
    {
        img.fillAmount = 0f;
        float t = 0f;
        while (t < cooldownFillDuration)
        {
            t += Time.deltaTime;
            img.fillAmount = Mathf.Clamp01(t / cooldownFillDuration);
            yield return null;
        }
        img.fillAmount = 1f;
    }

    private void AtualizaQuantidadeDeCoracoes()
    {
        int current = playerHealth.health;
        if (hearts.Count < current)
        {
            for (int i = hearts.Count; i < current; i++)
            {
                var go = Instantiate(heartPrefab, heartsParent);
                hearts.Add(go.GetComponent<Image>());
            }
        }
        else if (hearts.Count > current)
        {
            for (int i = hearts.Count - 1; i >= current; i--)
            {
                Destroy(hearts[i].gameObject);
                hearts.RemoveAt(i);
            }
        }
    }

    private void AtualizaBossBar()
    {
        if (boss == null || bossFillImage == null) return;

        if (!maxHealthInitialized)
        {
            if (towerSpawner != null && towerSpawner.instancias.Count > 0)
            {
                maxTotalHealth = towerSpawner.instancias
                    .Where(t => t.GetComponent<Health>() != null)
                    .Sum(t => t.GetComponent<Health>().maxHealth);
                maxHealthInitialized = true;
            }
            else if (bossHealth != null)
            {
                maxTotalHealth = bossHealth.maxHealth;
                maxHealthInitialized = true;
            }
            else if (bossController != null)
            {
                maxTotalHealth = bossController.maxHealth;
                maxHealthInitialized = true;
            }
        }

        int current = 0;
        if (towerSpawner != null && maxHealthInitialized)
            current = towerSpawner.instancias
                .Where(t => t.GetComponent<Health>() != null)
                .Sum(t => (int)currentHealthField.GetValue(t.GetComponent<Health>()));
        else if (bossHealth != null)
            current = (int)currentHealthField.GetValue(bossHealth);
        else if (bossController != null)
            current = bossController.currentHealth;

        bossFillImage.fillAmount = Mathf.Clamp01(
            maxTotalHealth > 0 ? (float)current / maxTotalHealth : 0f
        );

        CheckEndGame();
    }

    private void CheckEndGame()
    {
        if (!defeatTriggered && playerHealth.health <= 0)
        {
            defeatTriggered = true;
            StartCoroutine(ShowDefeatAfterDelay());
        }

        if (!victoryTriggered && eh_finalBoss)
        {
            bool victory = false;
            if (towerSpawner != null && maxHealthInitialized)
            {
                int sum = towerSpawner.instancias
                    .Where(t => t.GetComponent<Health>() != null)
                    .Sum(t => (int)currentHealthField.GetValue(t.GetComponent<Health>()));
                victory = sum <= 0;
            }
            else if (bossHealth != null)
                victory = (int)currentHealthField.GetValue(bossHealth) <= 0;
            else if (bossController != null)
                victory = bossController.currentHealth <= 0;

            if (victory)
            {
                victoryTriggered = true;
                StartCoroutine(ShowVictoryAfterDelay());
            }
        }
    }

    private IEnumerator ShowDefeatAfterDelay()
    {
        yield return new WaitForSeconds(4f);
        Time.timeScale = 0f;
        playerDeathCount++; // incrementa o contador de mortes na sessão
        endGamePanelDefeat.SetActive(true);
        if (btnReiniciar != null)
            btnReiniciar.SetActive(playerDeathCount == 1); // Só mostra na primeira morte
        GameManager.Instance.hasSlashAbility = false;
    }

    private IEnumerator ShowVictoryAfterDelay()
    {
        yield return new WaitForSeconds(5f);
        Time.timeScale = 0f;
        endGamePanelVictory.SetActive(true);
    }

    public void OnBtnReiniciar()
    {
        endGamePanelDefeat.SetActive(false);
        adScreen.SetActive(true);
        adScreenActive = true;
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
        pausePanel?.SetActive(isPaused);
    }

    public void ResetDefeatFlag()
    {
        defeatTriggered = false;
    }

    public void ShowControls() => controlsPopup?.SetActive(true);
    public void HideControls() => controlsPopup?.SetActive(false);
    public void ShowSettings() => settingsPopup?.SetActive(true);
    public void HideSettings() => settingsPopup?.SetActive(false);

    public void QuitGame()
    {
        Time.timeScale = 1f;
        playerDeathCount = 0; // Reseta o contador ao voltar para o menu principal
        GameManager.Instance.playerHealth = 5;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void HoverPause_Enter() => pauseLight?.SetActive(true);
    public void HoverPause_Exit() => pauseLight?.SetActive(false);
    public void HoverPauseClose_Enter() => pauseCloseLight?.SetActive(true);
    public void HoverPauseClose_Exit() => pauseCloseLight?.SetActive(false);
    public void HoverControlsPause_Enter() => controlsPauseLight?.SetActive(true);
    public void HoverControlsPause_Exit() => controlsPauseLight?.SetActive(false);
    public void HoverConfigPause_Enter() => configPauseLight?.SetActive(true);
    public void HoverConfigPause_Exit() => configPauseLight?.SetActive(false);
    public void HoverExitPause_Enter() => exitPauseLight?.SetActive(true);
    public void HoverExitPause_Exit() => exitPauseLight?.SetActive(false);
    public void HoverControlsClose_Enter() => controlsCloseLight?.SetActive(true);
    public void HoverControlsClose_Exit() => controlsCloseLight?.SetActive(false);
    public void HoverSettingsClose_Enter() => settingsCloseLight?.SetActive(true);
    public void HoverSettingsClose_Exit() => settingsCloseLight?.SetActive(false);
}
