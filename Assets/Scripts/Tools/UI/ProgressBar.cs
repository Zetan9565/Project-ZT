using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ProgressBar : SingletonMonoBehaviour<ProgressBar>
{
    [SerializeField]
    private GameObject bar;

    private Canvas barCanvas;

    [SerializeField]
    private Image fillArea;

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
    }

    public void NewProgress(float seconds, UnityAction doneAction, UnityAction cancelAction, string actionName = null)
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
        barCanvas.sortingOrder = WindowsManager.Instance.TopOrder + 1;
        if (progressCoroutine != null) StopCoroutine(progressCoroutine);
        progressCoroutine = StartCoroutine(Progress());
    }

    public void NewProgress(float seconds, int loopTimes, UnityAction breakAction, UnityAction doneAction, UnityAction cancelAction, string actionName = null)
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
        barCanvas.sortingOrder = WindowsManager.Instance.TopOrder + 1;
        breakCondition = null;
        progressCoroutine = StartCoroutine(Progress());
    }

    public void NewProgress(float seconds, int loopTimes, Func<bool> breakCondition, UnityAction breakAction, UnityAction doneAction, UnityAction cancelAction, string actionName = null)
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
        this.loopTimes = loopTimes;
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
        if (loopTimes > 0 && (breakCondition == null || !breakCondition.Invoke()))
        {
            NewProgress(targetTime, --loopTimes, breakCondition, breakAction, onDone, onCancel, actionText.text);
        }
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
