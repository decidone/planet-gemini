using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Production : Structure
{
    [SerializeField]
    protected GameObject ui;

    // ����(��ź, ����), �۾� �ð�, �۾���, ���, ����ǰ, ������ ����
    public virtual void SetRecipe(Recipe recipe) { }
    public virtual void OpenUI() { }
    public virtual void CloseUI() { }
}
