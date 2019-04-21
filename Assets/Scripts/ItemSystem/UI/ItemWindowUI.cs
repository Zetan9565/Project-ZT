using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class ItemWindowUI : MonoBehaviour
{
    public CanvasGroup itemWindow;

    public Image icon;

    public Text nameText;

    public Text typeText;

    public Text effectText;

    public Text priceText;
    public Text weightText;

    public Text mulFunTitle;
    public Text mulFunText;

    public GemstoneAgent gemstone_1;
    public GemstoneAgent gemstone_2;

    public Text descriptionText;

    /// <summary>
    /// 耐久度
    /// </summary>
    public DurabilityAgent durability;

    public Button closeButton;

    public Button mulFunButton;

    public Button discardButton;

    public GameObject buttonsArea;

    private void Awake()
    {
#if UNITY_STANDALONE
        MyTools.SetActive(buttonsArea, false);
        MyTools.SetActive(closeButton.gameObject, false);
#elif UNITY_ANDROID
        MyTools.SetActive(buttonsArea, true);
        MyTools.SetActive(closeButton.gameObject, true);
        discardButton.onClick.AddListener(ItemWindowManager.Instance.DiscardShowingItem);
        closeButton.onClick.AddListener(ItemWindowManager.Instance.CloseItemWindow);
#endif
        gemstone_1.Clear();
        gemstone_2.Clear();
    }
}