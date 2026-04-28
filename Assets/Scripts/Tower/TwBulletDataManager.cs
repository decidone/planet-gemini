using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletData
{
    public string bulletName;
    public int damage;
    public float fireRate;
    public float range;
    public bool explosion;

    // 생성자
    public BulletData(string name, int dmg, float rate, float r, bool explo)
    {
        bulletName = name;
        damage = dmg;
        fireRate = rate;
        range = r;
        explosion = explo;
    }
}

public class TwBulletDataManager : MonoBehaviour
{
    public Dictionary<string, BulletData> TowerBulletDic;
    public Dictionary<string, BulletData> TankBulletDic;

    #region Singleton
    public static TwBulletDataManager instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }
    #endregion

    private void Start()
    {
        TowerBulletDic = new Dictionary<string, BulletData>();
        TankBulletDic = new Dictionary<string, BulletData>();
        SetBulletData(); // 탄환 데이터 초기화
    }

    void SetBulletData()
    {
        BulletData[] towerBulletArray = new BulletData[]
        { // 타워 기본 데미지 15
            new BulletData("EnergyBullet", 0, 0, 0, true),
            new BulletData("CopperBullet", 15, 0f, 3, false),
            new BulletData("IronBullet", 29, 0.3f, 5, false),
            new BulletData("SteelBullet", 45, 0.3f, 6, false),
            new BulletData("ExplosiveBullet", 25, 4, 3, true),
            new BulletData("ManablastBullet", 35, 7, 3, true)
        };
        // 이름, 데미지, 공격속도, 범위, 폭발기능 순으로 넣어주면됨
        foreach (BulletData data in towerBulletArray)
        {
            TowerBulletDic.Add(data.bulletName, data);
        }

        BulletData[] tankBulletArray = new BulletData[]
        { // 탱크 기본 데미지 40
            new BulletData("CopperBullet", 10, 0, 1.7f, true),
            new BulletData("IronBullet", 20, 0, 2.1f, true),
            new BulletData("SteelBullet", 30, 0, 2.5f, true),
            new BulletData("ExplosiveBullet", 40, 0, 2.9f, true),
            new BulletData("ManablastBullet", 50, 0, 3.3f, true)
        };
        // 기존 데이터를 사용하되 사격 범위 대신 폭발 범위로 사용
        foreach (BulletData data in tankBulletArray)
        {
            TankBulletDic.Add(data.bulletName, data);
        }
    }
}
