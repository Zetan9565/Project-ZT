using System;
using System.Collections.Generic;
using UnityEngine;
using ZetanStudio;
using ZetanStudio.Extension;

public static class CheckPointManager
{
    private static Transform pointRoot;
    private static Dictionary<string, Transform> pointParents = new Dictionary<string, Transform>();

    private static Dictionary<CheckPointInformation, CheckPointData> checkPoints = new Dictionary<CheckPointInformation, CheckPointData>();

    [InitMethod(-2)]
    public static void Init()
    {
        if (!pointRoot) pointRoot = new GameObject("CheckPoints").transform;
        pointRoot.position = Vector3.zero;
    }

    public static CheckPointData CreateCheckPoint(CheckPointInformation info, Action<CheckPointInformation> moveIntoAction, Action<CheckPointInformation> leaveAction = null)
    {
        if (!info || !info.IsValid || info.Scene != Utility.GetActiveScene().name) return null;
        CheckPointData checkPointData = new CheckPointData(info);
        checkPointData.AddListener(moveIntoAction, leaveAction);
        foreach (var position in info.Positions)
        {
            checkPointData.Entities.Add(CreateCheckPointEntity(checkPointData, position));
        }
        return checkPointData;
    }

    public static CheckPoint CreateCheckPointEntity(CheckPointData data, Vector3 position)
    {
        if (!pointParents.TryGetValue(data.Info.ID, out var parent))
        {
            parent = pointRoot.CreateChild(data.Info.ID);
            pointParents.Add(data.Info.ID, parent);
        }
        CheckPoint checkPoint = parent.gameObject.CreateChild(position.ToString()).AddComponent<CheckPoint>();
        checkPoint.Init(data, position);
        return checkPoint;
    }

    public static void DeleteCheckPoint(CheckPointInformation info)
    {
        if (pointParents.TryGetValue(info.ID, out var parent))
            UnityEngine.Object.Destroy(parent.gameObject);
        checkPoints.Remove(info);
    }

    public static CheckPointData AddCheckPointListener(CheckPointInformation info, Action<CheckPointInformation> moveIntoAction, Action<CheckPointInformation> leaveAction = null)
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

    public static void RemoveCheckPointListener(CheckPointInformation info, Action<CheckPointInformation> moveIntoAction, Action<CheckPointInformation> leaveAction = null)
    {
        if (checkPoints.TryGetValue(info, out var checkPoint))
        {
            if (checkPoint) checkPoint.RemoveListener(moveIntoAction, leaveAction);
            else checkPoints.Remove(info);
        }
    }
}