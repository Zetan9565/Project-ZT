using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class WindowButton : MonoBehaviour
{
    public string type;

    public bool openClose = true;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(OpenClose);
    }

    private void OpenClose()
    {
        if (!openClose)
        {
            NewWindowsManager.OpenWindow(type);
        }
        else
        {
            NewWindowsManager.OpenClose(type);
        }
    }
}