using UnityEngine;
using UnityEngine.UI;

public class CropAgent : MonoBehaviour
{
    public Crop MCrop { get; private set; }

    public Text nameText;

    public Text totalDays;
    public Image dryIcon;
    public Image pestIcon;
    public Image matureIcon;
    public Button destroyButton;

    public void Init(Crop crop)
    {
        MCrop = crop;
        UpdateInfo();
    }

    public void UpdateInfo()
    {
        //totalDays.text = MCrop.totalGrowthDays.ToString();
        ZetanUtility.SetActive(dryIcon.gameObject, MCrop.Dry);
        ZetanUtility.SetActive(pestIcon.gameObject, MCrop.Pest);
        //ZetanUtility.SetActive(matureIcon.gameObject, MCrop.currentStage.HarvestAble);
    }

    public void Clear(bool recycle = false)
    {
        MCrop = null;
        totalDays.text = string.Empty;
        ZetanUtility.SetActive(dryIcon.gameObject, false);
        ZetanUtility.SetActive(pestIcon.gameObject, false);
        ZetanUtility.SetActive(matureIcon.gameObject, false);
        if (recycle) ObjectPool.Put(gameObject);
    }

    public void DestroyCrop()
    {
        ConfirmManager.Instance.New("销毁作物不会有任何产物，确定销毁吗？", delegate
         {
             if (MCrop) MCrop.Recycle();
             Clear(true);
         });
    }
}
