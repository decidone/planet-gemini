using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class FluidManager : NetworkBehaviour
{
    public float sendDelayTimer;
    public float sendDelayInterval;

    Dictionary<FluidFactoryCtrl, List<FluidFactoryCtrl>> mainSourceGroupObj = new Dictionary<FluidFactoryCtrl, List<FluidFactoryCtrl>>();   // 메인소스가 존재하는 경우
    Dictionary<FluidFactoryCtrl, List<FluidFactoryCtrl>> consumeSourceGroupObj = new Dictionary<FluidFactoryCtrl, List<FluidFactoryCtrl>>();// 소모소스만 존재하는 경우
    Coroutine sendfluidCoroutine;
    Coroutine getfluidCoroutine;
    #region Singleton
    public static FluidManager instance;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }
    #endregion

    void Update()
    {
        if (mainSourceGroupObj.Count > 0 || consumeSourceGroupObj.Count > 0)
        {
            sendDelayTimer += Time.deltaTime;
            if (sendDelayTimer > sendDelayInterval)
            {
                SendFluid();
                GetFluid();
                FluidSync();
                sendDelayTimer = 0;
            }
        }
    }

    #region MainSourceGroupControl
    public void MainSourceGroupAdd(FluidFactoryCtrl mainFluid, FluidFactoryCtrl fluid = null)
    {
        if (!mainSourceGroupObj.ContainsKey(mainFluid))
        {
            mainSourceGroupObj.Add(mainFluid, new List<FluidFactoryCtrl>());
        }
        if (fluid && !mainSourceGroupObj[mainFluid].Contains(fluid))
        {
            mainSourceGroupObj[mainFluid].Add(fluid);
        }
    }

    public void MainSourceGroupListRemove(FluidFactoryCtrl mainFluid, FluidFactoryCtrl fluid = null)
    {
        if (mainSourceGroupObj.ContainsKey(mainFluid) && mainSourceGroupObj[mainFluid].Contains(fluid))
        {
            mainSourceGroupObj[mainFluid].Remove(fluid);
        }
    }

    public void MainSourceGroupRemove(FluidFactoryCtrl mainFluid, FluidFactoryCtrl fluid)
    {
        if(sendfluidCoroutine != null)
            StopCoroutine(sendfluidCoroutine);
        if (mainSourceGroupObj.ContainsKey(mainFluid))
        {
            if(mainSourceGroupObj[mainFluid].Contains(fluid) || mainFluid == fluid)
            {
                List<FluidFactoryCtrl> clearList = new List<FluidFactoryCtrl> (mainSourceGroupObj[mainFluid]);
                foreach (FluidFactoryCtrl fluidList in clearList)
                {
                    fluidList.ResetSource();
                }
            }

            if(mainFluid == fluid)
                mainSourceGroupObj.Remove(mainFluid);
            else
                mainSourceGroupObj[mainFluid].Clear();
        }
    }

    void SendFluid()
    {
        foreach (var group in mainSourceGroupObj)
        {
            if(group.Key.outObj.Count > 0)
            {
                group.Key.SendFluid();
                sendfluidCoroutine = StartCoroutine(SendFluidCoroutine(group.Key));
            }
        }
    }

    IEnumerator SendFluidCoroutine(FluidFactoryCtrl mainFluid)
    {
        yield return null;

        List<FluidFactoryCtrl> reversedList = new List<FluidFactoryCtrl>();
        for (int i = mainSourceGroupObj[mainFluid].Count - 1; i >= 0; i--)
        {
            reversedList.Add(mainSourceGroupObj[mainFluid][i]);
        }

        foreach (FluidFactoryCtrl obj in reversedList)
        {
            if (obj)
            {
                obj.SendFluid();
                yield return null;
            }
        }
    }
    #endregion

    #region ConsumeSourceGroupControl
    public void ConsumeSourceGroupAdd(FluidFactoryCtrl consumeFluid, FluidFactoryCtrl fluid = null)
    {
        if (!consumeSourceGroupObj.ContainsKey(consumeFluid))
        {
            consumeSourceGroupObj.Add(consumeFluid, new List<FluidFactoryCtrl>());
        }
        if (fluid && !consumeSourceGroupObj[consumeFluid].Contains(fluid))
        {
            consumeSourceGroupObj[consumeFluid].Add(fluid);
        }
    }

    public void ConsumeSourceGroupListRemove(FluidFactoryCtrl consumeFluid, FluidFactoryCtrl fluid = null)
    {
        if (consumeSourceGroupObj.ContainsKey(consumeFluid) && consumeSourceGroupObj[consumeFluid].Contains(fluid))
        {
            consumeSourceGroupObj[consumeFluid].Remove(fluid);
        }
    }

    public void ConsumeSourceGroupRemove(FluidFactoryCtrl consumeFluid, FluidFactoryCtrl fluid)
    {
        if(getfluidCoroutine != null)
            StopCoroutine(getfluidCoroutine);
        if (consumeSourceGroupObj.ContainsKey(consumeFluid))
        {
            if (consumeSourceGroupObj[consumeFluid].Contains(fluid) || consumeFluid == fluid)
            {
                List<FluidFactoryCtrl> clearList = new List<FluidFactoryCtrl>(consumeSourceGroupObj[consumeFluid]);
                foreach (FluidFactoryCtrl fluidList in clearList)
                {
                    fluidList.ResetSource();
                }
            }

            if (consumeFluid == fluid)
                consumeSourceGroupObj.Remove(consumeFluid);
            else
                consumeSourceGroupObj[consumeFluid].Clear();
        }
    }

    void GetFluid()
    {
        foreach (var group in consumeSourceGroupObj)
        {
            if (group.Key && group.Key.outObj.Count > 0)
            {
                group.Key.ConsumeGroupSendFluid();
                getfluidCoroutine = StartCoroutine(GetFluidCoroutine(group.Key));
            }
        }
    }

    IEnumerator GetFluidCoroutine(FluidFactoryCtrl consumeFluid)
    {
        yield return null;

        List<FluidFactoryCtrl> fluidList = new List<FluidFactoryCtrl>(consumeSourceGroupObj[consumeFluid]);

        foreach (FluidFactoryCtrl obj in fluidList)
        {
            if (obj)
            {
                obj.ConsumeGroupSendFluid();
                yield return null;
            }
        }
    }
    #endregion

    void FluidSync()
    {
        if (IsServer)
        {
            foreach (var mainGroup in mainSourceGroupObj)
            {
                List<FluidFactoryCtrl> fluidList = new List<FluidFactoryCtrl>(mainGroup.Value);

                foreach (FluidFactoryCtrl obj in fluidList)
                {
                    if (obj)
                    {
                        obj.FluidSyncServerRpc();
                    }
                }
            }

            foreach (var consumeGroup in consumeSourceGroupObj)
            {
                List<FluidFactoryCtrl> fluidList = new List<FluidFactoryCtrl>(consumeGroup.Value);

                foreach (FluidFactoryCtrl obj in fluidList)
                {
                    if (obj)
                    {
                        obj.FluidSyncServerRpc();
                    }
                }
            }

        }
    }
}
