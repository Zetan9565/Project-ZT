using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ProgressBar : SingletonMonoBehaviour<ProgressBar>
{
    [SerializeField]
    private GameObject bar;

    [SerializeField]
    private Image fillArea;

    [SerializeField]
    private Text actionText;

    [SerializeField]
    private Text timeText;

    private bool isProgressing;
    private float targetTime;
    private float currentTime;

    private UnityEvent onDone = new UnityEvent();
    private UnityEvent onCancel = new UnityEvent();

    public void NewProgress(float seconds, UnityAction doneAction, UnityAction cancelAction, string actionName = null)
    {
        if (seconds <= 0) return;
        if (doneAction != null)
        {
            onDone.RemoveAllListeners();
            onDone.AddListener(doneAction);
        }
        if (cancelAction != null)
        {
            onCancel.RemoveAllListeners();
            onCancel.AddListener(cancelAction);
        }
        targetTime = seconds;
        isProgressing = true;
        if (actionText)
        {
            if (string.IsNullOrEmpty(actionName))
                ZetanUtility.SetActive(actionText.gameObject, false);
            else
            {
                actionText.text = actionName;
                ZetanUtility.SetActive(actionText.gameObject, true);
            }
        }
        ZetanUtility.SetActive(bar, true);
        StartCoroutine(Progress());
    }

    public void Done()
    {
        targetTime = 0;
        currentTime = 0;
        isProgressing = false;
        ZetanUtility.SetActive(bar, false);
        onDone?.Invoke();
    }

    public void Cancel()
    {
        targetTime = 0;
        currentTime = 0;
        isProgressing = false;
        ZetanUtility.SetActive(bar, false);
        onCancel?.Invoke();
    }

    public void CancelWithoutEvent()
    {
        targetTime = 0;
        currentTime = 0;
        isProgressing = false;
        ZetanUtility.SetActive(bar, false);
    }
    private IEnumerator Progress()
    {
        while (isProgressing)
        {
            currentTime += Time.deltaTime;
            if (fillArea) fillArea.fillAmount = currentTime / targetTime;
            if (timeText) timeText.text = currentTime.ToString("F2") + "/" + targetTime.ToString("F2");
            if (currentTime >= targetTime)
            {
                Done();
                yield break;
            }
            yield return null;
        }
    }
}
