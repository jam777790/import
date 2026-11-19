using System.Threading;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class AutoAimProjectile : MonoBehaviour
{
    private float damage;
    private float speed;
    private float knockbackForce;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Launch(Vector2 direction, float dmg, float spd, float force)
    {
        damage = dmg;
        speed = spd;
        knockbackForce = force;

        // Unity 6 최신 물리 이동 표준 적용[cite: 14]
        rb.linearVelocity = direction.normalized * speed;

        // 총알(투사체) 회전
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // 2초 뒤 자동 파괴 (메모리 누수 방지)[cite: 1]
        Destroy(gameObject, 2f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Monster"))
        {
            Monster monster = collision.GetComponent<Monster>();
            if (monster != null)
            {
                monster.TakeDamage(damage, rb.linearVelocity.normalized, knockbackForce);
            }
            // 적중 시 관통 없이 소멸
            Destroy(gameObject);
        }
    }
}
