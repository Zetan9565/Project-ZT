using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    private static SaveManager instance;
    public static SaveManager Instance
    {
        get
        {
            if (!instance || !instance.gameObject)
                instance = FindObjectOfType<SaveManager>();
            return instance;
        }
    }

    public static bool dontDestroyOnLoadOnce;

#if UNITY_EDITOR
    [DisplayName("存档文件名")]
#endif
    public string dataName = "SaveData.zdat";

#if UNITY_EDITOR
    [DisplayName("16或32字符密钥")]
    public string encrptKey = "zetangamedatezetangamdatezetanga";
#endif

    private void Awake()
    {
        if (!dontDestroyOnLoadOnce)
        {
            DontDestroyOnLoad(this);
            dontDestroyOnLoadOnce = true;
        }
        else
        {
            Destroy(gameObject);
        }
    }


    #region 存档相关
    public bool Save()
    {
        using (FileStream fs = OpenFile(Application.persistentDataPath + "/" + dataName, FileMode.Create))
        {
            try
            {
                BinaryFormatter bf = new BinaryFormatter();

                SaveData data = new SaveData();

                SaveBag(data);
                SaveQuest(data);
                SaveDialogue(data);

                bf.Serialize(fs, data);
                MyTools.Encrypt(fs, encrptKey);

                fs.Close();

                MessageManager.Instance.NewMessage("保存成功！");
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning(ex.Message);
                if (fs != null) fs.Close();
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
                data.backpackData.itemDatas.Add(new ItemData(info, BackpackManager.Instance.GetItemAgentByInfo(info).index));
            }
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
    #endregion

    #region 读档相关
    public void Load()
    {
        using (FileStream fs = OpenFile(Application.persistentDataPath + "/" + dataName, FileMode.Open))
        {
            try
            {
                BinaryFormatter bf = new BinaryFormatter();

                SaveData data = bf.Deserialize(MyTools.Decrypt(fs, encrptKey)) as SaveData;

                fs.Close();

                StartCoroutine(LoadAsync(data));
            }
            catch (System.Exception ex)
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
        GameManager.Instance.Init();
        LoadPlayer(data);
        yield return new WaitUntil(() => { return BackpackManager.Instance.MBackpack != null; });
        LoadBag(data);
        LoadQuest(data);
        LoadDialogue(data);
    }

    void LoadPlayer(SaveData data)
    {
        //PlayerInfoManager.Instance.SetPlayerInfo(new PlayerInfomation());
        //TODO 读取玩家信息
    }

    void LoadBag(SaveData data)
    {
        BackpackManager.Instance.LoadData(data.backpackData);
    }

    void LoadQuest(SaveData data)
    {
        foreach (QuestData questData in data.ongoingQuestDatas)
        {
            HandlingQuestData(questData);
            QuestManager.Instance.UpdateUI();
        }

        foreach (QuestData questData in data.completeQuestDatas)
        {
            Quest quest = HandlingQuestData(questData);
            QuestManager.Instance.CompleteQuest(quest, true);
        }
    }
    Quest HandlingQuestData(QuestData questData)
    {
        QuestGiver questGiver = GameManager.Instance.AllTalker[questData.originalGiverID] as QuestGiver;
        Quest quest = questGiver.QuestInstances.Find(x => x.ID == questData.questID);
        foreach (ObjectiveData od in questData.objectiveDatas)
        {
            foreach (Objective o in quest.Objectives)
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
    #endregion

    FileStream OpenFile(string path, FileMode fileMode, FileAccess fileAccess = FileAccess.ReadWrite)
    {
        try
        {
            return new FileStream(path, fileMode, fileAccess);
        }
        catch
        {
            return null;
        }
    }
}
