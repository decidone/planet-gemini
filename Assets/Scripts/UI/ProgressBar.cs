using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// UTF-8 설정
public class ProgressBar : MonoBehaviour
{
    public Slider slider;

    public void SetMaxProgress(float progress)
    {
        if (slider.maxValue != progress)
        {
            slider.maxValue = progress;
            slider.value = 0;
        }
    }

    public void SetProgress(float progress)
    {
        slider.value = progress;
    }
}
