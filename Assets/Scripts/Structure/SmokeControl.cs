using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmokeControl : MonoBehaviour
{
    [SerializeField]
    ShaderAnimController[] shaderAnims;

    public void SetSmokeActive(bool isActive)
    {
        foreach (var anim in shaderAnims)
        {
            if (anim != null)
            {
                //animator.enabled = isActive;
                //animator.gameObject.SetActive(isActive);
                //animator.gameObject.GetComponent<SpriteRenderer>().material = Resources.Load<Material>("Materials/ShaderAnimatedMat");
                anim.gameObject.SetActive(isActive);
                if (isActive)
                {
                    if (!anim.isInitialized)
                        anim.Refresh();
                    else
                        anim.Resume();
                }
                else
                    anim.Pause();
            }
        }
    }
}
