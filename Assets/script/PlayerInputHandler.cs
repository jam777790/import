using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    // 이동 스크립트와 비주얼 스크립트가 읽어갈 입력 프로퍼티
    public Vector2 MoveInput { get; private set; }

    // 조작 가능 여부 플래그 (레벨업 팝업, 피격 기절 시 활용)
    public bool CanInput { get; set; } = true;

    /// <summary>
    /// Player Input 컴포넌트(Send Messages 방식)의 Move 액션 수신
    /// </summary>
    public void OnMove(InputValue value)
    {
        if (!CanInput)
        {
            MoveInput = Vector2.zero;
            return;
        }

        MoveInput = value.Get<Vector2>();
    }

    public void ResetInput()
    {
        MoveInput = Vector2.zero;
    }
}