using System;
using System.Collections.Generic;
using ZetanStudio;

public class CheckPointData
{
    public CheckPointInformation Info { get; private set; }

    public bool IsTargetInside { get; private set; }

    public readonly List<CheckPoint> Entities = new List<CheckPoint>();

    private Action<CheckPointInformation> onInto;
    private Action<CheckPointInformation> onStay;
    private Action<CheckPointInformation> onLeave;

    public CheckPointData(CheckPointInformation info)
    {
        Info = info;
    }

    public void MoveInto()
    {
        onInto?.Invoke(Info);
        IsTargetInside = true;
    }

    public void StayInside()
    {
        onStay?.Invoke(Info);
        IsTargetInside = true;
    }

    public void LeaveAway()
    {
        onLeave?.Invoke(Info);
        IsTargetInside = false;
    }

    public void AddListener(Action<CheckPointInformation> intoAction, Action<CheckPointInformation> leaveAction = null, Action<CheckPointInformation> stayAction = null)
    {
        onInto += intoAction;
        onLeave += leaveAction;
        onStay += stayAction;
        if (onInto != null || onLeave != null || onStay!=null)
        {
            foreach (var entity in Entities)
            {
                Utility.SetActive(entity.gameObject, true);
            }
        }
    }
    public void RemoveListener(Action<CheckPointInformation> intoAction, Action<CheckPointInformation> leaveAction = null, Action<CheckPointInformation> stayAction = null)
    {
        onInto -= intoAction;
        onLeave -= leaveAction;
        onStay -= stayAction;
        if (onInto == null && onLeave == null)
        {
            foreach (var entity in Entities)
            {
                Utility.SetActive(entity.gameObject, false);
            }
        }
    }

    public static implicit operator bool(CheckPointData self)
    {
        return self != null;
    }
}