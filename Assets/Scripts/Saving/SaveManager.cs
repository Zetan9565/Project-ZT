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
        using (FileStream fs = ZetanUtil.OpenFile(Application.persistentDataPath + "/" + dataName, FileMode.Create))
        {
            try
            {
                BinaryFormatter bf = new BinaryFormatter();

                SaveData data = new SaveData();

                SaveBag(data);
                SaveBuilding(data);
                SaveWarehouse(data);
                SaveQuest(data);
                SaveDialogue(data);
                SaveTrigger(data);

                bf.Serialize(fs, data);
                ZetanUtil.Encrypt(fs, encryptKey);

                fs.Close();

                MessageManager.Instance.NewMessage("保存成功！");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex.Message);
                if (fs != null) fs.Close();
                MessageManager.Instance.NewMessage("保存失败！");
                return false;
            }
        }
    }

    void SaveBag(SaveData data)
    {
        if (BackpackManager.Instance.MBackpack != null)
        {
            data.backpackData.currentSize = (int)BackpackManager.Instance.MBackpack.backpackSize;
            data.backpackData.maxSize = BackpackManager.Instance.MBackpack.backpackSize.Max;
            data.backpackData.currentWeight = (float)BackpackManager.Instance.MBackpack.weightLoad;
            data.backpackData.maxWeightLoad = BackpackManager.Instance.MBackpack.weightLoad.Max;
            data.backpackData.money = BackpackManager.Instance.MBackpack.Money;
            foreach (ItemInfo info in BackpackManager.Instance.MBackpack.Items)
            {
                data.backpackData.itemDatas.Add(new ItemData(info, BackpackManager.Instance.GetItemAgentByInfo(info).indexInGrid));
            }
        }
    }

    void SaveBuilding(SaveData data)
    {
        data.buildingSystemData.learneds = BuildingManager.Instance.BuildingsLearned.Select(x => x.IDStarter).ToArray();
        foreach (Building b in FindObjectsOfType<Building>())
        {
            data.buildingSystemData.buildingDatas.Add(new BuildingData(b));
        }
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
        foreach (Quest quest in QuestManager.Instance.QuestsOngoing)
        {
            data.ongoingQuestDatas.Add(new QuestData(quest));
        }
        foreach (Quest quest in QuestManager.Instance.QuestsComplete)
        {
            data.completeQuestDatas.Add(new QuestData(quest));
        }
    }

    void SaveDialogue(SaveData data)
    {
        foreach (KeyValuePair<string, DialogueData> kvpDialog in DialogueManager.Instance.DialogueDatas)
        {
            data.dialogueDatas.Add(kvpDialog.Value);
        }
    }

    void SaveTrigger(SaveData data)
    {
        foreach (var trigger in TriggerManager.Instance.Triggers)
        {
            data.triggerDatas.Add(new TriggerData(trigger.Key, trigger.Value));
        }
    }
    #endregion

    #region 读档相关
    public void Load()
    {
        using (FileStream fs = ZetanUtil.OpenFile(Application.persistentDataPath + "/" + dataName, FileMode.Open))
        {
            try
            {
                BinaryFormatter bf = new BinaryFormatter();

                SaveData data = bf.Deserialize(ZetanUtil.Decrypt(fs, encryptKey)) as SaveData;

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
        GameManager.Init();
        LoadPlayer(data);
        yield return new WaitUntil(() => { return BackpackManager.Instance.MBackpack != null; });
        LoadBackpack(data);
        LoadBuilding(data);
        LoadWarehouse(data);
        LoadQuest(data);
        LoadDialogue(data);
        LoadTrigger(data);
    }

    void LoadPlayer(SaveData data)
    {
        //PlayerInfoManager.Instance.SetPlayerInfo(new PlayerInformation());
        //TODO 读取玩家信息
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
        QuestManager.Instance.QuestsOngoing.Clear();
        foreach (QuestData questData in data.ongoingQuestDatas)
        {
            HandlingQuestData(questData);
            QuestManager.Instance.UpdateUI();
        }
        QuestManager.Instance.QuestsComplete.Clear();
        foreach (QuestData questData in data.completeQuestDatas)
        {
            Quest quest = HandlingQuestData(questData);
            QuestManager.Instance.CompleteQuest(quest, true);
        }
    }
    Quest HandlingQuestData(QuestData questData)
    {
        TalkerData questGiver = GameManager.TalkerDatas[questData.originalGiverID];
        Quest quest = questGiver.questInstances.Find(x => x.ID == questData.questID);
        foreach (ObjectiveData od in questData.objectiveDatas)
        {
            foreach (Objective o in quest.ObjectiveInstances)
            {
                if (o.runtimeID == od.objectiveID)
                {
                    o.CurrentAmount = od.currentAmount;
                    break;
                }
            }
        }
        QuestManager.Instance.AcceptQuest(quest, true);
        return quest;
    }

    void LoadDialogue(SaveData data)
    {
        DialogueManager.Instance.DialogueDatas.Clear();
        foreach (DialogueData dd in data.dialogueDatas)
        {
            DialogueManager.Instance.DialogueDatas.Add(dd.dialogID, dd);
        }
    }

    void LoadTrigger(SaveData data)
    {
        TriggerManager.Instance.Triggers.Clear();
        foreach (TriggerData td in data.triggerDatas)
        {
            TriggerManager.Instance.SetTrigger(td.triggerName, td.triggerState == (int)TriggerState.On);
        }
    }
    #endregion
}
