using System;
using System.Collections.Generic;
using UnityEngine;
using ZetanStudio.CharacterSystem;

[DisallowMultipleComponent]
public class CharacterControlInput : MonoBehaviour
{
    public float actionInputInterval = 0.2f;
    [SerializeField, HideWhenPlaying]
    protected Vector2 startDirection = Vector2.right;

    protected readonly HashSet<string> triggers = new HashSet<string>();
    protected readonly Dictionary<string, ValueType> values = new Dictionary<string, ValueType>();

    public void SetTrigger(string name)
    {
        if (!triggers.Contains(name)) triggers.Add(name);
    }
    public bool ReadTrigger(string name)
    {
        return triggers.Contains(name);
    }
    public void ResetTrigger(string name)
    {
        triggers.Remove(name);
    }
    public void SetValue<T>(string name, T value) where T : struct
    {
        values[name] = value;
    }
    public bool ReadValue<T>(string name, out T value) where T : struct
    {
        value = default;
        return values.TryGetValue(name, out var find) && canConvert(find, out value);

        static bool canConvert(ValueType find, out T result)
        {
            result = default;
            try
            {
                result = (T)(dynamic)find;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
    public T ReadValue<T>(string name)
    {
        try
        {
            if (values.TryGetValue(name, out var find)) return (T)(dynamic)find;
            return default;
        }
        catch
        {
            return default;
        }
    }

    private void Awake()
    {
        SetValue(CharacterInputNames.Instance.Direction, startDirection);
        OnAwake();
    }

    private void Start()
    {
        OnStart();
    }
    private void Update()
    {
        OnUpdate();
    }
    private void LateUpdate()
    {
        triggers.Clear();
        OnLateUpdate();
    }
    private void FixedUpdate()
    {
        OnFixedUpdate();
    }

    protected virtual void OnAwake() { }
    protected virtual void OnStart() { }
    protected virtual void OnUpdate() { }
    protected virtual void OnLateUpdate() { }
    protected virtual void OnFixedUpdate() { }
}