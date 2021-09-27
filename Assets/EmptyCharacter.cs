using UnityEngine;

public class EmptyCharacter : Character<CharacterData>
{
    protected override void OnAwake()
    {
        Init(new CharacterData(ScriptableObject.CreateInstance<NPCInformation>()));
    }
}