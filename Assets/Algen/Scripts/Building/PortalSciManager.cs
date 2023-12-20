using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalSciManager : MonoBehaviour
{
    TempScienceDb scienceDb;
    string[] portalSciName;
    Dictionary<string, bool> portalSciDic = new Dictionary<string, bool>();

    #region Singleton
    public static PortalSciManager instance;

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
        scienceDb = TempScienceDb.instance;
        portalSciName = new string[] { "PortalItemIn", "PortalItemOut", "PortalUnitIn", "PortalUnitOut" };

        for (int i = 0; i < portalSciName.Length; i++)
        {
            if (scienceDb.scienceNameDb.ContainsKey(portalSciName[i]))
            {
                portalSciDic.Add(portalSciName[i], true);
            }
            else
            {
                portalSciDic.Add(portalSciName[i], false);
            }
        }
    }

    public void PortalSciUpgrade(string sciName)
    {
        if (portalSciDic.ContainsKey(sciName))
        {
            portalSciDic[sciName] = true;
        }
    }
}
