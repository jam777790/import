using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class StatUpgradeOption
{
    public StatType statType;
    public string statTitle;
    public string description;
    public float increaseAmount;
    public Sprite icon;
}

public class StatUpgradeManager : MonoBehaviour
{
    public static StatUpgradeManager Instance { get; private set; }

    [Header("스탯 업그레이드 풀")]
    public List<StatUpgradeOption> statOptions = new List<StatUpgradeOption>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // 기본 스탯 옵션 3종 초기 세팅
        InitDefaultOptions();
    }

    private void Start()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.OnLevelUpEvent += HandleLevelUp;
        }
    }

    private void OnDestroy()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.OnLevelUpEvent -= HandleLevelUp;
        }
    }

    private void InitDefaultOptions()
    {
        if (statOptions.Count == 0)
        {
            statOptions.Add(new StatUpgradeOption { statType = StatType.MoveSpeed, statTitle = "신속의 부적", description = "이동 속도가 +0.8 증가합니다.", increaseAmount = 0.8f });
            statOptions.Add(new StatUpgradeOption { statType = StatType.MaxHp, statTitle = "활력의 심장", description = "최대 체력이 +20 증가하고 체력을 회복합니다.", increaseAmount = 20f });
            statOptions.Add(new StatUpgradeOption { statType = StatType.MagnetRange, statTitle = "자석 코어", description = "보석 흡입 반경이 +0.6m 확장됩니다.", increaseAmount = 0.6f });
        }
    }

    private void HandleLevelUp(int level)
    {
        List<StatUpgradeOption> choices = GetRandomStatOptions(3);

        // 콘솔 로그 대신 팝업 UI 호출
        if (UpgradePopupUI.Instance != null)
        {
            UpgradePopupUI.Instance.OpenStatPopup(choices);
        }
    }

    public List<StatUpgradeOption> GetRandomStatOptions(int count = 3)
    {
        List<StatUpgradeOption> pool = new List<StatUpgradeOption>(statOptions);
        List<StatUpgradeOption> selected = new List<StatUpgradeOption>();

        int pickCount = Mathf.Min(count, pool.Count);
        for (int i = 0; i < pickCount; i++)
        {
            int randIndex = Random.Range(0, pool.Count);
            selected.Add(pool[randIndex]);
            pool.RemoveAt(randIndex);
        }

        return selected;
    }

    // 카드 UI에서 클릭 시 호출될 함수
    public void SelectStatUpgrade(StatUpgradeOption option)
    {
        PlayerStats.Instance.UpgradeStat(option.statType, option.increaseAmount);
        LevelManager.Instance.OnUpgradeSelected(); // 연쇄 레벨업 체크 및 게임 재개
    }
}