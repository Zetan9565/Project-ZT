//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class FieldGrid : MonoBehaviour
//{
//    [SerializeField]
//    private Field parent;
//    public Field Parent
//    {
//        get
//        {
//            return parent;
//        }
//    }

//    public Crop crop;
//    public float fertility;

//    public void Empty()
//    {
//        crop = null;
//    }

//    #region MonoBehaviour
//    private void Awake()
//    {
//        if (parent)
//        {
//            parent.Grids.Add(this);
//        }
//    }

//    private void OnTriggerEnter2D(Collider2D collision)
//    {
//        if (collision.CompareTag("Player") && parent && parent.IsBuilt)
//        {
//            UIManager.Instance.EnableInteractive(true, "土地");
//        }
//    }

//    private void OnTriggerStay2D(Collider2D collision)
//    {
//        if (collision.CompareTag("Player") && parent && parent.IsBuilt)
//        {
//            UIManager.Instance.EnableInteractive(true, "土地");
//        }
//    }

//    private void OnTriggerExit2D(Collider2D collision)
//    {
//        if (collision.CompareTag("Player") && parent && parent.IsBuilt)
//        {
//            UIManager.Instance.EnableInteractive(false);
//        }
//    }
//    #endregion
//}
