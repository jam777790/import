using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct WeaponLevelSpec
{
    public int level;
    public float damage;
    public float coolTime;
    public float rangeOrSpeed;
    public int count;
    [TextArea] public string description;
}

[CreateAssetMenu(fileName = "NewWeaponData", menuName = "TopDown/WeaponData")]
public class WeaponData : ScriptableObject
{
    public string weaponId;          // 예: "WPN_KNIFE", "WPN_AURA"
    public string weaponName;        // 표시 이름 (예: "단검", "성수")
    public Sprite icon;              // 카드 UI 아이콘
    public int maxLevel = 3;         // 최대 레벨
    public int baseWeight = 100;     // 추첨 기본 가중치
    public List<WeaponLevelSpec> levelSpecs = new List<WeaponLevelSpec>(); // 레벨별 상세 수치
}