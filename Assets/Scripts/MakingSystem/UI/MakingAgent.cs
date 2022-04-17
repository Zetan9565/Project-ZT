using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class MakingAgent : ListItem<MakingAgent, ItemBase>
{
    [SerializeField]
    private Text nameText;

    private MakingWindow window;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    public void OnClick()
    {
        if (Data)
        {
            window.ShowDescription(Data);
        }
    }

    public override void Refresh()
    {
        nameText.text = Data.Name;
    }

    public void SetWindow(MakingWindow window)
    {
        this.window = window;
    }

    public override void OnClear()
    {
        base.OnClear();
        nameText.text = string.Empty;
        Data = null;
    }
}
