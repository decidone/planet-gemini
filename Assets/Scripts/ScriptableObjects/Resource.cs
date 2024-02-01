using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "New Resource", menuName = "Data/Resource")]
public class Resource : ScriptableObject
{
    new public string name = "New Resource";
    public string type;         //Ore or Oil
    public float distribution;  //자원 분포
    public float scale;         //자원 청크 사이즈
    public List<Tile> tiles = new List<Tile>();

    public Item item = null;
    public float efficiency;    //채굴 효율 (채굴 쿨다운. 낮을수록 빨리 채굴)
    public int level;           //채굴 가능한 채굴기 레벨. 설원 바이옴의 경우 채굴기에서 따로 배율 적용
    public string biome;        //All or 특정바이옴으로 표시, All에 lake는 포함되지 않음. 전부는 아니지만 2바이옴 이상 필요한 경우 바이옴별로 데이터 생성
    public int value;           //자원의 가치
}
