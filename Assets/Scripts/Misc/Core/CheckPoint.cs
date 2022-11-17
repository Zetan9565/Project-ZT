using UnityEngine;

namespace ZetanStudio
{
    [DisallowMultipleComponent, RequireComponent(typeof(MapIconHolder))]
    public class CheckPoint : MonoBehaviour
    {
        public string ID => Data ? Data.Info.ID : string.Empty;

        private dynamic trigger;

        public CheckPointData Data { get; private set; }

        public bool IsValid => Data && Data.Info;

        public void Init(CheckPointData data, Vector3 position)
        {
            Data = data;
            Utility.SetActive(gameObject, data);
            if (!data) return;
            Data = data;
            gameObject.layer = Data.Info.Layer;
            transform.position = position;
            if (trigger == null)
            {
                switch (Data.Info.TriggerType)
                {
                    case CheckPointTriggerType.Box:
                        trigger = gameObject.AddComponent<BoxCollider2D>();
                        (trigger as BoxCollider2D).size = Data.Info.Size;
                        break;
                    case CheckPointTriggerType.Circle:
                        trigger = gameObject.AddComponent<CircleCollider2D>();
                        (trigger as CircleCollider2D).radius = Data.Info.Radius;
                        break;
                    case CheckPointTriggerType.Capsule:
                        trigger = gameObject.AddComponent<CapsuleCollider2D>();
                        (trigger as CapsuleCollider2D).size = new Vector2(Data.Info.Radius * 2, Data.Info.Height);
                        break;
                }
                if (trigger) (trigger as Collider2D).isTrigger = true;
            }
        }

        #region MonoBehaviour
        #region 3D Trigger
        //private void OnTriggerEnter(Collider other)
        //{
        //    if (!IsValid) return;
        //    if (other.CompareTag(Data.Info.TargetTag))
        //        Data.MoveInto();
        //}
        //private void OnTriggerStay(Collider other)
        //{
        //    if (!IsValid) return;
        //    if (other.CompareTag(Data.Info.TargetTag))
        //        Data.StayInside();
        //}
        //private void OnTriggerExit(Collider other)
        //{
        //    if (!IsValid) return;
        //    if (other.CompareTag(Data.Info.TargetTag))
        //        Data.LeaveAway();
        //}
        #endregion

        #region 2D Trigger
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!IsValid) return;
            if (collision.CompareTag(Data.Info.TargetTag))
                Data.MoveInto();
        }
        private void OnTriggerStay2D(Collider2D collision)
        {
            if (!IsValid) return;
            if (collision.CompareTag(Data.Info.TargetTag))
                Data.StayInside();
        }
        private void OnTriggerExit2D(Collider2D collision)
        {
            if (!IsValid) return;
            if (collision.CompareTag(Data.Info.TargetTag))
                Data.LeaveAway();
        }
        #endregion
        #endregion
    }
}