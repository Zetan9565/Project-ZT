using UnityEngine;
using UnityEngine.UI;

public class ShowItemInfoTest : MonoBehaviour {

    Text text;

    public ItemBase item;

	// Use this for initialization
	void Start () {
        text = GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update () {
        if (item) text.text = item.Name + ": " + BagManager.Instance.GetItemAmountByID(item.ID);
	}
}
