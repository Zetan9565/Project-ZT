using UnityEngine;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class BuildingPreview : MonoBehaviour
{
    private List<Collider> Colliders = new List<Collider>();

    private List<Collider2D> Colliders2D = new List<Collider2D>();

    public int ColliderCount
    {
        get { return Colliders.Count + Colliders2D.Count; }
    }

    public Vector3 Position
    {
        get { return transform.position; }
    }

    public LayerMask ignoreLayer = 9;

    public SpriteRenderer SpriteRenderer { get; private set; }
    //public MeshRenderer MeshRenderer { get; private set; }

    private void Awake()
    {
        SpriteRenderer = GetComponent<SpriteRenderer>();
        //MeshRenderer = GetComponent<MeshRenderer>();
    }

    public void OnTriggerEnter(Collider other)
    {
        if (!other.isTrigger)
            Colliders.Add(other);
    }

    public void OnTriggerExit(Collider other)
    {
        Colliders.Remove(other);
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.isTrigger)
        {
            Colliders2D.Add(collision);
        }
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        Colliders2D.Remove(collision);
    }
}