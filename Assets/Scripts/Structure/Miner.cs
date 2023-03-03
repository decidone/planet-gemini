using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Miner : Production
{
    [SerializeField]
    int maxAmount;
    [SerializeField]
    float cooldown;

    public string recipeUI;
    public Item item;   // 임시로 유니티에서 직접 줌. 나중에 관련 기능들을 만들면 setResource에서 처리할 것
    int amount;
    Inventory inventory;
    float timer;

    void Start()
    {
        inventory = this.GetComponent<Inventory>();
        amount = 0;

        // 레시피 설정하는 부분 임시 설정.
        // 나중에 플레이어가 레시피 설정하는 기능이 생기면 해당 메서드는 제거
        SetRecipe();
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (amount < maxAmount)
        {
            // 0.1초마다 입력 상태를 저장
            if (timer > cooldown)
            {
                inventory.Add(item, 1);
                timer = 0;
            }
        }
    }

    void SetResource()
    {
        // 매장된 자원 확인, 생산 자원을 지정
        // 아마 건설할 때 체크 후 그 뒤로는 사용 안 함. 그러면 이름을 setResource로 변경
    }

    void SetRecipe()
    {
        recipeUI = "Miner";
    }
}
