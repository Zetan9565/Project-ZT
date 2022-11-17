using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using ZetanStudio;
using ZetanStudio.StructureSystem;

public class StructureFlag : MonoBehaviour
{
    [SerializeField]
    private Text timeText;

    public new Transform transform { get; private set; }

    private CanvasGroup canvasGroup;

    private StructurePreview2D structure;

    private Vector3 targetPos;

    private bool IsValid => structure && structure.gameObject && structure.Data;

    public void Init(StructurePreview2D structure)
    {
        this.structure = structure;
        structure.flag = this;
        Update();
    }

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (!canvasGroup) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvasGroup.blocksRaycasts = false;
        transform = base.transform;
    }

    void Update()
    {
        if (IsValid)
        {
            timeText.text = !structure.Data.IsBuilding ? $"等待中\n[剩余{structure.Data.leftBuildTime:F2}s]" : $"建造中\n[剩余{structure.Data.leftBuildTime:F2}s]";
            targetPos = structure.transform.position + structure.StructureFlagOffset;
        }
        Vector3 viewportPoint = Camera.main.WorldToViewportPoint(targetPos);
        float sqrDistance = Vector3.SqrMagnitude(Camera.main.transform.position - targetPos);
        if (viewportPoint.z <= 0 || viewportPoint.x > 1 || viewportPoint.x < 0 || viewportPoint.y > 1 || viewportPoint.y < 0 || sqrDistance > 900f)
        {
            canvasGroup.alpha = 0;
        }
        else
        {
            Vector2 position = new Vector2(Screen.width * viewportPoint.x, Screen.height * viewportPoint.y);
            transform.position = position;
            if (sqrDistance > 625 && sqrDistance <= 900)
            {
                float percent = (900 - sqrDistance) / 275;
                canvasGroup.alpha = percent;
                transform.localScale = new Vector3(percent, percent, 1);
            }
            else
            {
                canvasGroup.alpha = 1;
                transform.localScale = Vector3.one;
            }
        }
    }

    private IEnumerator WaitToHide(float time)
    {
        yield return new WaitForSeconds(time);
        StopAllCoroutines();
        ObjectPool.Put(gameObject);
    }

    public void OnBuilt()
    {
        timeText.text ="已建成";
        StartCoroutine(WaitToHide(2));
    }

    public void Destroy()
    {
        StopAllCoroutines();
        structure = null;
        ObjectPool.Put(gameObject);
    }
}
