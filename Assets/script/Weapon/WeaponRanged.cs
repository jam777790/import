using UnityEngine;

public class WeaponRanged : MonoBehaviour
{
    [Header("무기 데이터 (SO)")]
    public WeaponData weaponData;

    [Header("무기 그래픽 및 발사구")]
    public Transform gunPivot;         // 회전할 총 오브젝트 (Gun)
    public SpriteRenderer gunSprite;   // 총이 뒤집히지 않도록 제어할 렌더러
    public Transform firePoint;        // 총알이 튀어나올 총구 위치

    [Header("프리팹 및 세팅 (SO에 의해 자동 갱신됨)")]
    public GameObject bulletPrefab;
    public int currentLevel = 1;
    public float fireCooldown = 0.8f;
    public float damage = 10f;
    public float bulletSpeed = 15f;
    public float knockbackForce = 3f;
    public float detectionRange = 10f; // 타겟 탐색 반경

    private float timer;

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
            fireCooldown = spec.coolTime;
            bulletSpeed = spec.rangeOrSpeed;
            Debug.Log($"[WeaponRanged] Lv.{level} 적용 완료 (공격력: {damage}, 쿨타임: {fireCooldown}, 속도: {bulletSpeed})");
        }
    }

    private void Update()
    {
        timer += Time.deltaTime;
        AimAndFire();
    }

    private void AimAndFire()
    {
        // 1. 반경 내 모든 몬스터 찾기
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, detectionRange, LayerMask.GetMask("Monster"));

        if (hitColliders.Length == 0) return; // 몬스터가 없으면 대기

        // 2. 가장 가까운 몬스터 거리 계산
        Transform closestEnemy = null;
        float minDistance = float.MaxValue;

        foreach (Collider2D coll in hitColliders)
        {
            float dist = Vector2.Distance(transform.position, coll.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closestEnemy = coll.transform;
            }
        }

        // 3. 총구 회전 및 발사
        if (closestEnemy != null && gunPivot != null)
        {
            // 타겟을 향한 방향 벡터 계산
            Vector2 aimDir = (closestEnemy.position - gunPivot.position).normalized;

            // 각도 계산 및 총 회전 적용
            float angle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;
            gunPivot.rotation = Quaternion.Euler(0, 0, angle);

            // 총이 왼쪽을 바라볼 때 위아래가 뒤집혀 보이지 않도록 상하 반전(FlipY) 처리
            if (gunSprite != null)
            {
                gunSprite.flipY = (angle > 90 || angle < -90);
            }

            // 4. 쿨타임이 다 찼을 때 총구(firePoint) 위치에서 총알 발사
            if (timer >= fireCooldown && bulletPrefab != null && firePoint != null)
            {
                // 플레이어 몸통이 아닌 총구 끝에서 생성되도록 firePoint.position 사용
                GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position, gunPivot.rotation);
                AutoAimProjectile projectile = bulletObj.GetComponent<AutoAimProjectile>();

                if (projectile != null)
                {
                    projectile.Launch(aimDir, damage, bulletSpeed, knockbackForce);
                }

                timer = 0f; // 쿨타임 초기화
            }
        }
    }
}   