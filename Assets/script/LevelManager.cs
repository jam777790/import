using UnityEngine;
using System;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("레벨 및 경험치 현황")]
    public int currentLevel = 1;
    public int currentExp = 0;
    public int requireExp = 17; // 초기값: Lv.1 -> Lv.2 요구치 (5 + 10 + 2 = 17)

    [Header("상태 플래그")]
    public bool isLevelUpPending = false; // 현재 레벨업 선택창 노출 중 여부

    // UI 갱신용 이벤트: (현재 경험치, 요구 경험치) 전달
    public event Action<int, int> OnExpChanged;
    // 레벨업 발생 이벤트: (달성한 레벨) 전달
    public event Action<int> OnLevelUpEvent;

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
        UpdateRequireExp();
        OnExpChanged?.Invoke(currentExp, requireExp);
    }

    // 기획 공식: RequireExp = 5 + (Level * 10) + (Level^2 * 2)
    private void UpdateRequireExp()
    {
        requireExp = 5 + (currentLevel * 10) + (currentLevel * currentLevel * 2);
    }

    // ExpGem이 플레이어에게 먹혔을 때 호출되는 함수
    public void AddExp(int amount)
    {
        currentExp += amount;
        Debug.Log($"[LevelManager] EXP 획득: +{amount} (현재: {currentExp} / 필요: {requireExp})");

        OnExpChanged?.Invoke(currentExp, requireExp);

        // 경험치가 요구치에 도달했고, 현재 레벨업 창이 떠있지 않다면 레벨업 체크
        if (currentExp >= requireExp && !isLevelUpPending)
        {
            TriggerLevelUp();
        }
    }

    // 레벨업 발동 및 일시정지 처리
    private void TriggerLevelUp()
    {
        isLevelUpPending = true;

        // 1. 경험치 이월(Carry-over) 및 레벨 증가
        currentExp -= requireExp;
        currentLevel++;
        UpdateRequireExp();

        Debug.Log($"<color=yellow>★ LEVEL UP! 현재 레벨: {currentLevel} ★</color>");

        // 2. 게임 일시정지 (Time.timeScale = 0)
        Time.timeScale = 0f;

        // 3. UI 팝업 요청 알림
        OnLevelUpEvent?.Invoke(currentLevel);

        // 임시 로그 (Step 3~4 카드 팝업 연결 전 테스트용)
        Debug.Log("[LevelManager] 게임이 일시정지되었습니다. 강화 카드 선택 대기 중...");
    }

    // Step 5에서 플레이어가 카드를 골랐을 때 호출할 함수 (연쇄 레벨업 방어)
    public void OnUpgradeSelected()
    {
        isLevelUpPending = false;

        // 잔여 이월 경험치가 다음 요구치를 여전히 초과하는지 체크 (연쇄 레벨업)
        if (currentExp >= requireExp)
        {
            Debug.Log("[LevelManager] 연쇄 레벨업 발동!");
            TriggerLevelUp();
        }
        else
        {
            // 더 이상 레벨업할 경험치가 없으면 게임 정상 재개
            Time.timeScale = 1f;
            OnExpChanged?.Invoke(currentExp, requireExp);
            Debug.Log("[LevelManager] 게임이 재개되었습니다.");
        }
    }

    // 테스트용: 인스펙터 우클릭 메뉴에서 즉시 카드 선택 완료 테스트 가능
    [ContextMenu("Test/선택 완료 및 재개(Resume)")]
    private void TestResume()
    {
        OnUpgradeSelected();
    }
}