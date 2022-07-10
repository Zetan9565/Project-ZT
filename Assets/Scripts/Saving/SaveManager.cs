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
        AsyncOperation ao = SceneManager.LoadSceneAsync(data.ReadString("sceneName"));
        ao.allowSceneActivation = false;
        yield return new WaitUntil(() => { return ao.progress >= 0.9f; });
        ao.allowSceneActivation = true;
        yield return new WaitUntil(() => { return ao.isDone; });
        GameManager.InitGame(typeof(TriggerHolder));

        LoadPlayer(data);
        yield return new WaitUntil(() => { return BackpackManager.Instance.Inventory != null; });
        LoadMethodAttribute.LoadAll(data);
        IsLoading = false;
    }

    void LoadPlayer(SaveData data)
    {
        //PlayerInfoManager.Instance.SetPlayerInfo(new PlayerInformation());
        //TODO 读取玩家信息
        PlayerManager.Instance.PlayerTransform.position = new Vector3(data.ReadFloat("playerPosX"), data.ReadFloat("playerPosY"), data.ReadFloat("playerPosZ"));
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