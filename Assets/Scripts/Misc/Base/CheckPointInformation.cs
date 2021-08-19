using UnityEngine;

[CreateAssetMenu(fileName = "check point", menuName = "Zetan Studio/其它/位置检查点")]
public class CheckPointInformation : DestinationInformation
{
    [SerializeField]
    private string targetTag = "Player";
    public string TargetTag => targetTag;

    [SerializeField]
    private int layer;
    public int Layer => layer;

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

    public override bool IsValid
    {
        get
        {
            return base.IsValid && !string.IsNullOrEmpty(targetTag);
        }
    }
}

public enum CheckPointTriggerType
{
    Box,
    Circle,
    Capsule
}