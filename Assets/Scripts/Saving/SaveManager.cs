using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
[AddComponentMenu("ZetanStudio/管理器/存档管理器")]
public class SaveManager : SingletonMonoBehaviour<SaveManager>
{
    [SerializeField]
#if UNITY_EDITOR
    [DisplayName("存档文件名")]
#endif
    private string dataName = "SaveData.zdat";

    [SerializeField]
#if UNITY_EDITOR
    [DisplayName("16或32字符密钥")]
#endif
    private string encryptKey = "zetangamedatezetangamdatezetanga";

    #region 存档相关
    public bool Save()
    {
        using (FileStream fs = ZetanUtility.OpenFile(Application.persistentDataPath + "/" + dataName, FileMode.Create))
        {
            try
            {
                BinaryFormatter bf = new BinaryFormatter();

                SaveData data = new SaveData();

                SaveBag(data);
                SaveBuilding(data);
                SaveMaking(data);
                SaveWarehouse(data);
                SaveQuest(data);
                SaveDialogue(data);
                SaveTrigger(data);
                SaveActions(data);
                SaveMapMark(data);

                bf.Serialize(fs, data);
                ZetanUtility.Encrypt(fs, encryptKey);

                fs.Close();

                MessageManager.Instance.NewMessage("保存成功！");
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
                Debug.LogWarning(ex.Message);
                if (fs != null) fs.Close();
                MessageManager.Instance.NewMessage("保存失败！");
                return false;
            }
        }
    }

    void SaveBag(SaveData data)
    {
        BackpackManager.Instance.SaveData(data);
    }

    void SaveBuilding(SaveData data)
    {
        BuildingManager.Instance.SaveData(data);
    }

    void SaveMaking(SaveData data)
    {
        MakingManager.Instance.SaveData(data);
    }

    void SaveWarehouse(SaveData data)
    {
        foreach (KeyValuePair<string, TalkerData> kvp in GameManager.TalkerDatas)
        {
            if (kvp.Value.info.IsWarehouseAgent)
            {
                data.warehouseDatas.Add(new WarehouseData(kvp.Value.TalkerID, kvp.Value.warehouse));
            }
        }
        foreach (var kvp in FindObjectsOfType<WarehouseAgent>())
        {
            data.warehouseDatas.Add(new WarehouseData(kvp.ID, kvp.MWarehouse));
        }
    }

    void SaveQuest(SaveData data)
    {
        QuestManager.Instance.SaveData(data);
    }

    void SaveDialogue(SaveData data)
    {
        DialogueManager.Instance.SaveData(data);
    }

    void SaveActions(SaveData data)
    {
        foreach (var action in ActionStack.ToArray().Reverse())
            data.actionDatas.Add(new ActionData(action));
    }

    void SaveTrigger(SaveData data)
    {
        TriggerManager.Instance.SaveData(data);
    }

    void SaveMapMark(SaveData data)
    {
        MapManager.Instance.SaveData(data);
    }
    #endregion

    #region 读档相关
    public void Load()
    {
        using (FileStream fs = ZetanUtility.OpenFile(Application.persistentDataPath + "/" + dataName, FileMode.Open))
        {
            try
            {
                BinaryFormatter bf = new BinaryFormatter();

                SaveData data = bf.Deserialize(ZetanUtility.Decrypt(fs, encryptKey)) as SaveData;

                fs.Close();

                StartCoroutine(LoadAsync(data));
            }
            catch (Exception ex)
            {
                if (fs != null) fs.Close();
                Debug.LogWarning(ex.Message);
            }
        }
    }

    IEnumerator LoadAsync(SaveData data)
    {
        AsyncOperation ao = SceneManager.LoadSceneAsync(data.sceneName);
        ao.allowSceneActivation = false;
        yield return new WaitUntil(() => { return ao.progress >= 0.9f; });
        ao.allowSceneActivation = true;
        yield return new WaitUntil(() => { return ao.isDone; });
        GameManager.InitGame(typeof(TriggerHolder));
        LoadPlayer(data);
        yield return new WaitUntil(() => { return PlayerManager.Instance.Backpack != null; });
        LoadBackpack(data);
        LoadBuilding(data);
        LoadMaking(data);
        LoadWarehouse(data);
        LoadQuest(data);
        LoadDialogue(data);
        LoadMapMark(data);
        LoadActions(data);
        LoadTrigger(data);
    }

    void LoadPlayer(SaveData data)
    {
        //PlayerInfoManager.Instance.SetPlayerInfo(new PlayerInformation());
        //TODO 读取玩家信息
        PlayerManager.Instance.PlayerTransform.position = new Vector3(data.playerPosX, data.playerPosY, data.playerPosZ);
    }

    void LoadBackpack(SaveData data)
    {
        BackpackManager.Instance.LoadData(data.backpackData);
    }

    void LoadBuilding(SaveData data)
    {
        BuildingManager.Instance.Init();
        BuildingManager.Instance.LoadData(data.buildingSystemData);
    }

    void LoadMaking(SaveData data)
    {
        MakingManager.Instance.LoadData(data);
    }

    void LoadWarehouse(SaveData data)
    {
        WarehouseAgent[] warehouseAgents = FindObjectsOfType<WarehouseAgent>();
        foreach (WarehouseData wd in data.warehouseDatas)
        {
            Warehouse warehouse = null;
            GameManager.TalkerDatas.TryGetValue(wd.handlerID, out TalkerData handler);
            if (handler) warehouse = handler.warehouse;
            else
            {
                WarehouseAgent wagent = Array.Find(warehouseAgents, x => x.ID == wd.handlerID);
                if (wagent) warehouse = wagent.MWarehouse;
            }
            if (warehouse != null)
            {
                warehouse.warehouseSize = new ScopeInt(wd.maxSize) { Current = wd.currentSize };
                warehouse.Items.Clear();
                foreach (ItemData id in wd.itemDatas)
                {
                    ItemInfo newInfo = new ItemInfo(GameManager.GetItemByID(id.itemID), id.amount)
                    {
                        indexInGrid = id.indexInGrid
                    };
                    //TODO 把newInfo的耐久度等信息处理
                    warehouse.Items.Add(newInfo);
                }
            }
        }
    }

    void LoadQuest(SaveData data)
    {
        QuestManager.Instance.LoadQuest(data);
    }

    void LoadDialogue(SaveData data)
    {
        DialogueManager.Instance.LoadData(data);
    }

    void LoadActions(SaveData data)
    {
        var actions = FindObjectsOfType<ActionExecutor>();
        foreach (var ad in data.actionDatas)
            foreach (var action in actions)
                if (action.ID == ad.ID)
                    ActionStack.Push(action, (ActionType)ad.actionType, ad.executionTime);
    }

    void LoadTrigger(SaveData data)
    {
        TriggerManager.Instance.LoadData(data);
    }

    void LoadMapMark(SaveData data)
    {
        MapManager.Instance.LoadData(data);
    }
    #endregion
}