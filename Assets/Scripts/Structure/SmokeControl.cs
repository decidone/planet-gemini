using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmokeControl : MonoBehaviour
{
    [SerializeField]
    Animator[] smokeAnimators;

    public void SetSmokeActive(bool isActive)
    {
        foreach (var animator in smokeAnimators)
        {
            if (animator != null)
            {
                animator.enabled = isActive;
                animator.gameObject.SetActive(isActive);
            }
        }
    }
}
