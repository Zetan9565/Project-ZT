using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class Building : MonoBehaviour
{
    public string IDStarter;

    public string IDTail;

    public string ID { get { return IDStarter + IDTail; } }

    public new string name;

    public UnityEngine.UI.Button.ButtonClickedEvent onButtonClick = new UnityEngine.UI.Button.ButtonClickedEvent();

    public bool isUnderBuilding;

    [SerializeField]
    private Vector3 buildingFlagOffset;
    public Vector3 BuildingFlagOffset => buildingFlagOffset;

    public bool IsBuilt { get; private set; }

    public BuildingInformation MBuildingInfo { get; private set; }


    private Dictionary<Behaviour, bool> components = new Dictionary<Behaviour, bool>();
    private Dictionary<Collider, bool> colliders = new Dictionary<Collider, bool>();

    [SerializeField]
    protected UnityEvent onDestroy = new UnityEvent();

    public BuildingAgent buildingAgent;

    public float leftBuildTime;

    public bool StarBuild(BuildingInformation buildingInfo, Vector3 position)
    {
        if (!buildingInfo) return false;
        transform.position = position;
        MBuildingInfo = buildingInfo;
        IDStarter = MBuildingInfo.IDStarter;
        name = MBuildingInfo.Name;
        leftBuildTime = MBuildingInfo.BuildTime;
        if (buildingAgent) buildingAgent.UpdateUI();
        GetIDTail();
        if (string.IsNullOrEmpty(IDTail))
        {
            MessageManager.Instance.New($"[{name}]已经达到最大建设数量");
            if (buildingAgent) buildingAgent.Clear(true);
            Destroy(gameObject);
            return false;
        }
        foreach (var b in GetComponentsInChildren<Behaviour>())
        {
            var c2d = b as Collider2D;
            if (c2d && !c2d.isTrigger) continue;
            components.Add(b, b.enabled);
            if (b != this) b.enabled = false;
        }
        foreach (var c in GetComponentsInChildren<Collider>())
        {
            if (!c.isTrigger) continue;
            colliders.Add(c, c.enabled);
            c.enabled = false;
        }
        isUnderBuilding = true;
        StartCoroutine(Build());
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
            foreach (var b in GetComponentsInChildren<Behaviour>())
            {
                components.Add(b, b.enabled);
                if (b != this) b.enabled = false;
            }
            isUnderBuilding = true;
            StartCoroutine(Build());
        }
        else
        {
            isUnderBuilding = false;
            IsBuilt = true;
        }
        if (buildingAgent) buildingAgent.UpdateUI();
    }

    private void BuildComplete()
    {
        foreach (var b in components)
        {
            b.Key.enabled = b.Value;
        }
        isUnderBuilding = false;
        IsBuilt = true;
        MessageManager.Instance.New($"[{name}] 建造完成了");
        if (buildingAgent) buildingAgent.UpdateUI();
    }

    private IEnumerator Build()
    {
        while (isUnderBuilding)
        {
            leftBuildTime -= Time.deltaTime;
            if (leftBuildTime <= 0)
            {
                BuildComplete();
                yield break;
            }
            if (buildingAgent) buildingAgent.UpdateUI();
            yield return null;
        }
    }

    public virtual void AskDestroy()
    {
        ConfirmManager.Instance.New(string.Format("确定拆除{0}{1}吗？", name, (Vector2)transform.position),
            delegate { BuildingManager.Instance.DestroyBuilding(this); });
    }

    void GetIDTail()
    {
        Building[] buildings = FindObjectsOfType<Building>();
        IDTail = string.Empty;
        for (int i = 1; i < 1000; i++)
        {
            IDTail = i.ToString().PadLeft(3, '0');
            string newID = IDStarter + IDTail;
            if (!Array.Exists(buildings, x => x.ID == newID && x != this))
                break;
        }
    }

    public void Destroy()
    {
        onDestroy?.Invoke();
        Destroy(gameObject);
    }

    #region MonoBehaviour
    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && IsBuilt)
        {
            BuildingManager.Instance.CanManage(this);
        }
    }

    protected virtual void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && IsBuilt)
        {
            BuildingManager.Instance.CanManage(this);
        }
    }

    protected virtual void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && IsBuilt && BuildingManager.Instance.CurrentBuilding == this)
        {
            BuildingManager.Instance.CannotManage();
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