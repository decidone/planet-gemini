using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Furnace : Production
{
    [SerializeField]
    int maxAmount;
    [SerializeField]
    float cooldown;

    public string recipeUI;
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
            if (timer > cooldown)
            {
                //inventory.Add(item, 1);
                timer = 0;
            }
        }
    }

    void SetRecipe()
    {
        recipeUI = "Furnace";
    }
}
