using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class ProgressBar : SingletonMonoBehaviour<ProgressBar>
{
    [SerializeField]
    private GameObject bar;

    private Canvas barCanvas;

    [SerializeField]
    private Image fillArea;

    [SerializeField]
    private Button cancel;

    [SerializeField]
    private Text actionText;

    [SerializeField]
    private Text timeText;

    private bool isProgressing;
    private float targetTime;
    private float currentTime;
    private int loopTimes;

    private Action onDone;
    private Action onCancel;
    private Func<bool> breakCondition;
    private Action breakAction;
    private Func<float> getTime;
    private Func<bool> shouldCancel;

    private void Awake()
    {
        if (!GetComponent<GraphicRaycaster>()) gameObject.AddComponent<GraphicRaycaster>();
        barCanvas = GetComponent<Canvas>();
        barCanvas.overrideSorting = true;
        barCanvas.sortingLayerID = SortingLayer.NameToID("UI");
        cancel.onClick.AddListener(Cancel);
    }

    public void New(float seconds, Action doneAction, Action cancelAction, string actionName = null, bool displayCancel = false)
    {
        if (seconds <= 0) return;
        if (progressCoroutine != null)
        {
            StopCoroutine(progressCoroutine);
            progressCoroutine = null;
        }
        onDone = doneAction;
        onCancel = cancelAction;
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
        loopTimes = 0;
        ZetanUtility.SetActive(bar, true);
        ZetanUtility.SetActive(cancel, displayCancel);
        //barCanvas.sortingOrder = WindowsManager.Instance.TopOrder + 1;
        if (progressCoroutine != null) StopCoroutine(progressCoroutine);
        progressCoroutine = StartCoroutine(Progress());
    }

    public void New(float seconds, int loopTimes, Action breakAction, Action doneAction, Action cancelAction, string actionName = null, bool displayCancel = false)
    {
        if (seconds <= 0 || loopTimes < 0) return;
        if (progressCoroutine != null)
        {
            StopCoroutine(progressCoroutine);
            progressCoroutine = null;
        }
        onDone = doneAction;
        onCancel = cancelAction;
        breakCondition = null;
        this.breakAction = breakAction;
        targetTime = seconds;
        isProgressing = true;
        this.loopTimes = loopTimes;
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
        ZetanUtility.SetActive(cancel, displayCancel);
        //barCanvas.sortingOrder = WindowsManager.Instance.TopOrder + 1;
        breakCondition = null;
        progressCoroutine = StartCoroutine(Progress());
    }

    public void New(float seconds, Func<bool> breakCondition, Action breakAction,
        Action doneAction, Action cancelAction, string actionName = null, bool displayCancel = false)
    {
        if (seconds <= 0 || loopTimes < 0) return;
        if (progressCoroutine != null)
        {
            StopCoroutine(progressCoroutine);
            progressCoroutine = null;
        }
        onDone = doneAction;
        onCancel = cancelAction;
        this.breakAction = breakAction;
        targetTime = seconds;
        loopTimes = 0;
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
        ZetanUtility.SetActive(cancel, displayCancel);
        //barCanvas.sortingOrder = WindowsManager.Instance.TopOrder + 1;
        this.breakCondition = breakCondition;
        progressCoroutine = StartCoroutine(Progress());
    }

    public void New(float seconds, Func<float> getTime, Func<bool> cancelCondition, Action cancelAction, string actionName = null)
    {
        targetTime = seconds;
        this.getTime = getTime;
        if (progressCoroutine != null)
        {
            StopCoroutine(progressCoroutine);
            progressCoroutine = null;
        }
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
        if (cancelCondition != null)
            shouldCancel = cancelCondition;
        else shouldCancel = () => { return true; };
        onCancel = cancelAction;
        ZetanUtility.SetActive(bar, true);
        ZetanUtility.SetActive(cancel, true);
        progressCoroutine = StartCoroutine(ProgressCustom());
    }

    private void Done()
    {
        currentTime = 0;
        isProgressing = false;
        ZetanUtility.SetActive(bar, false);
        onDone?.Invoke();
        if (breakCondition != null && !breakCondition.Invoke()) New(targetTime, breakCondition, breakAction, onDone, onCancel, actionText.text, cancel.gameObject.activeSelf);
        else if (breakCondition == null && loopTimes > 0) New(targetTime, --loopTimes, breakAction, onDone, onCancel, actionText.text, cancel.gameObject.activeSelf);
        else
        {
            breakAction?.Invoke();
            CancelWithoutAction();
        }
    }

    public void Cancel()
    {
        onCancel?.Invoke();
        CancelWithoutAction();
    }

    public void CancelWithoutAction()
    {
        targetTime = 0;
        currentTime = 0;
        loopTimes = 0;
        isProgressing = false;
        onDone = null;
        onCancel = null;
        breakCondition = null;
        ZetanUtility.SetActive(bar, false);
        if (progressCoroutine != null) StopCoroutine(progressCoroutine);
    }

    private Coroutine progressCoroutine;
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
    private IEnumerator ProgressCustom()
    {
        while (isProgressing)
        {
            if (getTime != null && !shouldCancel())
            {
                if (fillArea) fillArea.fillAmount = getTime() / targetTime;
                if (timeText) timeText.text = getTime().ToString("F2") + "/" + targetTime.ToString("F2");
            }
            else
            {
                CancelWithoutAction();
                yield break;
            }
            yield return null;
        }
    }
}
