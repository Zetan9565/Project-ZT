using UnityEngine;

[CreateAssetMenu(fileName = "field information", menuName = "Zetan Studio/种植/田地信息")]
public class FieldInformation : ScriptableObject
{
    [SerializeField]
    private string _ID;
    public string ID => _ID;

    [SerializeField]
    private string _name;
    public string Name => _name;

    [SerializeField]
    private int humidity = 50;
    public int Humidity => humidity;

    [SerializeField]
    private int capacity = 9;
    public int Capacity => capacity;
}