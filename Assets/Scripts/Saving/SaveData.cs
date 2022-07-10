using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

[Serializable]
public class SaveData : SaveDataItem
{
    public string version;
    public DateTime saveDate;

    public SaveData(string version)
    {
        this.version = version;
        saveDate = DateTime.Now;
        this["sceneName"] = SceneManager.GetActiveScene().name;
        var playerPos = PlayerManager.Instance.PlayerTransform.position;
        this["playerPosX"] = playerPos.x;
        this["playerPosY"] = playerPos.y;
        this["playerPosZ"] = playerPos.z;
    }
}

[Serializable]
public class SaveDataItem
{
    public object this[string key]
    {
        get
        {
            if (intDict.TryGetValue(key, out var intValue)) return intValue;
            else if (floatDict.TryGetValue(key, out var floatValue)) return floatValue;
            else if (boolDict.TryGetValue(key, out var boolValue)) return boolValue;
            else if (stringDict.TryGetValue(key, out var stringValue)) return stringValue;
            else if (dataDict.TryGetValue(key, out var dataValue)) return dataValue;
            else throw new KeyNotFoundException(key);
        }
        set
        {
            if (value is int intValue) Write(key, intValue);
            else if (value is float floatValue) Write(key, floatValue);
            else if (value is bool boolValue) Write(key, boolValue);
            else if (value is string stringValue) Write(key, stringValue);
            else if (value is SaveDataItem dataValue) Write(key, dataValue);
            else throw new InvalidCastException(key);
        }
    }

    [JsonProperty] private List<int> intList;
    [JsonProperty] private List<float> floatList;
    [JsonProperty] private List<bool> boolList;
    [JsonProperty] private List<string> stringList;
    [JsonProperty] private List<SaveDataItem> dataList;
    [JsonProperty] private Dictionary<string, int> intDict;
    [JsonProperty] private Dictionary<string, float> floatDict;
    [JsonProperty] private Dictionary<string, bool> boolDict;
    [JsonProperty] private Dictionary<string, string> stringDict;
    [JsonProperty] private Dictionary<string, SaveDataItem> dataDict;

    #region 写入列表
    public int Write(int value)
    {
        intList ??= new List<int>();
        intList.Add(value);
        return value;
    }
    public float Write(float value)
    {
        floatList ??= new List<float>();
        floatList.Add(value);
        return value;
    }
    public bool Write(bool value)
    {
        boolList ??= new List<bool>();
        boolList.Add(value);
        return value;
    }
    public string Write(string value)
    {
        stringList ??= new List<string>();
        stringList.Add(value);
        return value;
    }
    public SaveDataItem Write(SaveDataItem value)
    {
        dataList ??= new List<SaveDataItem>();
        dataList.Add(value);
        return value;
    }
    public IEnumerable<int> WriteAll(IEnumerable<int> values)
    {
        intList ??= new List<int>();
        intList.AddRange(values);
        return values;
    }
    public IEnumerable<float> WriteAll(IEnumerable<float> values)
    {
        floatList ??= new List<float>();
        floatList.AddRange(values);
        return values;
    }
    public IEnumerable<bool> WriteAll(IEnumerable<bool> values)
    {
        boolList ??= new List<bool>();
        boolList.AddRange(values);
        return values;
    }
    public IEnumerable<string> WriteAll(IEnumerable<string> values)
    {
        stringList ??= new List<string>();
        stringList.AddRange(values);
        return values;
    }
    public IEnumerable<SaveDataItem> WriteAll(IEnumerable<SaveDataItem> values)
    {
        dataList ??= new List<SaveDataItem>();
        dataList.AddRange(values);
        return values;
    }
    #endregion

    #region 写入字典
    public int Write(string key, int value)
    {
        intDict ??= new Dictionary<string, int>();
        return intDict[key] = value;
    }
    public float Write(string key, float value)
    {
        floatDict ??= new Dictionary<string, float>();
        return floatDict[key] = value;
    }
    public bool Write(string key, bool value)
    {
        boolDict ??= new Dictionary<string, bool>();
        return boolDict[key] = value;
    }
    public string Write(string key, string value)
    {
        stringDict ??= new Dictionary<string, string>();
        return stringDict[key] = value;
    }
    public SaveDataItem Write(string key, SaveDataItem value)
    {
        dataDict ??= new Dictionary<string, SaveDataItem>();
        return dataDict[key] = value;
    }
    #endregion

    #region 按集合读取
    public List<int> ReadIntList() => intList ?? new List<int>();
    public List<float> ReadFloatList() => floatList ?? new List<float>();
    public List<bool> ReadBoolList() => boolList ?? new List<bool>();
    public List<string> ReadStringList() => stringList ?? new List<string>();
    public List<SaveDataItem> ReadDataList() => dataList ?? new List<SaveDataItem>();
    public Dictionary<string, int> ReadIntDict() => intDict ?? new Dictionary<string, int>();
    public Dictionary<string, float> ReadFloatDict() => floatDict ?? new Dictionary<string, float>();
    public Dictionary<string, bool> ReadBoolDict() => boolDict ?? new Dictionary<string, bool>();
    public Dictionary<string, string> ReadStringDict() => stringDict ?? new Dictionary<string, string>();
    public Dictionary<string, SaveDataItem> ReadDataDict() => dataDict ?? new Dictionary<string, SaveDataItem>();
    #endregion

    #region 按名称读取
    public bool TryReadInt(string key, out int value)
    {
        if (intDict != null) return intDict.TryGetValue(key, out value);
        value = default;
        return false;
    }
    public bool TryReadFloat(string key, out float value)
    {
        if (floatDict != null) return floatDict.TryGetValue(key, out value);
        value = default;
        return false;
    }
    public bool TryReadBool(string key, out bool value)
    {
        if (boolDict != null) return boolDict.TryGetValue(key, out value);
        value = default;
        return false;
    }
    public bool TryReadString(string key, out string value)
    {
        if (stringDict != null) return stringDict.TryGetValue(key, out value);
        value = default;
        return false;
    }
    public bool TryReadData(string key, out SaveDataItem value)
    {
        if (dataDict != null) return dataDict.TryGetValue(key, out value);
        value = default;
        return false;
    }

    public int ReadInt(string key) => intDict != null && intDict.TryGetValue(key, out var value) ? value : default;
    public float ReadFloat(string key) => floatDict != null && floatDict.TryGetValue(key, out var value) ? value : default;
    public bool ReadBool(string key) => boolDict != null && boolDict.TryGetValue(key, out var value) ? value : default;
    public string ReadString(string key) => stringDict != null && stringDict.TryGetValue(key, out var value) ? value : default;
    public SaveDataItem ReadData(string key) => dataDict != null && dataDict.TryGetValue(key, out var value) ? value : default;
    #endregion

    #region 按下标读取
    public bool TryReadInt(int index, out int value)
    {
        if (intList != null && index >= 0 && index < intList.Count)
        {
            value = intList[index];
            return true;
        }
        value = default;
        return false;
    }
    public bool TryReadFloat(int index, out float value)
    {
        if (floatList != null && index >= 0 && index < floatList.Count)
        {
            value = floatList[index];
            return true;
        }
        value = default;
        return false;
    }
    public bool TryReadBool(int index, out bool value)
    {
        if (boolList != null && index >= 0 && index < boolList.Count)
        {
            value = boolList[index];
            return true;
        }
        value = default;
        return false;
    }
    public bool TryReadString(int index, out string value)
    {
        if (stringList != null && index >= 0 && index < stringList.Count)
        {
            value = stringList[index];
            return true;
        }
        value = default;
        return false;
    }
    public bool TryReadData(int index, out SaveDataItem value)
    {
        if (dataList != null && index >= 0 && index < dataList.Count)
        {
            value = dataList[index];
            return true;
        }
        value = default;
        return false;
    }

    public int ReadInt(int index) => intList != null && index >= 0 && index < intList.Count ? intList[index] : default;
    public float ReadFloat(int index) => floatList != null && index >= 0 && index < floatList.Count ? floatList[index] : (float)default;
    public bool ReadBool(int index) => boolList != null && index >= 0 && index < boolList.Count ? boolList[index] : default;
    public string ReadString(int index) => stringList != null && index >= 0 && index < stringList.Count ? stringList[index] : default;
    public SaveDataItem ReadData(int index) => dataList != null && index >= 0 && index < dataList.Count ? dataList[index] : default;
    #endregion
}