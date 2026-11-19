using UnityEngine;
using System;

[RequireComponent(typeof(Collider2D))]
public class ExpGem : MonoBehaviour
{
    private enum GemState { Idle, Tracking }

    [Header("보석 등급별 스프라이트")]
    public Sprite greenSprite; // 소형 (1~5)
    public Sprite blueSprite;  // 중형 (6~30)
    public Sprite redSprite;   // 대형 (31 이상)

    [Header("추적 물리 스펙 (기획 규격)")]
    public float acceleration = 12f;  // 초당 가속도 (+12 m/s²)
    public float maxSpeed = 30f;       // 최대 추적 속도

    private SpriteRenderer spriteRenderer;
    private Collider2D col;
    private int expAmount;
    private Action<ExpGem> onReturnToPool;

    private GemState currentState = GemState.Idle;
    private Transform targetPlayer;
    private float currentSpeed = 0f;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
    }

    public void SpawnSetup(Vector2 spawnPos, int expValue, Action<ExpGem> returnAction)
    {
        transform.position = spawnPos;
        expAmount = expValue;
        onReturnToPool = returnAction;

        currentState = GemState.Idle;
        targetPlayer = null;
        currentSpeed = 0f;
        col.enabled = true;

        UpdateGemVisual(expAmount);
    }

    private void UpdateGemVisual(int exp)
    {
        if (spriteRenderer == null) return;

        if (exp >= 31)
        {
            if (redSprite != null) spriteRenderer.sprite = redSprite;
            else spriteRenderer.color = Color.red;
        }
        else if (exp >= 6)
        {
            if (blueSprite != null) spriteRenderer.sprite = blueSprite;
            else spriteRenderer.color = Color.cyan;
        }
        else
        {
            if (greenSprite != null) spriteRenderer.sprite = greenSprite;
            else spriteRenderer.color = Color.green;
        }
    }

    private void Update()
    {
        // Tracking 상태일 때 플레이어를 향해 등가속도 추적
        if (currentState == GemState.Tracking && targetPlayer != null)
        {
            currentSpeed = Mathf.Min(currentSpeed + (acceleration * Time.deltaTime), maxSpeed);
            Vector3 targetPos = targetPlayer.position;
            transform.position = Vector3.MoveTowards(transform.position, targetPos, currentSpeed * Time.deltaTime);
        }
    }

    // Player의 MagnetRange(자석 반경)에 닿았을 때 호출
    public void StartTracking(Transform playerTransform)
    {
        if (currentState == GemState.Tracking) return;

        targetPlayer = playerTransform;
        currentState = GemState.Tracking;
        currentSpeed = 0f; // 초기 추적 속도 0 m/s
    }

    // 플레이어 본체 피격 콜라이더 접촉 시 획득 처리
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 자석 범위가 아닌 'Player' 본체 태그 접촉 판정
        if (collision.CompareTag("Player"))
        {
            Debug.Log($"[경험치 획득] +{expAmount} EXP");

            LevelManager.Instance.AddExp(expAmount);

            currentState = GemState.Idle;
            col.enabled = false;
            onReturnToPool?.Invoke(this);
        }
    }
}