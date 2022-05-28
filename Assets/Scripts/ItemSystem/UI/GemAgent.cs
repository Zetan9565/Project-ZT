using UnityEngine;
using UnityEngine.UI;
using ZetanStudio.Item;
using ZetanStudio.Item.Module;

public class GemAgent : MonoBehaviour
{
    private Item gemstone;

    [SerializeField]
    private Image icon;

    [SerializeField]
    private Text nameText;

    [SerializeField]
    private Text effectText;

    public void Init(Item gem)
    {
        if (gem) return;
        gemstone = gem;
        icon.overrideSprite = gemstone.Icon;
        nameText.text = gemstone.Name;
        //effectText.text = gemstone.Powerup.ToString();
    }

    public void Clear()
    {
        gemstone = null;
        icon.overrideSprite = null;
        nameText.text = "空槽";
        effectText.text = "可以镶嵌宝石";
    }
}
