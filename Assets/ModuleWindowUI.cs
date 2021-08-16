using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModuleWindowUI : MonoBehaviour
{
    [SerializeField]
    private ModuleWindowUIElement[] UIElements = new ModuleWindowUIElement[0];

    private readonly Dictionary<string, GameObject> elementDict = new Dictionary<string, GameObject>();
    private readonly Dictionary<string, RectTransform> rectElement = new Dictionary<string, RectTransform>();
    private readonly Dictionary<string, Text> textElement = new Dictionary<string, Text>();
    private readonly Dictionary<string, Image> imageElement = new Dictionary<string, Image>();
    private readonly Dictionary<string, Button> buttonElement = new Dictionary<string, Button>();

    public GameObject this[string name] => GetElement(name);

    private void Awake()
    {
        foreach (var ele in UIElements)
        {
            if (ele.IsValid)
            {
                elementDict.Add(ele.name, ele.gameObject);
                var rectTrans = ele.gameObject.GetComponent<RectTransform>();
                if (rectTrans)
                    rectElement.Add(ele.name, rectTrans);
                var text = ele.gameObject.GetComponent<Text>();
                if (text)
                    textElement.Add(ele.name, text);
                var image = ele.gameObject.GetComponent<Image>();
                if (image)
                    imageElement.Add(ele.name, image);
                var button = ele.gameObject.GetComponent<Button>();
                if (button)
                    buttonElement.Add(ele.name, button);
            }
        }
    }

    public GameObject GetElement(string name)
    {
        if (elementDict.TryGetValue(name, out var gameObject))
            return gameObject;
        else
        {
            Debug.LogWarning($"�Ҳ�������Ϊ[{name}]��UI���ѷ��ش�������");
            return this.gameObject;
        }
    }
    public RectTransform GetRectTranstrom(string name)
    {
        if (rectElement.TryGetValue(name, out var rectTrans))
            return rectTrans;
        else
        {
            Debug.LogWarning($"�Ҳ�������Ϊ[{name}]�ľ��α任���ѷ��ش�������");
            return GetComponent<RectTransform>();
        }
    }
    public Text GetText(string name)
    {
        if (textElement.TryGetValue(name, out var text))
            return text;
        else
        {
            Debug.LogWarning($"�Ҳ�������Ϊ[{name}]�����֣��ѷ��ش�������");
            return GetComponent<Text>();
        }
    }
    public Image GetImage(string name)
    {
        if (imageElement.TryGetValue(name, out var image))
            return image;
        else
        {
            Debug.LogWarning($"�Ҳ�������Ϊ[{name}]��ͼ���ѷ��ش�������");
            return GetComponent<Image>();
        }
    }
    public Button GetButton(string name)
    {
        if (buttonElement.TryGetValue(name, out var button))
            return button;
        else
        {
            Debug.LogWarning($"�Ҳ�������Ϊ[{name}]�İ�ť���ѷ��ش�������");
            return GetComponent<Button>();
        }
    }
}

[System.Serializable]
public class ModuleWindowUIElement
{
    public string name;
    public GameObject gameObject;

    public bool IsValid => !string.IsNullOrEmpty(name) && gameObject;
}