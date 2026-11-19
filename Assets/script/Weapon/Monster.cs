using UnityEngine;
using System.Collections;
using System;

[RequireComponent(typeof(Rigidbody2D))]
public class Monster : MonoBehaviour
{
    [Header("몬스터 데이터 (SO)")]
    public NewMonsterData monsterData;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Collider2D col;
    private Transform targetPlayer;

    private float currentHp;
    private bool isKnockbacked = false;
    private Action<Monster> onReturnToPool;
    private Action<Vector2, int> onDropExpGem; //  추가: 보석 드랍 요청 콜백 (위치, 경험치량)

    public event Action OnMonsterDied; // 보스 사망 감지용 이벤트

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        col = GetComponent<Collider2D>();
    }

    //  수정: Spawner로부터 보석 드랍 함수(dropGemAction)도 함께 전달받음
    public void SpawnReset(Vector2 spawnPos, Action<Monster> returnAction, Action<Vector2, int> dropGemAction = null)
    {
        transform.position = spawnPos;
        onReturnToPool = returnAction;
        onDropExpGem = dropGemAction;

        // 프리팹에 미리 할당해 둔 monsterData 기준으로 스탯 초기화
        if (monsterData != null)
        {
            currentHp = monsterData.maxHp;
            transform.localScale = new Vector3(monsterData.scaleMultiplier, monsterData.scaleMultiplier, 1f);
        }

        targetPlayer = GameObject.FindGameObjectWithTag("Player")?.transform;
        isKnockbacked = false;
        col.enabled = true;
        rb.linearVelocity = Vector2.zero;

        if (animator != null)
            animator.Rebind();
    }

    private void FixedUpdate()
    {
        if (isKnockbacked || targetPlayer == null || currentHp <= 0 || monsterData == null) return;

        Vector2 direction = (targetPlayer.position - transform.position).normalized;
        rb.linearVelocity = direction * monsterData.moveSpeed; // SO의 이동속도 사용
        spriteRenderer.flipX = direction.x < 0;

        // 플레이어와 거리가 22m 이상 멀어지면 풀로 자동 회수 (보석이 드랍되지 않음!)
        if (Vector2.Distance(transform.position, targetPlayer.position) > 22f)
        {
            onReturnToPool?.Invoke(this);
        }
    }

    public void TakeDamage(float damage, Vector2 knockbackDir, float knockbackForce)
    {
        if (currentHp <= 0) return;

        currentHp -= damage;
        if (animator != null) animator.SetTrigger("Hit");

        if (currentHp <= 0)
        {
            Die();
        }
        else
        {
            StopCoroutine("KnockbackRoutine");
            StartCoroutine(KnockbackRoutine(knockbackDir, knockbackForce));
        }
    }

    private IEnumerator KnockbackRoutine(Vector2 dir, float force)
    {
        isKnockbacked = true;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(dir * force, ForceMode2D.Impulse);

        yield return new WaitForSeconds(0.2f);

        rb.linearVelocity = Vector2.zero;
        isKnockbacked = false;
    }

    private void Die()
    {
        col.enabled = false;
        rb.linearVelocity = Vector2.zero;

        if (animator != null)
            animator.SetTrigger("Dead");

        //  추가: 사망 시 현재 위치에 SO의 expReward만큼 경험치 보석 드랍!
        if (monsterData != null)
        {
            onDropExpGem?.Invoke(transform.position, monsterData.expReward);
        }

        OnMonsterDied?.Invoke(); // 보스 격퇴 감지 알림

        StartCoroutine(ReturnRoutine(0.5f));

        if (InGameHUD.Instance != null)
        {
            InGameHUD.Instance.AddKillCount();
        }
    }

    private IEnumerator ReturnRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        // 일반 몬스터는 풀로 반환하고, 풀을 안 쓰는 보스(null)는 깔끔하게 파괴
        if (onReturnToPool != null)
            onReturnToPool.Invoke(this);
        else
            Destroy(gameObject);
    }
}