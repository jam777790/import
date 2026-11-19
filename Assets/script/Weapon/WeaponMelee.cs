using UnityEngine;

using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class WeaponMelee : MonoBehaviour
{
    [Header("무기 데이터 (SO)")]
    public WeaponData weaponData;

    [Header("근접 무기 프리팹")]
    public GameObject meleePrefab;

    [Header("무기 스펙 (SO에 의해 자동 갱신됨)")]
    public int currentLevel = 1;
    public float attackCooldown = 1.5f; // 1.5초마다 1번씩 휘두름
    public float damage = 15f;
    public float knockbackForce = 5f;
    public float attackOffset = 0.5f;   // 캐릭터 중심에서 얼마나 떨어져서 휘두를지 거리

    private float timer;
    private PlayerMovement playerMovement;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void Start()
    {
        // 게임 시작 시 SO의 1레벨 스펙으로 초기화
        ApplyLevelSpec(1);
    }

    // 보물상자 카드 선택 시 호출되어 수치를 갱신하는 함수
    public void ApplyLevelSpec(int level)
    {
        currentLevel = level;
        if (weaponData == null || weaponData.levelSpecs == null) return;

        int index = level - 1;
        if (index >= 0 && index < weaponData.levelSpecs.Count)
        {
            WeaponLevelSpec spec = weaponData.levelSpecs[index];
            damage = spec.damage;
            attackCooldown = spec.coolTime;
            Debug.Log($"[WeaponMelee] Lv.{level} 적용 완료 (공격력: {damage}, 쿨타임: {attackCooldown})");
        }
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= attackCooldown)
        {
            Attack();
            timer = 0f;
        }
    }

    private void Attack()
    {
        if (meleePrefab == null) return;

        // 1. 플레이어의 마지막 이동 방향 가져오기
        Vector2 dir = playerMovement.LastMoveDirection;

        // 2. 플레이어 위치 기준 정면으로 offset만큼 떨어진 월드 위치 계산
        Vector3 spawnPos = transform.position + (Vector3)(dir * attackOffset);

        // 3. 무기 생성 (부모를 Player로 설정하여 플레이어의 이동을 실시간으로 추적)
        GameObject meleeObj = Instantiate(meleePrefab, spawnPos, Quaternion.identity, transform);
        MeleeSlash slash = meleeObj.GetComponent<MeleeSlash>();

        if (slash != null)
        {
            // 기존 5개 인자(플레이어 Transform 및 offset 전달) 그대로 유지
            slash.Setup(damage, knockbackForce, dir, transform, attackOffset);
        }
    }
}