using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Production : Structure
{
    [SerializeField]
    protected GameObject ui;

    // 연료(석탄, 전기), 작업 시간, 작업량, 재료, 생산품, 아이템 슬롯
    public virtual void SetRecipe(Recipe recipe) { }
    public virtual void OpenUI() { }
    public virtual void CloseUI() { }
}
