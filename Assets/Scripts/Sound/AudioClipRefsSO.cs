using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class AudioClipRefsSO : ScriptableObject
{
    public AudioClip[] mainSceneBgm;
    public AudioClip[] inGameBgm;
    public AudioClip[] battleBgm;
    public AudioClip[] waveBgm;
    public AudioClip[] structureSfx;
    public AudioClip[] uiSfx;
    public AudioClip[] unitSfx;
}
