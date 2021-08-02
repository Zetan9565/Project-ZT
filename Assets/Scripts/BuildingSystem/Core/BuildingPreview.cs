﻿using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[DisallowMultipleComponent]
public class BuildingPreview : MonoBehaviour
{
    public BuildingData Data { get; private set; }

    private readonly HashSet<Collider> colliders = new HashSet<Collider>();

    private readonly HashSet<Collider2D> colliders2D = new HashSet<Collider2D>();

    public GameObject preview;
    public GameObject building;

    public bool BuildAble => (colliders.Count + colliders2D.Count) < 1;

    public LayerMask ignoreLayer = 0;

    [SerializeField]
    private float centerOffset = 1.0f;
    public float CenterOffset
    {
        get
        {
            return centerOffset;
        }
    }

    [SerializeField]
    private Vector3 buildingFlagOffset;
    public Vector3 BuildingFlagOffset => buildingFlagOffset;

    private Renderer[] renderers;

    private readonly Dictionary<Renderer, Color> oriColors = new Dictionary<Renderer, Color>();
    private readonly Dictionary<Renderer, Color> redColors = new Dictionary<Renderer, Color>();

    private void Awake()
    {
        if (!preview.TryGetComponent<TriggerEvents>(out var colliderEvent))
        {
            colliderEvent = preview.AddComponent<TriggerEvents>();
            colliderEvent.OnEnter2D.AddListener(OnObstacleEnter);
            colliderEvent.OnStay2D.AddListener(OnObstacleStay);
            colliderEvent.OnExit2D.AddListener(OnObstacleExit);
        }
        if (preview.gameObject.layer != LayerMask.NameToLayer("BuildingPreview"))
            preview.gameObject.layer = LayerMask.NameToLayer("BuildingPreview");

        ZetanUtility.SetActive(preview, true);
        ZetanUtility.SetActive(building, false);

        renderers = GetComponentsInChildren<MeshRenderer>();
        renderers = renderers.Concat(GetComponentsInChildren<SpriteRenderer>()).ToArray();

        foreach (Renderer renderer in renderers)
        {
            if (renderer.gameObject.activeSelf)
            {
                oriColors.Add(renderer, renderer.material.color);
                redColors.Add(renderer, renderer.material.color * Color.red);
            }
        }

        //Collider[] colliders = preview.GetComponentsInChildren<Collider>();
        //foreach (Collider collider in colliders)
        //{
        //    collider.isTrigger = true;
        //}

        Collider2D[] collider2Ds = preview.GetComponentsInChildren<Collider2D>();
        foreach (Collider2D collider2D in collider2Ds)
        {
            collider2D.isTrigger = true;
        }
    }

    #region 障碍物检测相关
    #region 3D Trigger
    //public void OnObstacleEnter(Collider other)
    //{
    //    if (!other.isTrigger)
    //        colliders.Add(other);
    //    CheckObstacle();
    //}
    
    //public void OnObstacleStay(Collider other)
    //{
    //    if (!other.isTrigger)
    //        colliders.Add(other);
    //    CheckObstacle();
    //}

    //public void OnObstacleExit(Collider other)
    //{
    //    colliders.Remove(other);
    //    CheckObstacle();
    //}
    #endregion

    #region 2D Trigger
    public void OnObstacleEnter(Collider2D collision)
    {
        if (!collision.isTrigger)
        {
            if (!colliders2D.Contains(collision))
                colliders2D.Add(collision);
        }
        CheckObstacle();
    }

    public void OnObstacleStay(Collider2D collision)
    {
        if (!collision.isTrigger)
        {
            if (!colliders2D.Contains(collision))
                colliders2D.Add(collision);
        }
        CheckObstacle();
    }

    public void OnObstacleExit(Collider2D collision)
    {
        colliders2D.Remove(collision);
        CheckObstacle();
    }
    #endregion

    private void CheckObstacle()
    {
        void SetColor(Dictionary<Renderer, Color> colors)
        {
            foreach (Renderer renderer in renderers)
            {
                if (colors.TryGetValue(renderer, out var color))
                    renderer.material.color = color;
            }
        }

        if (!BuildAble)
        {
            SetColor(redColors);
        }
        else
        {
            SetColor(oriColors);
        }
    }
    #endregion

    public void StartBuild(BuildingData data)
    {
        Data = data;
        data.preview = this;
        ZetanUtility.SetActive(preview, false);
        ZetanUtility.SetActive(building, true);
        transform.position = data.position;
        gameObject.name = Data.name;
    }

    public void OnBuilt()
    {
        if (!Data) return;
        MessageManager.Instance.New($"[{Data.name}] 建造完成了");
        BuildingManager.Instance.Build(Data);
        Data = null;
        ZetanUtility.SetActive(gameObject, false);
        Destroy(gameObject, 2);
    }
}