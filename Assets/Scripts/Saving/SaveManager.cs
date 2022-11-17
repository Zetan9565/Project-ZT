using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using ZetanStudio.PlayerSystem;

namespace ZetanStudio.SavingSystem
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Zetan Studio/管理器/存档管理器")]
    public class SaveManager : SingletonMonoBehaviour<SaveManager>
    {
        [SerializeField, Label("存档文件名")]
        private string dataName = "SaveData.zdat";

        [SerializeField, Label("16或32字符密钥")]
        private string encryptKey = "zetangamedatazetangamedatazetang";

        public bool IsLoading { get; private set; }

        #region 存档相关
        public bool Save()
        {
            using FileStream fs = Utility.OpenFile(Application.persistentDataPath + "/" + dataName, FileMode.Create);
            try
            {
                BinaryFormatter bf = new BinaryFormatter();

                SaveData data = new SaveData(Application.version);

                SaveMethodAttribute.SaveAll(data);
                bf.Serialize(fs, data);
                Utility.Encrypt(fs, encryptKey);

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
            using FileStream fs = Utility.OpenFile(Application.persistentDataPath + "/" + dataName, FileMode.Open);
            try
            {
                BinaryFormatter bf = new BinaryFormatter();

                SaveData data = bf.Deserialize(Utility.Decrypt(fs, encryptKey)) as SaveData;

                fs.Close();

                IsLoading = true;
                SceneLoader.LoadScene(data.ReadString("sceneName"), () =>
                {
                    GameManager.InitGame(typeof(TriggerSystem.TriggerHolder));
                    LoadPlayer(data);
                    LoadMethodAttribute.LoadAll(data);
                    IsLoading = false;
                });
            }
            catch (Exception ex)
            {
                if (fs != null) fs.Close();
                Debug.LogWarning(ex.Message);
            }
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
            var methods = new List<MethodInfo>(Utility.GetMethodsWithAttribute<SaveMethodAttribute>());
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
            var methods = new List<MethodInfo>(Utility.GetMethodsWithAttribute<LoadMethodAttribute>());
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
}