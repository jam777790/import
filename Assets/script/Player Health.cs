using UnityEngine;
using System;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public float maxHp = 100f;
    public float currentHp;
    private bool isInvincible = false;
    private SpriteRenderer spriteRenderer;

    // 1. 체력 UI 연동용 이벤트 선언 (현재 HP, 최대 HP)
    public event Action<float, float> OnHpChanged;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentHp = maxHp;
    }

    private void Start()
    {
        // 2. 게임 시작 시 현재 체력으로 UI 초기화
        OnHpChanged?.Invoke(currentHp, maxHp);
    }

    public void Heal(float amount)
    {
        currentHp = Mathf.Min(currentHp + amount, maxHp);
        // 3. 체력 회복 시 UI 갱신 호출
        OnHpChanged?.Invoke(currentHp, maxHp);
    }

    public void TakeDamage(float damage)
    {
        currentHp = Mathf.Max(0, currentHp - damage);
        // 4. 피격 시 UI 갱신 호출
        OnHpChanged?.Invoke(currentHp, maxHp);

        Debug.Log($"플레이어 피격! 남은 체력: {currentHp}");

        if (currentHp <= 0)
        {
            Debug.Log("플레이어 사망!");
        }
        else
        {
            StartCoroutine(InvincibilityRoutine());
        }
    }

    private IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        for (int i = 0; i < 5; i++)
        {
            spriteRenderer.color = new Color(1, 1, 1, 0.3f);
            yield return new WaitForSeconds(0.05f);
            spriteRenderer.color = new Color(1, 1, 1, 1f);
            yield return new WaitForSeconds(0.05f);
        }
        isInvincible = false;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Monster") && !isInvincible)
        {
            Monster monster = collision.GetComponent<Monster>();
            if (monster != null && monster.monsterData != null)
            {
                TakeDamage(monster.monsterData.contactDamage);
            }
        }
    }
}