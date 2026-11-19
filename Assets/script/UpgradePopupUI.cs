using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class UpgradePopupUI : MonoBehaviour
{
    public static UpgradePopupUI Instance { get; private set; }

    [Header("루트 패널")]
    public GameObject popupPanel; // Popup_Upgrade 자기 자신

    [Header("타이틀 텍스트")]
    public TextMeshProUGUI textTitle;

    [Header("카드 3개 슬롯")]
    public List<UpgradeCardUI> cardSlots = new List<UpgradeCardUI>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // 시작 시 팝업 비활성화
        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
        }
    }

    // 레벨업 시 호출 (스탯 3택)
    public void OpenStatPopup(List<StatUpgradeOption> options)
    {
        popupPanel.SetActive(true);
        if (textTitle != null) textTitle.text = "LEVEL UP!";

        for (int i = 0; i < cardSlots.Count; i++)
        {
            if (i < options.Count)
            {
                cardSlots[i].gameObject.SetActive(true);
                StatUpgradeOption opt = options[i];
                cardSlots[i].SetupStatCard(opt, () => OnSelectStat(opt));
            }
            else
            {
                cardSlots[i].gameObject.SetActive(false);
            }
        }
    }

    // 보물상자 개봉 시 호출 (무기 3택)
    public void OpenWeaponPopup(List<UpgradeOption> options)
    {
        popupPanel.SetActive(true);
        if (textTitle != null) textTitle.text = "TREASURE CHEST!";

        for (int i = 0; i < cardSlots.Count; i++)
        {
            if (i < options.Count)
            {
                cardSlots[i].gameObject.SetActive(true);
                UpgradeOption opt = options[i];
                cardSlots[i].SetupWeaponCard(opt, () => OnSelectWeapon(opt));
            }
            else
            {
                cardSlots[i].gameObject.SetActive(false);
            }
        }
    }

    private void OnSelectStat(StatUpgradeOption option)
    {
        popupPanel.SetActive(false);
        StatUpgradeManager.Instance.SelectStatUpgrade(option);
    }

    private void OnSelectWeapon(UpgradeOption option)
    {
        popupPanel.SetActive(false);

        if (option.type == UpgradeType.Weapon)
        {
            UpgradeManager.Instance.OnWeaponUpgradeSelected(option.weaponData.weaponId);
        }
        else if (option.type == UpgradeType.Heal)
        {
            // Fallback 체력 30% 즉시 회복
            FindFirstObjectByType<PlayerHealth>()?.Heal(30f);
            Time.timeScale = 1f;
        }
    }
}