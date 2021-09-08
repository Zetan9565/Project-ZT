using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZetanExtends;

public class MultLanText : Text
{
    [SerializeField]
    private string _ID;
    public string ID => _ID;

    [SerializeField]
    private int language;
    public int Language => language;

    [SerializeField]
    private MultLanTextData[] textDatas;
    public MultLanTextData[] TextDatas => textDatas;
}
public class MultLanTextData
{
    public int language;
    public string text;
}