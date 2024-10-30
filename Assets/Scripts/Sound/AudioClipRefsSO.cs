using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class AudioClipRefsSO : ScriptableObject
{
    public AudioClip[] mainSceneBgm;
    public AudioClip[] dayBgm;
    public AudioClip[] nightBgm;
    public AudioClip[] marketBgm;
    public AudioClip[] battleBgm;
    public AudioClip[] waveBgm;
    public AudioClip[] structureSfx;
    public AudioClip[] uiSfx;
    public AudioClip[] unitSfx;
}
