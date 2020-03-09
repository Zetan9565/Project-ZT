using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class FloatingJoystick : Joystick
{
    [SerializeField] private bool fade = true;
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 2f;
    [SerializeField] private float startFadeOutDuration = 5f;

    private Image imageHD;
    private Image imageBG;

    protected override void Start()
    {
        base.Start();
        imageHD = handle.GetComponent<Image>();
        imageBG = background.GetComponent<Image>();
        if (fade)
        {
            if(imageHD) imageHD.CrossFadeAlpha(0f, startFadeOutDuration, true);
            if(imageBG) imageBG.CrossFadeAlpha(0f, startFadeOutDuration, true);
        }
        else background.gameObject.SetActive(false);
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        background.anchoredPosition = ScreenPointToAnchoredPosition(eventData.position);
        if (fade)
        {
            if(imageHD) imageHD.CrossFadeAlpha(1f, fadeInDuration, true);
            if(imageBG) imageBG.CrossFadeAlpha(1f, fadeInDuration, true);
        }
        else background.gameObject.SetActive(true);
        base.OnPointerDown(eventData);
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        Stop();
    }

    public override void Stop()
    {
        base.Stop();
        if (fade)
        {
            if (imageHD) imageHD.CrossFadeAlpha(0f, fadeOutDuration, true);
            if (imageBG) imageBG.CrossFadeAlpha(0f, fadeOutDuration, true);
        }
        else background.gameObject.SetActive(false);
    }
}