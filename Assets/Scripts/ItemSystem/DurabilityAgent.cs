using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DurabilityAgent : MonoBehaviour
{
    public Image fill;

    public Text value;

    public void UnShow()
    {
        MyTools.SetActive(gameObject, false);
    }
}
