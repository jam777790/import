using UnityEngine;

[CreateAssetMenu(fileName = "NewMonsterData", menuName = "TopDown/MonsterData")]
public class NewMonsterData : ScriptableObject
{
    [Header("기본 정보")]
    public string monsterId;         // 예: "MON_BAT"
    public string monsterName;       // 표시 이름 (예: "박쥐")

    [Header("스탯")]
    public float maxHp;              // 최대 체력
    public float moveSpeed;          // 이동 속도
    public float contactDamage;      // 충돌 데미지
    public int expReward;            // 드랍 경험치

    [Header("비주얼 및 연출")]
    // 이 값을 1.5로 주면 1.5배 커지고, 0.5로 주면 반절로 작아집니다!
    public float scaleMultiplier = 1.0f;

    // 비주얼 스프라이트는 텍스트(CSV)로 경로를 매핑하기 까다로우니
    // 에디터에서 수동으로 드래그 앤 드롭하는 것이 초보자에게 안전합니다.
    public Sprite visualSprite;
}
