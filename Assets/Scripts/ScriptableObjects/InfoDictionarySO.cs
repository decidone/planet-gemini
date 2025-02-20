using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

[CreateAssetMenu(fileName = "New Info", menuName = "Data/Info")]
public class InfoDictionarySO : ScriptableObject
{
    new public string name = "New Info";
    public int type;    // 0: 시스템, 1: 건물, 2: 유닛, 3: 아이템
    public string explanation;  // 설명

    public int layout;  // 1: 간단한 설명 텍스트 + 영상, 2: 설명 텍스트 + 이미지
    public VideoClip videoClip;
}
