using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Events;
using System;

public class Building : MonoBehaviour
{
    public string IDStarter;

    public string IDTail;

    public string ID { get { return IDStarter + IDTail; } }

    public new string name;

    public bool IsUnderBuilding { get; private set; }

    [SerializeField]
    private TextMesh buildingFlag;

    public bool IsBuilt { get; private set; }

    public BuildingInformation MBuildingInfo { get; private set; }

    private List<MonoBehaviour> components = new List<MonoBehaviour>();
    private List<bool> componentStates = new List<bool>();

    protected UnityEvent onDestroy = new UnityEvent();

    public BuildingAgent buildingAgent;

    public float leftBuildTime;

    public bool StarBuild(BuildingInformation buildingInfo, Vector3 position)
    {
        transform.position = position;
        MBuildingInfo = buildingInfo;
        IDStarter = MBuildingInfo.IDStarter;
        name = MBuildingInfo.Name;
        leftBuildTime = MBuildingInfo.BuildTime;
        if (buildingAgent) buildingAgent.UpdateUI();
        GetIDTail();
        if (string.IsNullOrEmpty(IDTail))
        {
            MessageManager.Instance.NewMessage(name + "已经达到最大建设数量");
            if (buildingAgent) buildingAgent.Clear(true);
            Destroy(gameObject);
        }
        foreach (MonoBehaviour mb in GetComponentsInChildren<MonoBehaviour>())
        {
            componentStates.Add(mb.enabled);
            components.Add(mb);
            if (mb != this) mb.enabled = false;
        }
        IsUnderBuilding = true;
        StartCoroutine(Build());
        if (buildingFlag) ZetanUtilities.SetActive(buildingFlag.gameObject, true);
        return true;
    }

    public void LoadBuild(string IDStarter, string IDTail, string name, float buildTime, Vector3 position)
    {
        transform.position = position;
        this.IDStarter = IDStarter;
        this.IDTail = IDTail;
        this.name = name;
        leftBuildTime = buildTime;
        if (leftBuildTime > 0)
        {
            foreach (MonoBehaviour mb in GetComponentsInChildren<MonoBehaviour>())
            {
                componentStates.Add(mb.enabled);
                components.Add(mb);
                if (mb != this) mb.enabled = false;
            }
            IsUnderBuilding = true;
            StartCoroutine(Build());
            if (buildingFlag) ZetanUtilities.SetActive(buildingFlag.gameObject, true);
        }
        else
        {
            IsUnderBuilding = false;
            IsBuilt = true;
            if (buildingFlag) ZetanUtilities.SetActive(buildingFlag.gameObject, false);
        }
        if (buildingAgent) buildingAgent.UpdateUI();
    }

    private void BuildComplete()
    {
        for (int i = 0; i < components.Count; i++)
        {
            components[i].enabled = componentStates[i];
        }
        IsUnderBuilding = false;
        IsBuilt = true;
        if (buildingFlag) buildingFlag.text = "建造完成！";
        MessageManager.Instance.NewMessage("[" + name + "] 建造完成了");
        if (buildingAgent) buildingAgent.UpdateUI();
        StartCoroutine(WaitToHideFlag());
    }

    private IEnumerator Build()
    {
        while (IsUnderBuilding)
        {
            leftBuildTime -= Time.deltaTime;
            if (buildingFlag) buildingFlag.text = "建造中[" + leftBuildTime.ToString("F2") + "s]";
            if (leftBuildTime <= 0)
            {
                BuildComplete();
                yield break;
            }
            if (buildingAgent) buildingAgent.UpdateUI();
            yield return null;
        }
    }

    private IEnumerator WaitToHideFlag()
    {
        yield return new WaitForSeconds(2);
        if (buildingFlag) ZetanUtilities.SetActive(buildingFlag.gameObject, false);
    }

    public virtual void TryDestroy()
    {
        onDestroy?.Invoke();
        ConfirmManager.Instance.NewConfirm(string.Format("确定拆除{0}{1}吗？", name, (Vector2)transform.position),
            BuildingManager.Instance.ConfirmDestroy,
            delegate
            {
                if (IsBuilt && BuildingManager.Instance.ToDestroy == this)
                {
                    BuildingManager.Instance.CannotDestroy();
                }
            });
    }

    void GetIDTail()
    {
        Building[] buildings = FindObjectsOfType<Building>();
        IDTail = string.Empty;
        for (int i = 1; i < 100000; i++)
        {
            IDTail = i.ToString().PadLeft(5, '0');
            string newID = IDStarter + IDTail;
            if (!Array.Exists(buildings, x => x.ID == newID && x != this))
                break;
        }
    }

    #region MonoBehaviour
    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player" && IsBuilt)
        {
            BuildingManager.Instance.CanDestroy(this);
        }
    }

    protected virtual void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == "Player" && IsBuilt)
        {
            BuildingManager.Instance.CanDestroy(this);
        }
    }

    protected virtual void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Player" && IsBuilt && BuildingManager.Instance.ToDestroy == this)
        {
            BuildingManager.Instance.CannotDestroy();
        }
    }

    /*protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" && IsBuilt)
        {
            BuildingManager.Instance.CanDestroy(this);
        }
    }

    protected virtual void OnTriggerStay(Collider other)
    {
        if (other.tag == "Player" && IsBuilt)
        {
            BuildingManager.Instance.CanDestroy(this);
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player" && IsBuilt && BuildingManager.Instance.ToDestroy == this)
        {
            BuildingManager.Instance.CannotDestroy();
        }
    }*/
    #endregion
}