using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class NewItemWindowBaseUI : WindowUI
{
    public Image icon;
    public Text nameText;
    public Text typeText;

    public Transform effectParent;
    public GameObject effectPrefab;

    public Text priceTitle;
    public Text priceText;
    public Text weightText;

    public Transform contentParent;

    public GameObject titlePrefab;
    public GameObject contentPrefab;
    public GameObject attributePrefab;
    public GemstoneAgent gemPrefab;

    public Transform buttonParent;
    public GameObject buttonPrefab;

    public Transform cacheParent;
}