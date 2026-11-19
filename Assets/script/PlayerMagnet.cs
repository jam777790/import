using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class PlayerMagnet : MonoBehaviour
{
    [Header("자석 흡입 반경")]
    public float magnetRadius = 2.5f;

    private CircleCollider2D magnetCollider;
    private Transform playerRoot;

    private void Awake()
    {
        magnetCollider = GetComponent<CircleCollider2D>();
        magnetCollider.isTrigger = true;
        magnetCollider.radius = magnetRadius;

        // 최상위 플레이어 Transform 캐싱
        playerRoot = transform.parent != null ? transform.parent : transform;
    }

    // 자석 범위 안으로 ExpGem이 들어왔을 때 트리거
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("ExpGem"))
        {
            ExpGem gem = collision.GetComponent<ExpGem>();
            if (gem != null)
            {
                gem.StartTracking(playerRoot);
            }
        }
    }

    // 인스펙터 수치 변경 시 콜라이더 크기 실시간 반영
    private void OnValidate()
    {
        if (magnetCollider == null)
            magnetCollider = GetComponent<CircleCollider2D>();

        magnetCollider.radius = magnetRadius;
        magnetCollider.isTrigger = true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, magnetRadius);
    }

    // 하단에 콜라이더 반경 갱신 함수 추가
    public void SetRadius(float newRadius)
    {
        magnetRadius = newRadius;
        if (magnetCollider != null)
        {
            magnetCollider.radius = newRadius;
        }
    }
}