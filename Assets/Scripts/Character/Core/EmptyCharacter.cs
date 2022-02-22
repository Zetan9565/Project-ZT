using UnityEngine;

public class EmptyCharacter : Character
{
    [SerializeReference, ReadOnly]
    private CharacterData data;

    public override CharacterData GetData()
    {
        return data;
    }

    public override void SetData(CharacterData value)
    {
        data = value;
    }

    protected override void OnAwake()
    {
        Init(new CharacterData(ScriptableObject.CreateInstance<NPCInformation>()));
    }
}