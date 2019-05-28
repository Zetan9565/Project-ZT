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

    public BuildingInfomation MBuildingInfo { get; private set; }

    private List<MonoBehaviour> components = new List<MonoBehaviour>();
    private List<bool> componentStates = new List<bool>();

    private UnityEvent onDestroy = new UnityEvent();

    private bool custumDestroy;

    public BuildingAgent buildingAgent;

    public bool StarBuild(BuildingInfomation buildingInfo, Vector3 position)
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
        MyTools.SetActive(buildingFlag.gameObject, true);
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
            MyTools.SetActive(buildingFlag.gameObject, true);
        }
        else
        {
            IsUnderBuilding = false;
            IsBuilt = true;
            MyTools.SetActive(buildingFlag.gameObject, false);
        }
        if (buildingAgent) buildingAgent.UpdateUI();
    }

    void BuildComplete()
    {
        for (int i = 0; i < components.Count; i++)
        {
            components[i].enabled = componentStates[i];
        }
        IsUnderBuilding = false;
        IsBuilt = true;
        buildingFlag.text = "建造完成！";
        MessageManager.Instance.NewMessage("[" + name + "] 建造完成了");
        if (buildingAgent) buildingAgent.UpdateUI();
        StartCoroutine(WaitToUnshowFlag());
    }

    public void CustumDestroy(params UnityAction[] destroyActions)
    {
        if (destroyActions.Length < 1) { custumDestroy = false; return; }
        foreach (UnityAction ua in destroyActions)
        {
            onDestroy.AddListener(ua);
        }
        custumDestroy = true;
    }

    public float leftBuildTime;

    IEnumerator WaitToUnshowFlag()
    {
        yield return new WaitForSeconds(2);
        MyTools.SetActive(buildingFlag.gameObject, false);
    }

    public void TryDestroy()
    {
        if (custumDestroy) onDestroy.Invoke();
        else ConfirmHandler.Instance.NewConfirm(string.Format("确定拆除{0}{1}吗？", name, (Vector2)transform.position),
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
    private void Update()
    {
        if (IsUnderBuilding)
        {
            leftBuildTime -= Time.deltaTime;
            buildingFlag.text = "建造中[" + leftBuildTime.ToString("F2") + "s]";
            if (leftBuildTime <= 0)
            {
                BuildComplete();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player" && IsBuilt)
        {
            BuildingManager.Instance.CanDestroy(this);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == "Player" && IsBuilt)
        {
            BuildingManager.Instance.CanDestroy(this);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Player" && IsBuilt && BuildingManager.Instance.ToDestroy == this)
        {
            BuildingManager.Instance.CannotDestroy();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" && IsBuilt)
        {
            BuildingManager.Instance.CanDestroy(this);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Player" && IsBuilt)
        {
            BuildingManager.Instance.CanDestroy(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player" && IsBuilt && BuildingManager.Instance.ToDestroy == this)
        {
            BuildingManager.Instance.CannotDestroy();
        }
    }
    #endregion
}