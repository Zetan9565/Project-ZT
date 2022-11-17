using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine.SceneManagement;
using ZetanStudio.PlayerSystem;

[Serializable]
public class SaveData : GenericData
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
public class GenericData
{
    public object this[string key]
    {
        get
        {
            if (intDict.TryGetValue(key, out var intValue)) return intValue;
            else if (boolDict.TryGetValue(key, out var boolValue)) return boolValue;
            else if (floatDict.TryGetValue(key, out var floatValue)) return floatValue;
            else if (stringDict.TryGetValue(key, out var stringValue)) return stringValue;
            else if (dataDict.TryGetValue(key, out var dataValue)) return dataValue;
            else throw new KeyNotFoundException(key);
        }
        set
        {
            if (value is int intValue) Write(key, intValue);
            else if (value is bool boolValue) Write(key, boolValue);
            else if (value is float floatValue) Write(key, floatValue);
            else if (value is string stringValue) Write(key, stringValue);
            else if (value is GenericData dataValue) Write(key, dataValue);
            else throw new InvalidCastException(key);
        }
    }

    [JsonProperty] private List<int> intList;
    [JsonProperty] private List<bool> boolList;
    [JsonProperty] private List<float> floatList;
    [JsonProperty] private List<string> stringList;
    [JsonProperty] private List<GenericData> dataList;

    [JsonProperty] private Dictionary<string, int> intDict;
    [JsonProperty] private Dictionary<string, bool> boolDict;
    [JsonProperty] private Dictionary<string, float> floatDict;
    [JsonProperty] private Dictionary<string, string> stringDict;
    [JsonProperty] private Dictionary<string, GenericData> dataDict;

    #region 写入列表
    public int Write(int value)
    {
        intList ??= new List<int>();
        intList.Add(value);
        return value;
    }
    public bool Write(bool value)
    {
        boolList ??= new List<bool>();
        boolList.Add(value);
        return value;
    }
    public float Write(float value)
    {
        floatList ??= new List<float>();
        floatList.Add(value);
        return value;
    }
    public string Write(string value)
    {
        stringList ??= new List<string>();
        stringList.Add(value);
        return value;
    }
    public GenericData Write(GenericData value)
    {
        dataList ??= new List<GenericData>();
        dataList.Add(value);
        return value;
    }

    public T WriteAll<T>(T values) where T : IEnumerable
    {
        if (values is IEnumerable<int> intValues)
        {
            intList ??= new List<int>();
            intList.AddRange(intValues);
        }
        else if (values is IEnumerable<bool> boolValues)
        {
            boolList ??= new List<bool>();
            boolList.AddRange(boolValues);
        }
        else if (values is IEnumerable<float> floatValues)
        {
            floatList ??= new List<float>();
            floatList.AddRange(floatValues);
        }
        else if (values is IEnumerable<string> stringValues)
        {
            stringList ??= new List<string>();
            stringList.AddRange(stringValues);
        }
        else if (values is IEnumerable<GenericData> dataValues)
        {
            dataList ??= new List<GenericData>();
            dataList.AddRange(dataValues);
        }
        else throw new InvalidCastException(values?.GetType().ToString());
        return values;
    }
    #endregion

    #region 写入字典
    public int Write(string key, int value)
    {
        intDict ??= new Dictionary<string, int>();
        return intDict[key] = value;
    }
    public bool Write(string key, bool value)
    {
        boolDict ??= new Dictionary<string, bool>();
        return boolDict[key] = value;
    }
    public float Write(string key, float value)
    {
        floatDict ??= new Dictionary<string, float>();
        return floatDict[key] = value;
    }
    public string Write(string key, string value)
    {
        stringDict ??= new Dictionary<string, string>();
        return stringDict[key] = value;
    }
    public GenericData Write(string key, GenericData value)
    {
        dataDict ??= new Dictionary<string, GenericData>();
        return dataDict[key] = value;
    }
    #endregion

    #region 按集合读取
    public ReadOnlyCollection<int> ReadIntList() => new ReadOnlyCollection<int>(intList ?? new List<int>());
    public ReadOnlyCollection<bool> ReadBoolList() => new ReadOnlyCollection<bool>(boolList ?? new List<bool>());
    public ReadOnlyCollection<float> ReadFloatList() => new ReadOnlyCollection<float>(floatList ?? new List<float>());
    public ReadOnlyCollection<string> ReadStringList() => new ReadOnlyCollection<string>(stringList ?? new List<string>());
    public ReadOnlyCollection<GenericData> ReadDataList() => new ReadOnlyCollection<GenericData>(dataList ?? new List<GenericData>());

    public ReadOnlyDictionary<string, int> ReadIntDict() => new ReadOnlyDictionary<string, int>(intDict ?? new Dictionary<string, int>());
    public ReadOnlyDictionary<string, bool> ReadBoolDict() => new ReadOnlyDictionary<string, bool>(boolDict ?? new Dictionary<string, bool>());
    public ReadOnlyDictionary<string, float> ReadFloatDict() => new ReadOnlyDictionary<string, float>(floatDict ?? new Dictionary<string, float>());
    public ReadOnlyDictionary<string, string> ReadStringDict() => new ReadOnlyDictionary<string, string>(stringDict ?? new Dictionary<string, string>());
    public ReadOnlyDictionary<string, GenericData> ReadDataDict() => new ReadOnlyDictionary<string, GenericData>(dataDict ?? new Dictionary<string, GenericData>());
    #endregion

    #region 按名称读取
    public bool TryReadInt(string key, out int value)
    {
        if (intDict != null) return intDict.TryGetValue(key, out value);
        value = default;
        return false;
    }
    public bool TryReadBool(string key, out bool value)
    {
        if (boolDict != null) return boolDict.TryGetValue(key, out value);
        value = default;
        return false;
    }
    public bool TryReadFloat(string key, out float value)
    {
        if (floatDict != null) return floatDict.TryGetValue(key, out value);
        value = default;
        return false;
    }
    public bool TryReadString(string key, out string value)
    {
        if (stringDict != null) return stringDict.TryGetValue(key, out value);
        value = default;
        return false;
    }
    public bool TryReadData(string key, out GenericData value)
    {
        if (dataDict != null) return dataDict.TryGetValue(key, out value);
        value = default;
        return false;
    }

    public int ReadInt(string key) => intDict != null && intDict.TryGetValue(key, out var value) ? value : default;
    public bool ReadBool(string key) => boolDict != null && boolDict.TryGetValue(key, out var value) ? value : default;
    public float ReadFloat(string key) => floatDict != null && floatDict.TryGetValue(key, out var value) ? value : default;
    public string ReadString(string key) => stringDict != null && stringDict.TryGetValue(key, out var value) ? value : default;
    public GenericData ReadData(string key) => dataDict != null && dataDict.TryGetValue(key, out var value) ? value : default;
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
    public bool TryReadData(int index, out GenericData value)
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
    public bool ReadBool(int index) => boolList != null && index >= 0 && index < boolList.Count ? boolList[index] : default;
    public float ReadFloat(int index) => floatList != null && index >= 0 && index < floatList.Count ? floatList[index] : (float)default;
    public string ReadString(int index) => stringList != null && index >= 0 && index < stringList.Count ? stringList[index] : default;
    public GenericData ReadData(int index) => dataList != null && index >= 0 && index < dataList.Count ? dataList[index] : default;
    #endregion
}