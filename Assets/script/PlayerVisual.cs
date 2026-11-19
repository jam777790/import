using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(Animator), typeof(PlayerInputHandler))]
public class PlayerVisual : MonoBehaviour
{
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int DeadHash = Animator.StringToHash("Dead");

    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private PlayerInputHandler inputHandler;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        inputHandler = GetComponent<PlayerInputHandler>();
    }
    private void LateUpdate()
    {
        Vector2 input = inputHandler.MoveInput;

        // 1. 스프라이트 좌우 반전 처리 (직전 시선 유지)
        if (input.x < -0.01f)
        {
            spriteRenderer.flipX = true;
        }
        else if (input.x > 0.01f)
        {
            spriteRenderer.flipX = false;
        }

        // 2. 애니메이션 전환 (입력 벡터의 크기를 Speed 파라미터로 전달)
        animator.SetFloat(SpeedHash, input.magnitude);
    }

    /// <summary>
    /// 향후 플레이어 사망 시 호출될 메서드
    /// </summary>
    public void PlayDead()
    {
        animator.SetTrigger(DeadHash);
    }
}