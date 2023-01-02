using UnityEngine;
using System.Collections;
using UnityEngine.Events;

namespace ZetanStudio
{
    [AddComponentMenu("Zetan Studio/地图图标生成器")]
    public class MapIconHolder : MonoBehaviour
    {
        public Sprite icon;

        public Vector2 iconSize = new Vector2(48, 48);

        public Vector2 offset;

        public bool drawOnWorldMap = true;

        public bool keepOnMap = true;

        [SerializeField, Tooltip("小于零时表示显示状态不受距离影响。")]
        public float maxValidDistance = -1;

        public bool forceHided;

        public bool removeAble;

        public bool showRange;

        public Color rangeColor = new Color(1, 1, 1, 0.5f);

        public float rangeSize = 144;

        public MapIconType iconType;

        public MapIconData iconInstance;

        public bool gizmos = true;

        public bool AutoHide => maxValidDistance > 0;

        public string textToDisplay;

        public MapIconEvents iconEvents;

        public UnityEvent OnFingerClick => iconEvents.onFingerClick;
        public UnityEvent OnMouseClick => iconEvents.onMouseClick;

        public UnityEvent OnMouseEnter => iconEvents.onMouseEnter;
        public UnityEvent OnMouseExit => iconEvents.onMouseExit;

        public void CreateIcon()
        {
            if (MapManager.Instance)
            {
                if (iconInstance)
                {
                    //Debug.Log(gameObject.name + " remove");
                    MapManager.Instance.RemoveMapIcon(this, true);
                }
                MapManager.Instance.CreateMapIcon(this);
                //Debug.Log(gameObject.name);
            }
        }

        public void HideIcon()
        {
            if (iconInstance) iconInstance.Hide();
        }

        readonly WaitForSeconds WaitForSeconds = new WaitForSeconds(0.5f);
        private IEnumerator UpdateIcon()
        {
            while (true)
            {
                if (iconInstance)
                {
                    iconInstance.UpdateIcon(icon);
                    iconInstance.UpdateSize(iconSize);
                    iconInstance.iconType = iconType;
                    yield return WaitForSeconds;
                }
                else yield return new WaitUntil(() => iconInstance);
            }
        }

        #region MonoBehaviour
        void Start()
        {
            CreateIcon();
        }

        private void Awake()
        {
            StartCoroutine(UpdateIcon());
        }

        private void OnDrawGizmosSelected()
        {
            if (gizmos && MapManager.Instance && !Application.isPlaying)
                MapManager.Instance.DrawIconGizmos(this);
        }

        private void OnDestroy()
        {
            if (MapManager.Instance) MapManager.Instance.RemoveMapIcon(this, true);
        }
        #endregion
    }
    [System.Serializable]
    public class MapIconEvents
    {
        public UnityEvent onFingerClick = new UnityEvent();
        public UnityEvent onMouseClick = new UnityEvent();

        public UnityEvent onMouseEnter = new UnityEvent();
        public UnityEvent onMouseExit = new UnityEvent();

        public void RemoveAllListner()
        {
            onFingerClick.RemoveAllListeners();
            onMouseClick.RemoveAllListeners();
            onMouseEnter.RemoveAllListeners();
            onMouseExit.RemoveAllListeners();
        }
    }
}