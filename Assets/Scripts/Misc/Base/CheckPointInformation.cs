using System;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "check point", menuName = "Zetan Studio/任务/位置检查点")]
public class CheckPointInformation : ScriptableObject
{
    [SerializeField]
    private string _ID;
    public string ID => _ID;

    [SerializeField]
    private string targetTag = "Player";
    public string TargetTag => targetTag;

    [SerializeField]
    private int layer;
    public int Layer => layer;

    [SerializeField]
    private SceneAsset scene;
    public SceneAsset Scene => scene;

    [SerializeField]
    private Vector3[] positions;
    public Vector3[] Positions => positions;

    [SerializeField]
    private CheckPointTriggerType triggerType = CheckPointTriggerType.Box;
    public CheckPointTriggerType TriggerType => triggerType;

    [SerializeField]
    private Vector3 size = Vector3.one;
    public Vector3 Size => size;

    [SerializeField]
    private float radius = 0.5f;
    public float Radius => radius;

    [SerializeField]
    private float height = 2.0f;
    public float Height => height;

    public bool IsValid => !string.IsNullOrEmpty(_ID) && !string.IsNullOrEmpty(targetTag) && scene;

    public static string GetAutoID(int length = 5)
    {
        string newID = string.Empty;
        CheckPointInformation[] all = Resources.LoadAll<CheckPointInformation>("Configuration");
        int len = (int)Mathf.Pow(10, length);
        for (int i = 0; i < len; i++)
        {
            newID = "POINT" + i.ToString().PadLeft(length, '0');
            if (!Array.Exists(all, x => x.ID == newID))
                break;
        }
        return newID;
    }
}

public enum CheckPointTriggerType
{
    Box,
    Circle,
    Capsule
}