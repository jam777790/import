using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(PlayerInputHandler))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Speed Settings")]
    [Tooltip("최고 이동 속도")]
    [SerializeField] private float maxSpeed = 5.0f;

    [Header("Acceleration Tuning (뱁서라이크 최적값)")]
    [Tooltip("출발 가속도 (권장 30~50: 0.1초 내외 즉각 반응)")]
    [Range(10f, 100f)]
    [SerializeField] private float acceleration = 35.0f;

    [Tooltip("정지 감속도 (권장 40~60: 쫀득한 멈춤, 조작 불쾌감 방지)")]
    [Range(10f, 100f)]
    [SerializeField] private float deceleration = 45.0f;

    private Rigidbody2D rb;
    private PlayerInputHandler inputHandler;
    private Vector2 currentVelocity;

    // 단검 발사 방향 지정을 위해 외부에서 참조하는 마지막 이동 방향
    public Vector2 LastMoveDirection { get; private set; } = Vector2.right;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        inputHandler = GetComponent<PlayerInputHandler>();
    }

    private void FixedUpdate()
    {
        Vector2 rawInput = inputHandler.MoveInput;

        // 1. 대각선 속도 뻥튀기 방지 (정규화)
        Vector2 targetDirection = rawInput.sqrMagnitude > 1.0f ? rawInput.normalized : rawInput;

        // 2. 마지막 유효 이동 방향 저장 (단검 발사 각도용)
        if (targetDirection.sqrMagnitude > 0.01f)
        {
            LastMoveDirection = targetDirection;
        }

        // 3. 목표 속도 산출
        Vector2 targetVelocity = targetDirection * maxSpeed;

        // 4. 가속 / 감속 계수 선택
        float rate = (rawInput.sqrMagnitude > 0.01f) ? acceleration : deceleration;

        // 5. 부드러운 속도 보간 (프레임 독립적 물리 연산)
        currentVelocity = Vector2.MoveTowards(
            currentVelocity,
            targetVelocity,
            rate * Time.fixedDeltaTime
        );

        // 6. 물리 엔진에 반영 (Unity 6 표준: linearVelocity)
        rb.linearVelocity = currentVelocity;
    }
        // 하단에 속도 변경 함수 추가
public void SetMaxSpeed(float newSpeed)
    {
        maxSpeed = newSpeed;
    }

}
