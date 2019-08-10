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

    public SpriteRenderer SpriteRenderer { get; private set; }
    //public MeshRenderer MeshRenderer { get; private set; }

    private void Awake()
    {
        SpriteRenderer = GetComponent<SpriteRenderer>();
        //MeshRenderer = GetComponent<MeshRenderer>();

        Rigidbody2D rigidbody2D = GetComponent<Rigidbody2D>();
        if (!rigidbody2D) rigidbody2D = gameObject.AddComponent<Rigidbody2D>();
        rigidbody2D.isKinematic = true;
        Collider2D collider2D = GetComponent<Collider2D>();
        if (gameObject.layer != LayerMask.NameToLayer("BuildingPreview"))
            gameObject.layer = LayerMask.NameToLayer("BuildingPreview");
        if (collider2D && !collider2D.isTrigger) collider2D.isTrigger = true;
        //Collider collider = GetComponent<Collider>();
        //if (collider && !collider.isTrigger) collider.isTrigger = true;
    }

    //public void OnTriggerEnter(Collider other)
    //{
    //    if (!other.isTrigger)
    //        Colliders.Add(other);
    //}

    //public void OnTriggerExit(Collider other)
    //{
    //    Colliders.Remove(other);
    //}

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