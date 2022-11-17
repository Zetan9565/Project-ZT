using UnityEngine;
using ZetanStudio.SavingSystem;

public class SaveLoadTestBtn : MonoBehaviour
{
    [Tooltip("True = 存档， False = 读档")]
    public bool saveOrLoad = true;

    public void OnClick()
    {
        if (saveOrLoad) SaveManager.Instance.Save();
        else SaveManager.Instance.Load();
    }
}
