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

    private CropStage stageBef;

    public void Init(Crop crop)
    {
        Clear();
        MCrop = crop;
        MCrop.UI = this;
        MCrop.Data.OnStageChange += UpdateInfo;
        UpdateInfo();
    }

    public void UpdateInfo()
    {
        if (!MCrop) return;
        totalDays.text = MCrop.Data.growthDays.ToString();
        nameText.text = MCrop.Data.Info.name;
        ZetanUtility.SetActive(dryIcon.gameObject, MCrop.Dry);
        ZetanUtility.SetActive(pestIcon.gameObject, MCrop.Pest);
        ZetanUtility.SetActive(matureIcon.gameObject, MCrop.Data.HarvestAble);
    }

    public void UpdateInfo(CropStage stage)
    {
        if(stage!=stageBef)
        {
            stageBef = stage;
            UpdateInfo();
        }
    }

    public void Clear(bool recycle = false)
    {
        MCrop = null;
        nameText.text = string.Empty;
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
             FieldManager.Instance.Remove(MCrop);
         });
    }
}
