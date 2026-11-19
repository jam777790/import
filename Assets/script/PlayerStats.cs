using UnityEngine;

public enum StatType
{
    MoveSpeed,
    MaxHp,
    MagnetRange
}

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance { get; private set; }

    [Header("기본 스탯 (Base Stats)")]
    public float baseMoveSpeed = 5.0f;
    public float baseMaxHp = 100f;
    public float baseMagnetRadius = 0.5f;

    [Header("현재 적용 스탯 (Current Stats)")]
    public float currentMoveSpeed;
    public float currentMaxHp;
    public float currentMagnetRadius;

    // 플레이어 내 세부 컴포넌트 참조
    private PlayerMovement playerMovement;
    private PlayerHealth playerHealth;
    private PlayerMagnet playerMagnet;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        playerMovement = GetComponent<PlayerMovement>();
        playerHealth = GetComponent<PlayerHealth>();
        playerMagnet = GetComponentInChildren<PlayerMagnet>();

        // 초기 수치 세팅
        currentMoveSpeed = baseMoveSpeed;
        currentMaxHp = baseMaxHp;
        currentMagnetRadius = baseMagnetRadius;
    }

    private void Start()
    {
        ApplyStats();
    }

    // 각 컴포넌트에 현재 계산된 스탯 적용
    public void ApplyStats()
    {
        if (playerMovement != null)
        {
            // PlayerMovement의 속도 갱신
            playerMovement.SetMaxSpeed(currentMoveSpeed);
        }

        if (playerHealth != null)
        {
            playerHealth.maxHp = currentMaxHp;
        }

        if (playerMagnet != null)
        {
            playerMagnet.SetRadius(currentMagnetRadius);
        }
    }

    // 스탯 강화 처리 함수
    public void UpgradeStat(StatType type, float increaseValue)
    {
        switch (type)
        {
            case StatType.MoveSpeed:
                currentMoveSpeed += increaseValue;
                Debug.Log($"[스탯 강화] 이동속도 +{increaseValue} ➔ 현재: {currentMoveSpeed}");
                break;

            case StatType.MaxHp:
                currentMaxHp += increaseValue;
                // 최대 체력 증가 시 현재 체력도 증가량만큼 회복
                if (playerHealth != null)
                {
                    playerHealth.Heal(increaseValue);
                }
                Debug.Log($"[스탯 강화] 최대 체력 +{increaseValue} ➔ 현재: {currentMaxHp}");
                break;

            case StatType.MagnetRange:
                currentMagnetRadius += increaseValue;
                Debug.Log($"[스탯 강화] 자석 범위 +{increaseValue} ➔ 현재: {currentMagnetRadius}");
                break;
        }

        ApplyStats();
    }
}
