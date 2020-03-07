using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

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

    private UnityAction onDone;
    private UnityAction onCancel;
    private Func<bool> breakCondition;
    private UnityAction breakAction;

    private void Awake()
    {
        if (!GetComponent<GraphicRaycaster>()) gameObject.AddComponent<GraphicRaycaster>();
        barCanvas = GetComponent<Canvas>();
        barCanvas.overrideSorting = true;
        barCanvas.sortingLayerID = SortingLayer.NameToID("UI");
        cancel.onClick.AddListener(Cancel);
    }

    public void New(float seconds, UnityAction doneAction, UnityAction cancelAction, string actionName = null, bool displayCancel = false)
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
        barCanvas.sortingOrder = WindowsManager.Instance.TopOrder + 1;
        if (progressCoroutine != null) StopCoroutine(progressCoroutine);
        progressCoroutine = StartCoroutine(Progress());
    }

    public void New(float seconds, int loopTimes, UnityAction doneAction, UnityAction cancelAction, string actionName = null, bool displayCancel = false)
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
        breakAction = null;
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
        barCanvas.sortingOrder = WindowsManager.Instance.TopOrder + 1;
        breakCondition = null;
        progressCoroutine = StartCoroutine(Progress());
    }

    public void New(float seconds, Func<bool> breakCondition, UnityAction breakAction,
        UnityAction doneAction, UnityAction cancelAction, string actionName = null, bool displayCancel = false)
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
        barCanvas.sortingOrder = WindowsManager.Instance.TopOrder + 1;
        this.breakCondition = breakCondition;
        progressCoroutine = StartCoroutine(Progress());
    }

    private void Done()
    {
        currentTime = 0;
        isProgressing = false;
        ZetanUtility.SetActive(bar, false);
        onDone?.Invoke();
        if (breakCondition != null && !breakCondition.Invoke()) New(targetTime, breakCondition, breakAction, onDone, onCancel, actionText.text, cancel.gameObject.activeSelf);
        else if (breakCondition == null && loopTimes > 0) New(targetTime, --loopTimes, onDone, onCancel, actionText.text, cancel.gameObject.activeSelf);
        else
        {
            breakAction?.Invoke();
            CancelWithoutNotify();
        }
    }

    public void Cancel()
    {
        targetTime = 0;
        currentTime = 0;
        loopTimes = 0;
        isProgressing = false;
        onCancel?.Invoke();
        onDone = null;
        onCancel = null;
        breakCondition = null;
        ZetanUtility.SetActive(bar, false);
        if (progressCoroutine != null) StopCoroutine(progressCoroutine);
    }

    private void CancelWithoutNotify()
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
}
