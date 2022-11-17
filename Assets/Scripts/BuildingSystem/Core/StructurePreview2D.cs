using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ZetanStudio.StructureSystem
{
    using InteractionSystem;

    [DisallowMultipleComponent]
    public class StructurePreview2D : Interactive2D
    {
        public override string Name
        {
            get
            {
                return Data ? Data.Info.Name : gameObject.name;
            }
        }

        public override bool IsInteractive
        {
            get
            {
                return Data && !(WindowsManager.IsWindowOpen<StructureManageWindow>(out var structure) && structure.IsManaging);
            }
        }

        public StructureData Data { get; private set; }

        private readonly HashSet<Collider2D> colliders2D = new HashSet<Collider2D>();

        public GameObject preview;
        public GameObject structure;

        public bool BuildAble => colliders2D.Count < 1;

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
        private Vector3 structureFlagOffset;
        public Vector3 StructureFlagOffset => structureFlagOffset;

        private Renderer[] renderers;
        private readonly Dictionary<Renderer, Color> oriColors = new Dictionary<Renderer, Color>();
        private readonly Dictionary<Renderer, Color> redColors = new Dictionary<Renderer, Color>();

        [HideInInspector]
        public StructureFlag flag;

        private void Awake()
        {
            if (preview.layer != LayerMask.NameToLayer("StructurePreview"))
                preview.layer = LayerMask.NameToLayer("StructurePreview");

            Utility.SetActive(preview, true);
            Utility.SetActive(structure, false);

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

            Collider2D[] collider2Ds = preview.GetComponentsInChildren<Collider2D>();
            foreach (Collider2D collider2D in collider2Ds)
            {
                collider2D.isTrigger = true;
            }
        }

        #region 障碍物检测相关
        #region 2D Trigger
        protected override void OnTriggerEnter2D(Collider2D collision)
        {
            if (!Data)
            {
                if (!collision.isTrigger)
                {
                    if (!colliders2D.Contains(collision))
                        colliders2D.Add(collision);
                }
                CheckObstacle();
            }
            else base.OnTriggerEnter2D(collision);
        }

        protected override void OnTriggerStay2D(Collider2D collision)
        {
            if (!Data)
            {
                if (!collision.isTrigger)
                {
                    if (!colliders2D.Contains(collision))
                        colliders2D.Add(collision);
                }
                CheckObstacle();
            }
            else base.OnTriggerStay2D(collision);
        }

        protected override void OnTriggerExit2D(Collider2D collision)
        {
            if (!Data)
            {
                colliders2D.Remove(collision);
                CheckObstacle();
            }
            else base.OnTriggerExit2D(collision);
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

        public void StartBuild(StructureData data)
        {
            Data = data;
            data.preview = this;
            Utility.SetActive(preview, false);
            Utility.SetActive(structure, true);
            transform.position = data.position;
            gameObject.name = Data.Name;
        }

        public void OnBuilt()
        {
            Data = null;
            if (flag) flag.OnBuilt();
            Destroy(gameObject);
        }

        public void OnCancelManage()
        {
            if (Data && !Data.Info.AutoBuild) Data.PauseConstruct();
        }

        public void OnDoneConstruct()
        {
            WindowsManager.HideWindow<StructureManageWindow>(false, this);
        }

        public bool StartConstruct()
        {
            if (Data && !Data.Info.AutoBuild)
            {
                WindowsManager.HideWindow<StructureManageWindow>(true, this);
                Data.StartConstruct();
                return true;
            }
            return false;
        }

        public void PauseConstruct()
        {
            OnDoneConstruct();
            if (Data) Data.PauseConstruct();
        }

        public void PutMaterials(IEnumerable<ItemInfo> materials)
        {
            if (Data) Data.PutMaterials(materials);
        }

        public override bool DoInteract()
        {
            return WindowsManager.OpenWindow<StructureManageWindow>(this);
        }

        protected override void OnNotInteractable()
        {
            if (WindowsManager.IsWindowOpen<StructureManageWindow>(out var manager) && manager.Target == this)
                manager.Interrupt();
        }

        public virtual void Destroy()
        {
            if (flag) flag.Destroy();
            Destroy(gameObject);
        }
    }
}