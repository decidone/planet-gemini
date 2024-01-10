using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoNeedItemUi : MonoBehaviour
{
    public Image icon;
    public Text amount;

    public void AmountSet(int saveAmount, int fullAmount)
    {
        amount.text = saveAmount + " / " + fullAmount;
    }
}
