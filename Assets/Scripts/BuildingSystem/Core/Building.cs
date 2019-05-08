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

    private bool IsUnderBuilding;

    [SerializeField]
    private TextMesh buildingFlag;

    public bool IsBuilt { get; private set; }

    private List<MonoBehaviour> components = new List<MonoBehaviour>();
    private List<bool> componentStates = new List<bool>();

    private UnityEvent onDestroy = new UnityEvent();

    private bool custumDestroy;

    public void StarBuild(BuildingInfo buildingInfo, Vector3 position)
    {
        transform.position = position;
        IDStarter = buildingInfo.IDStarter;
        name = buildingInfo.Name;
        leftBuildTime = buildingInfo.BuildTime;
        GetIDTail();
        foreach (MonoBehaviour mb in GetComponentsInChildren<MonoBehaviour>())
        {
            componentStates.Add(mb.enabled);
            components.Add(mb);
            if (mb != this) mb.enabled = false;
        }
        IsUnderBuilding = true;
        MyTools.SetActive(buildingFlag.gameObject, true);
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

    IEnumerator WaitToUnshowFlag()
    {
        yield return new WaitForSeconds(2);
        MyTools.SetActive(buildingFlag.gameObject, false);
    }

    public void TryDestroy()
    {
        if (custumDestroy) onDestroy.Invoke();
        else ConfirmHandler.Instance.NewConfirm("设施内的东西不会保留，确定销毁吗？", delegate { BuildingManager.Instance.ConfirmDestroy(); });
    }

    void GetIDTail()
    {
        Building[] buildings = FindObjectsOfType<Building>();
        for (int i = 1; i < 100000; i++)
        {
            IDTail = i.ToString().PadLeft(5, '0');
            string newID = IDStarter + IDTail;
            if (!Array.Exists(buildings, x => x.ID == newID && x != this))
                return;
        }
        IDTail = string.Empty;
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
}