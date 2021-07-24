using UnityEngine;

[CreateAssetMenu(fileName = "field information", menuName = "ZetanStudio/种植/田地信息")]
public class FieldInformation : ScriptableObject
{
    [SerializeField]
    private string _ID;
    public string ID => _ID;

    [SerializeField]
    private string _name;
    public new string name => _name;

    [SerializeField]
    private int humidity = 50;
    public int Humidity => humidity;

    [SerializeField]
    private int capacity = 9;
    public int Capacity => capacity;
}