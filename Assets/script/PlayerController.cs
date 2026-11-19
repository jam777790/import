using UnityEngine;
using UnityEngine.InputSystem; // 신규 Input System 네임스페이스

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5.0f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Vector2 rawInput;      // Input System에서 수신한 원본 벡터
    private Vector2 moveDirection; // 정규화(Normalize)된 실제 이동 벡터

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// Player Input 컴포넌트의 Send Messages 방식 또는 C# Event 연동 메서드
    /// Input Action 이름이 "Move"일 경우 "OnMove"로 자동 호출됩니다.
    /// </summary>
    public void OnMove(InputValue value)
    {
        // 1. 입력 벡터 수신
        rawInput = value.Get<Vector2>();

        // 2. 대각선 속도 뻥튀기 방지 (정규화)
        // 크기가 1을 넘는 대각선 입력일 때만 단축 벡터(길이 1)로 변환
        moveDirection = rawInput.magnitude > 1.0f ? rawInput.normalized : rawInput;

        // 3. 이동 방향에 따른 좌우 반전(Flip) 처리
        if (moveDirection.x < 0)
        {
            spriteRenderer.flipX = true;  // 좌측
        }
        else if (moveDirection.x > 0)
        {
            spriteRenderer.flipX = false; // 우측
        }
    }

    private void FixedUpdate()
    {
        // 4. 물리 기반 이동 적용 (Unity 6 표준: linearVelocity)
        rb.linearVelocity = moveDirection * moveSpeed;
    }
}