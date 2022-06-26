using System.Text;
using UnityEngine;
using UnityEngine.UI;
using ZetanStudio;
using ZetanStudio.ItemSystem;
using ZetanStudio.ItemSystem.Module;

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
        if (!gem)
        {
            Clear();
            return;
        }
        gemstone = gem;
        icon.overrideSprite = gemstone.Icon;
        nameText.text = gemstone.Name;
        if (gem.TryGetModule<AttributeModule>(out var attribute) && attribute.Attributes.Count > 0)
        {
            StringBuilder sb = new StringBuilder(attribute.Attributes[0].ToString());
            for (int i = 1; i < attribute.Attributes.Count; i++)
            {
                sb.Append('/');
                sb.Append(attribute.Attributes[i].ToString());
            }
            effectText.text = sb.ToString();
        }
        else effectText.text = Tr("无属性");
    }

    public void Clear()
    {
        gemstone = null;
        icon.overrideSprite = null;
        nameText.text = Tr("空槽");
        effectText.text = Tr("可以镶嵌宝石");
    }

    private string Tr(string text)
    {
        return LM.Tr(GetType().Name, text);
    }
}
