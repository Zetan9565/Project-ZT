using UnityEngine;

public class InteractionUI : WindowUI
{
    public RectTransform view;

    public GameObject buttonPrefab;
    public Transform buttonParent;

    protected override void Awake()
    {
        base.Awake();
        windowCanvas.overrideSorting = false;
    }
}