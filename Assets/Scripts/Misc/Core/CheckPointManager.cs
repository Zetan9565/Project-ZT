using System;
using System.Collections.Generic;
using UnityEngine;
using ZetanExtends;

public class CheckPointManager : SingletonMonoBehaviour<CheckPointManager>
{
    private Transform pointRoot;
    private Dictionary<string, Transform> pointParents = new Dictionary<string, Transform>();

    private Dictionary<CheckPointInformation, CheckPointData> checkPoints = new Dictionary<CheckPointInformation, CheckPointData>();

    public CheckPointData CreateCheckPoint(CheckPointInformation info, Action<CheckPointInformation> moveIntoAction, Action<CheckPointInformation> leaveAction = null)
    {
        if (!info || !info.IsValid || info.Scene.name != ZetanUtility.ActiveScene.name) return null;
        CheckPointData checkPointData = new CheckPointData(info);
        checkPointData.AddListener(moveIntoAction, leaveAction);
        foreach (var position in info.Positions)
        {
            checkPointData.Entities.Add(CreateCheckPointEntity(checkPointData, position));
        }
        return checkPointData;
    }

    public CheckPoint CreateCheckPointEntity(CheckPointData data, Vector3 position)
    {
        if (!pointRoot) pointRoot = new GameObject("CheckPoints").transform;
        if (!pointParents.TryGetValue(data.Info.ID, out var parent))
        {
            parent = pointRoot.CreateChild(data.Info.ID);
            pointParents.Add(data.Info.ID, parent);
        }
        CheckPoint checkPoint = parent.gameObject.CreateChild(position.ToString()).AddComponent<CheckPoint>();
        checkPoint.Init(data, position);
        return checkPoint;
    }

    public void DeleteCheckPoint(CheckPointInformation info)
    {
        if (pointParents.TryGetValue(info.ID, out var parent))
            Destroy(parent.gameObject);
        checkPoints.Remove(info);
    }

    public CheckPointData AddCheckPointListener(CheckPointInformation info, Action<CheckPointInformation> moveIntoAction, Action<CheckPointInformation> leaveAction = null)
    {
        if (checkPoints.TryGetValue(info, out var checkPoint))
        {
            if (checkPoint)
            {
                checkPoint.AddListener(moveIntoAction, leaveAction);
                return checkPoint;
            }
            else
            {
                checkPoints.Remove(info);
                return CreateCheckPoint(info, moveIntoAction, leaveAction);
            }
        }
        else return CreateCheckPoint(info, moveIntoAction, leaveAction);
    }

    public void RemoveCheckPointListener(CheckPointInformation info, Action<CheckPointInformation> moveIntoAction, Action<CheckPointInformation> leaveAction = null)
    {
        if (checkPoints.TryGetValue(info, out var checkPoint))
        {
            if (checkPoint) checkPoint.RemoveListener(moveIntoAction, leaveAction);
            else checkPoints.Remove(info);
        }
    }
}