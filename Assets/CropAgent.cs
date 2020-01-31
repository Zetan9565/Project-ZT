using System.Collections;
using System.Collections.Generic;
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
        totalDays.text = MCrop.totalGrowthDays.ToString();
        ZetanUtil.SetActive(dryIcon.gameObject, MCrop.Dry);
        ZetanUtil.SetActive(pestIcon.gameObject, MCrop.Pest);
        ZetanUtil.SetActive(matureIcon.gameObject, MCrop.currentStage.HarvestAble);
    }

    public void Clear(bool recycle = false)
    {
        MCrop = null;
        totalDays.text = string.Empty;
        ZetanUtil.SetActive(dryIcon.gameObject, false);
        ZetanUtil.SetActive(pestIcon.gameObject, false);
        ZetanUtil.SetActive(matureIcon.gameObject, false);
        if (recycle) ObjectPool.Instance.Put(gameObject);
    }

    public void DestroyCrop()
    {
        ConfirmManager.Instance.NewConfirm("销毁作物不会有任何产物，确定销毁吗？", delegate
         {
             MCrop.Recycle();
             Clear();
         });
    }
}
