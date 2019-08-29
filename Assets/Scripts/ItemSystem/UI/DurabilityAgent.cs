using UnityEngine;
using UnityEngine.UI;

public class DurabilityAgent : MonoBehaviour
{
    public Image fill;

    public Text value;

    public void UnShow()
    {
        ZetanUtilities.SetActive(gameObject, false);
    }
}
