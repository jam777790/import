using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UpgradeCardUI : MonoBehaviour
{
    [Header("UI 컴포넌트 연결")]
    public Image iconImage;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descText;
    public Button selectButton;

    private Action onClickAction;

    private void Awake()
    {
        if (selectButton != null)
        {
            selectButton.onClick.AddListener(OnClickCard);
        }
    }

    // 1. 레벨업 스탯 카드 바인딩
    public void SetupStatCard(StatUpgradeOption option, Action selectCallback)
    {
        titleText.text = option.statTitle;
        descText.text = option.description;

        if (option.icon != null && iconImage != null)
        {
            iconImage.sprite = option.icon;
            iconImage.enabled = true;
        }
        else if (iconImage != null)
        {
            iconImage.enabled = false;
        }

        onClickAction = selectCallback;
    }

    // 2. 보물상자 무기 카드 바인딩
    public void SetupWeaponCard(UpgradeOption option, Action selectCallback)
    {
        titleText.text = option.title;
        descText.text = option.description;

        if (option.icon != null && iconImage != null)
        {
            iconImage.sprite = option.icon;
            iconImage.enabled = true;
        }
        else if (iconImage != null)
        {
            iconImage.enabled = false;
        }

        onClickAction = selectCallback;
    }

    private void OnClickCard()
    {
        onClickAction?.Invoke();
    }
}