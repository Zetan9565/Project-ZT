using UnityEngine;

[CreateAssetMenu(fileName = "destination", menuName = "Zetan Studio/任务/辅助位置")]
public class DestinationInformation : ScriptableObject
{
    [SerializeField]
    private string _ID;
    public string ID => _ID;

    [SerializeField]
    private string scene;
    public string Scene => scene;

    [SerializeField]
    private Vector3[] positions = new Vector3[] { Vector3.zero };
    public Vector3[] Positions => positions;

    public virtual bool IsValid => !string.IsNullOrEmpty(_ID) && !string.IsNullOrEmpty(scene);

    public static string GetAutoID(int length = 5)
    {
        string newID = string.Empty;
        CheckPointInformation[] all = Resources.LoadAll<CheckPointInformation>("Configuration");
        int len = (int)Mathf.Pow(10, length);
        for (int i = 0; i < len; i++)
        {
            newID = "POINT" + i.ToString().PadLeft(length, '0');
            if (!System.Array.Exists(all, x => x.ID == newID))
                break;
        }
        return newID;
    }

}