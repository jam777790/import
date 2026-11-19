using System.Collections;
using System.Threading;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class MeleeSlash : MonoBehaviour
{
    private float damage;
    private float knockbackForce;
    private Vector2 swingDirection;

    // 5개 인자 Setup에서 전달받는 값 (추가된 필드)
    private Transform playerTransform;
    private float offsetDist;

    // WeaponMelee에서 생성 직후 스펙을 전달받음
    public void Setup(float dmg, float force, Vector2 dir, Transform player, float offset)
    {
        damage = dmg;
        knockbackForce = force;
        swingDirection = dir;
        playerTransform = player;
        offsetDist = offset;

        StartCoroutine(SwingRoutine());
    }

    private IEnumerator SwingRoutine()
    {
        float swingDuration = 0.25f; // 휘두르는 데 걸리는 시간 (0.25초)
        float timer = 0f;

        // 플레이어가 바라보는 방향의 기본 각도
        float baseAngle = Mathf.Atan2(swingDirection.y, swingDirection.x) * Mathf.Rad2Deg;

        // 그려주신 그림처럼 180도(직각 위에서 직각 아래로) 휘두르도록 각도 설정
        float startAngle = swingDirection.x >= 0 ? baseAngle + 90f : baseAngle - 90f;
        float endAngle = swingDirection.x >= 0 ? baseAngle - 90f : baseAngle + 90f;

        while (timer < swingDuration)
        {
            timer += Time.deltaTime;
            float t = timer / swingDuration;

            // 시간에 맞춰 시작 각도에서 끝 각도로 부드럽게 회전
            float currentAngle = Mathf.Lerp(startAngle, endAngle, t);
            transform.rotation = Quaternion.Euler(0, 0, currentAngle);

            yield return null;
        }

        // 3주 차 기준 임시 파괴 처리 (풀링은 4주 차)[cite: 14]
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 궤적에 닿은 몬스터 타격
        if (collision.gameObject.layer == LayerMask.NameToLayer("Monster"))
        {
            Monster monster = collision.GetComponent<Monster>();
            if (monster != null)
            {
                monster.TakeDamage(damage, swingDirection, knockbackForce);
            }
        }
    }
}