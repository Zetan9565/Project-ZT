using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Field : Building
{
    public List<Crop> Crops { get; } = new List<Crop>();
    public int space;
    public int fertility;
    public int Humidity;

    private new void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && IsBuilt)
        {
            if (FieldManager.Instance.CurrentField != this)
                FieldManager.Instance.CanManage(this);
        }
    }

    private new void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && IsBuilt)
        {
            if (FieldManager.Instance.CurrentField != this)
                FieldManager.Instance.CanManage(this);
        }
    }

    private new void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && IsBuilt)
        {
            if (FieldManager.Instance.CurrentField == this)
                FieldManager.Instance.CannotManage();
        }
    }

    public override void AskDestroy()
    {
        onDestroy?.Invoke();
        ConfirmManager.Instance.NewConfirm("耕地内的作物不会保留，确定退耕吗？",
            BuildingManager.Instance.ConfirmDestroy,
            delegate
            {
                if (IsBuilt && BuildingManager.Instance.ToDestroy == this)
                {
                    BuildingManager.Instance.CannotDestroy();
                }
            });
    }
}
