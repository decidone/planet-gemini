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
    public Dictionary<string, BulletData> bulletDic;

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
        bulletDic = new Dictionary<string, BulletData>();
        SetBulletData(); // 탄환 데이터 초기화
    }

    void SaveBulletData(BulletData data)
    {
        bulletDic.Add(data.bulletName, data);
    }

    void SetBulletData()
    {
        BulletData[] bulletArray = new BulletData[]
        {
        new BulletData("EnergyBullet", 0, 0, 0, true),
        new BulletData("CopperBullet", 10, 0f, 5, false),
        new BulletData("IronBullet", 17, 0.1f, 4, false),
        new BulletData("SteelBullet", 30, 0.3f, 3, false),
        new BulletData("ExplosiveBullet", 35, 1, 2, true),
        new BulletData("ManablastBullet", 45, 1, 1, true)
        };
        // 이름, 데미지, 공격속도, 범위, 폭발기능 순으로 넣어주면됨
        foreach (BulletData data in bulletArray)
        {
            SaveBulletData(data);
        }
    }
}
