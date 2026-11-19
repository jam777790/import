using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class TreasureChest : MonoBehaviour
{
    private Collider2D col;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("<color=yellow>★ [보물상자 습득] 무기 강화 카드 추첨 발동! ★</color>");

            // 1. 게임 일시정지
            Time.timeScale = 0f;

            // 2. Step 3의 무기 3택 추첨 실행
            if (UpgradeManager.Instance != null)
            {
                UpgradeManager.Instance.OpenChestUpgrade();
            }

            // 3. 상자 오브젝트 파괴
            Destroy(gameObject);
        }
    }
}
