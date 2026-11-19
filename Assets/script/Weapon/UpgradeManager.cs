using UnityEngine;
using System.Collections.Generic;

public enum UpgradeType
{
    Weapon,
    Heal // 예외 처리용 소비형 카드 (후보군 부족 시 등장)
}

[System.Serializable]
public class UpgradeOption
{
    public UpgradeType type;
    public WeaponData weaponData;
    public int currentLevel;
    public int nextLevel;
    public string title;
    public string description;
    public Sprite icon;
}

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    [Header("전체 장비 데이터 풀")]
    public List<WeaponData> allWeapons = new List<WeaponData>();

    [Header("장비 슬롯 제한")]
    public int maxWeaponSlots = 4;

    [Header("소비형 회복 카드 기본 에셋 (Fallback용)")]
    public Sprite healIcon;

    // 현재 플레이어가 보유한 무기별 레벨 (Key: weaponId, Value: Level)
    private Dictionary<string, int> playerWeaponLevels = new Dictionary<string, int>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 3주차 기본 지급 무기(단검) 1레벨 등록
        playerWeaponLevels["WPN_KNIFE"] = 1;
    }

    // 보물상자(TreasureChest)를 먹었을 때 호출되는 진입점
    public void OpenChestUpgrade()
    {
        List<UpgradeOption> chosenCards = GetRandomUpgradeOptions(3);

        // 콘솔 로그 대신 팝업 UI 호출
        if (UpgradePopupUI.Instance != null)
        {
            UpgradePopupUI.Instance.OpenWeaponPopup(chosenCards);
        }
    }

    // 가중치 기반 무작위 비복원 3종 추출
    public List<UpgradeOption> GetRandomUpgradeOptions(int count = 3)
    {
        List<UpgradeOption> results = new List<UpgradeOption>();

        // 1단계: 유효 풀 필터링
        List<WeaponData> validPool = new List<WeaponData>();
        int equippedWeaponCount = playerWeaponLevels.Count;

        foreach (WeaponData weapon in allWeapons)
        {
            if (weapon == null) continue;

            int currentLv = GetWeaponLevel(weapon.weaponId);

            // 조건 1: 이미 최대 레벨에 도달한 장비 제외
            if (currentLv >= weapon.maxLevel) continue;

            // 조건 2: 미보유 무기인데 장비 슬롯이 꽉 찼다면 제외
            if (currentLv == 0 && equippedWeaponCount >= maxWeaponSlots) continue;

            validPool.Add(weapon);
        }

        // 2단계: 가중치(Weight) 기반 비복원 추출
        int pickCount = Mathf.Min(count, validPool.Count);
        for (int i = 0; i < pickCount; i++)
        {
            int totalWeight = 0;
            foreach (WeaponData w in validPool)
            {
                totalWeight += Mathf.Max(1, w.baseWeight);
            }

            int randomValue = Random.Range(0, totalWeight);
            int accumulatedWeight = 0;
            WeaponData selectedWeapon = null;

            foreach (WeaponData w in validPool)
            {
                accumulatedWeight += Mathf.Max(1, w.baseWeight);
                if (randomValue < accumulatedWeight)
                {
                    selectedWeapon = w;
                    break;
                }
            }

            if (selectedWeapon != null)
            {
                results.Add(CreateWeaponUpgradeOption(selectedWeapon));
                validPool.Remove(selectedWeapon); // 중복 방지
            }
        }

        // 3단계: 예외 처리 (Fallback Pool) - 후보가 3장 미만일 때 소비형 카드 보충
        while (results.Count < count)
        {
            results.Add(CreateFallbackHealOption());
        }

        return results;
    }

    private UpgradeOption CreateWeaponUpgradeOption(WeaponData weapon)
    {
        int curLv = GetWeaponLevel(weapon.weaponId);
        int targetLv = curLv + 1;

        string desc = "능력치가 강화됩니다.";
        if (weapon.levelSpecs != null && targetLv - 1 < weapon.levelSpecs.Count)
        {
            desc = weapon.levelSpecs[targetLv - 1].description;
        }

        return new UpgradeOption
        {
            type = UpgradeType.Weapon,
            weaponData = weapon,
            currentLevel = curLv,
            nextLevel = targetLv,
            title = curLv == 0 ? $"{weapon.weaponName} (신규)" : weapon.weaponName,
            description = desc,
            icon = weapon.icon
        };
    }

    private UpgradeOption CreateFallbackHealOption()
    {
        return new UpgradeOption
        {
            type = UpgradeType.Heal,
            weaponData = null,
            currentLevel = 0,
            nextLevel = 0,
            title = "맛있는 고기",
            description = "체력을 즉시 30% 회복합니다.",
            icon = healIcon
        };
    }

    public int GetWeaponLevel(string weaponId)
    {
        if (playerWeaponLevels.TryGetValue(weaponId, out int level))
        {
            return level;
        }
        return 0; // 0이면 미보유
    }

    public void LevelUpWeapon(string weaponId)
    {
        if (playerWeaponLevels.ContainsKey(weaponId))
        {
            playerWeaponLevels[weaponId]++;
        }
        else
        {
            playerWeaponLevels[weaponId] = 1;
        }
    }

    public void OnWeaponUpgradeSelected(string weaponId)
    {
        LevelUpWeapon(weaponId);
        int newLevel = GetWeaponLevel(weaponId);


        if (weaponId == "WPN_MELEE")
        {
            WeaponMelee melee = FindFirstObjectByType<WeaponMelee>();
            if (melee != null) melee.ApplyLevelSpec(newLevel);
        }
        else if (weaponId == "WPN_RANGED")
        {
            WeaponRanged ranged = FindFirstObjectByType<WeaponRanged>();
            if (ranged != null) ranged.ApplyLevelSpec(newLevel);
        }

        Time.timeScale = 1f;
        Debug.Log($"[UpgradeManager] '{weaponId}' Lv.{newLevel} 강화 완료. 게임 재개.");
    }
}