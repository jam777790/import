using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InGameHUD : MonoBehaviour
{
    public static InGameHUD Instance { get; private set; }

    [Header("경험치 바 (Exp)")]
    public Image expFillImage;
    public TextMeshProUGUI textLevel;

    [Header("타이머 (Timer)")]
    public TextMeshProUGUI textTimer;

    [Header("킬 카운트 (Kill Count)")]
    public TextMeshProUGUI textKillCount;

    [Header("체력 바 (HP)")]
    public Image hpFillImage;
    public TextMeshProUGUI textHp;

    private float elapsedTime = 0f;
    private int killCount = 0;
    private bool isTimerRunning = true;

    private PlayerHealth playerHealth;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // 1. LevelManager 이벤트 구독
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.OnExpChanged += UpdateExpBar;
            LevelManager.Instance.OnLevelUpEvent += UpdateLevelText;

            UpdateLevelText(LevelManager.Instance.currentLevel);
            UpdateExpBar(LevelManager.Instance.currentExp, LevelManager.Instance.requireExp);
        }

        // 2. PlayerHealth 이벤트 구독
        playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.OnHpChanged += UpdateHpBar;
            UpdateHpBar(playerHealth.currentHp, playerHealth.maxHp);
        }
    }

    private void OnDestroy()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.OnExpChanged -= UpdateExpBar;
            LevelManager.Instance.OnLevelUpEvent -= UpdateLevelText;
        }

        if (playerHealth != null)
        {
            playerHealth.OnHpChanged -= UpdateHpBar;
        }
    }

    private void Update()
    {
        // 일시정지 상태(Time.timeScale = 0)에서는 타이머 일시정지
        if (isTimerRunning && Time.timeScale > 0)
        {
            elapsedTime += Time.deltaTime;
            UpdateTimerText();
        }
    }

    private void UpdateTimerText()
    {
        int minutes = Mathf.FloorToInt(elapsedTime / 60F);
        int seconds = Mathf.FloorToInt(elapsedTime % 60F);
        textTimer.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void AddKillCount()
    {
        killCount++;
        textKillCount.text = killCount.ToString();
    }

    private void UpdateExpBar(int currentExp, int requireExp)
    {
        if (requireExp <= 0) return;
        float ratio = Mathf.Clamp01((float)currentExp / requireExp);
        expFillImage.fillAmount = ratio;
    }

    private void UpdateLevelText(int level)
    {
        textLevel.text = $"Lv. {level}";
    }

    private void UpdateHpBar(float currentHp, float maxHp)
    {
        if (maxHp <= 0) return;
        float ratio = Mathf.Clamp01(currentHp / maxHp);
        hpFillImage.fillAmount = ratio;
        textHp.text = $"{Mathf.CeilToInt(currentHp)} / {Mathf.CeilToInt(maxHp)}";
    }

    public void StopTimer() => isTimerRunning = false;
    public float GetElapsedTime() => elapsedTime;
    public int GetKillCount() => killCount;
}