using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
[AddComponentMenu("Zetan Studio/管理器/存档管理器")]
public class SaveManager : SingletonMonoBehaviour<SaveManager>
{
    [SerializeField]
#if UNITY_EDITOR
    [Label("存档文件名")]
#endif
    private string dataName = "SaveData.zdat";

    [SerializeField]
#if UNITY_EDITOR
    [Label("16或32字符密钥")]
#endif
    private string encryptKey = "zetangamedatezetangamdatezetanga";

    public bool IsLoading { get; private set; }

    #region 存档相关
    public bool Save()
    {
        using FileStream fs = ZetanUtility.OpenFile(Application.persistentDataPath + "/" + dataName, FileMode.Create);
        try
        {
            BinaryFormatter bf = new BinaryFormatter();

            SaveData data = new SaveData(Application.version);

            //SaveTime(data);
            //SaveBag(data);
            //SaveStructure(data);
            //SaveMaking(data);
            //SaveWarehouse(data);
            //SaveQuest(data);
            //SaveDialogue(data);
            //SaveTrigger(data);
            //SaveActions(data);
            //SaveMapMark(data);
            SaveMethodAttribute.SaveAll(data);
            bf.Serialize(fs, data);
            ZetanUtility.Encrypt(fs, encryptKey);

            fs.Close();

            MessageManager.Instance.New("保存成功！");
            //Debug.Log("存档版本号：" + data.version);
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogWarning(ex.Message);
            if (fs != null) fs.Close();
            MessageManager.Instance.New("保存失败！");
            return false;
        }
    }

    void SaveWarehouse(SaveData data)
    {
        //foreach (var talker in DialogueManager.Talkers.Values)
        //{
        //    if (talker.Info.IsWarehouseAgent)
        //        data.warehouseDatas.Add(new WarehouseSaveData(talker.TalkerID, talker.warehouse));
        //}
        //foreach (var kvp in FindObjectsOfType<Warehouse>())
        //{
        //    data.warehouseDatas.Add(new WarehouseSaveData(kvp.EntityID, kvp.WData));
        //}
    }

    void SaveActions(SaveData data)
    {
        foreach (var action in ActionStack.ToArray().Reverse())
            data.actionDatas.Add(new ActionSaveData(action));
    }

    void SaveTrigger(SaveData data)
    {
        TriggerManager.SaveData(data);
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
        IsLoading = true;
        AsyncOperation ao = SceneManager.LoadSceneAsync(data.sceneName);
        ao.allowSceneActivation = false;
        yield return new WaitUntil(() => { return ao.progress >= 0.9f; });
        ao.allowSceneActivation = true;
        yield return new WaitUntil(() => { return ao.isDone; });
        GameManager.InitGame(typeof(TriggerHolder));

        LoadPlayer(data);
        yield return new WaitUntil(() => { return BackpackManager.Instance.Inventory != null; });
        //LoadBackpack(data);
        //LoadStructure(data);
        //LoadMaking(data);
        //LoadWarehouse(data);
        //LoadQuest(data);
        //LoadDialogue(data);
        //LoadMapMark(data);
        //LoadActions(data);
        //LoadTrigger(data);
        //LoadTime(data);
        LoadMethodAttribute.LoadAll(data);
        IsLoading = false;
    }

    void LoadPlayer(SaveData data)
    {
        //PlayerInfoManager.Instance.SetPlayerInfo(new PlayerInformation());
        //TODO 读取玩家信息
        PlayerManager.Instance.PlayerTransform.position = new Vector3(data.playerPosX, data.playerPosY, data.playerPosZ);
    }

    void LoadWarehouse(SaveData data)
    {
        //Warehouse[] warehouses = FindObjectsOfType<Warehouse>();
        //foreach (WarehouseSaveData wd in data.warehouseDatas)
        //{
        //    WarehouseData warehouse = null;
        //    DialogueManager.Talkers.TryGetValue(wd.handlerID, out TalkerData handler);
        //    if (handler) warehouse = handler.warehouse;
        //    else
        //    {
        //        Warehouse wagent = Array.Find(warehouses, x => x.EntityID == wd.handlerID);
        //        if (wagent) warehouse = wagent.WData;
        //    }
        //    if (warehouse != null)
        //    {
        //        warehouse.size = new ScopeInt(wd.maxSize) { Current = wd.currentSize };
        //        warehouse.Items.Clear();
        //        foreach (ItemSaveData id in wd.itemDatas)
        //        {
        //            ItemInfo newInfo = new ItemInfo(ItemUtility.GetItemByID(id.itemID), id.amount)
        //            {
        //                indexInGrid = id.indexInGrid
        //            };
        //            //TODO 把newInfo的耐久度等信息处理
        //            warehouse.Items.Add(newInfo);
        //        }
        //    }
        //}
    }

    void LoadActions(SaveData data)
    {
        var actions = FindObjectsOfType<ActionExecutor>();
        foreach (var ad in data.actionDatas)
            foreach (var action in actions)
                if (action.ID == ad.ID)
                    ActionStack.Push(action, (ActionType)ad.actionType, ad.endDelayTime, ad.executionTime);
    }

    void LoadTrigger(SaveData data)
    {
        TriggerManager.LoadData(data);
    }

    void LoadMapMark(SaveData data)
    {
        MapManager.Instance.LoadData(data);
    }
    #endregion
}

[AttributeUsage(AttributeTargets.Method)]
public class SaveMethodAttribute : Attribute
{
    public readonly int priority;

    public SaveMethodAttribute(int priority = 0)
    {
        this.priority = priority;
    }

    public static void SaveAll(SaveData data)
    {
        var methods = new List<MethodInfo>(ZetanUtility.GetMethodsWithAttribute<SaveMethodAttribute>());
        methods.Sort((x, y) =>
        {
            var attrx = x.GetCustomAttribute<SaveMethodAttribute>();
            var attry = y.GetCustomAttribute<SaveMethodAttribute>();
            if (attrx.priority < attry.priority)
                return -1;
            else if (attrx.priority > attry.priority)
                return 1;
            return 0;
        });
        foreach (var method in methods)
        {
            try
            {
                method.Invoke(null, new object[] { data });
            }
            catch { }
        }
    }
}
[AttributeUsage(AttributeTargets.Method)]
public class LoadMethodAttribute : Attribute
{
    public readonly int priority;

    public LoadMethodAttribute(int priority = 0)
    {
        this.priority = priority;
    }

    public static void LoadAll(SaveData data)
    {
        var methods = new List<MethodInfo>(ZetanUtility.GetMethodsWithAttribute<LoadMethodAttribute>());
        methods.Sort((x, y) =>
        {
            var attrx = x.GetCustomAttribute<LoadMethodAttribute>();
            var attry = y.GetCustomAttribute<LoadMethodAttribute>();
            if (attrx.priority < attry.priority)
                return -1;
            else if (attrx.priority > attry.priority)
                return 1;
            return 0;
        });
        foreach (var method in methods)
        {
            try
            {
                method.Invoke(null, new object[] { data });
            }
            catch { }
        }
    }
}

namespace ZetanStudio.Serialization
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class PloyListConverter<T> : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            List<T> values = new List<T>();
            try
            {
                foreach (var item in JObject.Load(reader).Properties())
                {
                    values.Add((T)item.Value.ToObject(Type.GetType(item.Name)));
                }
            }
            catch { }
            return values;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var list = (List<T>)value;
            var obj = new JObject();
            foreach (var item in list)
            {
                obj.Add(item.GetType().AssemblyQualifiedName, JToken.FromObject(item));
            }
            serializer.Serialize(writer, obj);
        }
    }
    public class PloyArrayConverter<T> : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            List<T> values = new List<T>();
            try
            {
                foreach (var item in JObject.Load(reader).Properties())
                {
                    values.Add((T)item.Value.ToObject(Type.GetType(item.Name)));
                }
            }
            catch { }
            return values.ToArray();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var list = (T[])value;
            var obj = new JObject();
            foreach (var item in list)
            {
                obj.Add(item.GetType().AssemblyQualifiedName, JToken.FromObject(item));
            }
            serializer.Serialize(writer, obj);
        }
    }
    public class PloyConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            try
            {
                foreach (var item in JObject.Load(reader).Properties())
                {
                    return item.Value.ToObject(Type.GetType(item.Name));
                }
            }
            catch
            {
                return null;
            }
            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, new JObject() { { value.GetType().AssemblyQualifiedName, JToken.FromObject(value) } });
        }
    }
}