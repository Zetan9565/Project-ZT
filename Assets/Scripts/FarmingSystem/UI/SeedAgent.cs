using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SeedAgent : MonoBehaviour
{
    public Text nameText;

    public SeedItem Seed { get; private set; }

    public void Init(SeedItem seed)
    {
        Seed = seed;
        nameText.text = Seed.name;
    }

    public void Clear(bool recycle=false)
    {
        nameText.text = string.Empty;
        Seed = null;
        if (recycle) ObjectPool.Instance.Put(gameObject);
    }

    public void OnClick()
    {
        PlantManager.Instance.ShowDescription(Seed);
    }
}