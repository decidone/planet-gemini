using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletData
{
    public string bulletName;
    public int damage;
    public float fireRate;
    public float range;

    // 생성자
    public BulletData(string name, int dmg, float rate, float r)
    {
        bulletName = name;
        damage = dmg;
        fireRate = rate;
        range = r;
    }
}

public class TwBulletDataManager : MonoBehaviour
{
    public Dictionary<string, BulletData> BulletDic;

    #region Singleton
    public static TwBulletDataManager instance;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of GameManager found!");
            return;
        }

        instance = this;
    }
    #endregion

    private void Start()
    {
        BulletDic = new Dictionary<string, BulletData>();
        SetBulletData(); // 탄환 데이터 초기화

        foreach (var data in BulletDic)
        {
            Debug.Log(data.Key + " : " + data.Value.damage + " : " + data.Value.fireRate + " : " + data.Value.range);
        }
    }

    void SaveBulletData(BulletData data)
    {
        BulletDic.Add(data.bulletName, data);
    }

    void SetBulletData()
    {
        BulletData[] bulletArray = new BulletData[]
        {
        new BulletData("CopperBullet", 5, 5, 5),
        new BulletData("IronBullet", 10, 4, 4),
        new BulletData("SteelBullet", 15, 3, 3),
        new BulletData("ExplosiveBullet", 20, 2, 2),
        new BulletData("ManablastBullet", 25, 1, 1)
        };

        foreach (BulletData data in bulletArray)
        {
            SaveBulletData(data);
        }
    }
}
