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

    public List<StructureData> strDataList; // 건물 정보창에서 바로 사전 페이지를 띄워주기 위한 식별용

    public int layout;  // 1: 간단한 설명 텍스트 + 영상, 2: 설명 텍스트 + 레시피
    public VideoClip videoClip;

    public bool canBuy;
    public Building productionBuilding;
    public List<Recipe> recipes;
}
