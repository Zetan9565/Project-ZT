using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BuildingFlag : MonoBehaviour
{
    [SerializeField]
    private Text timeText;

    public new Transform transform { get; private set; }

    private CanvasGroup canvasGroup;

    private BuildingPreview building;

    private Vector3 targetPos;

    private bool IsValid => building && building.gameObject && building.Data;

    public void Init(BuildingPreview building)
    {
        this.building = building;
        building.flag = this;
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
            timeText.text = !building.Data.IsBuilding ? $"等待中\n[剩余{building.Data.leftBuildTime:F2}s]" : $"建造中\n[剩余{building.Data.leftBuildTime:F2}s]";
            targetPos = building.transform.position + building.BuildingFlagOffset;
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
        building = null;
        ObjectPool.Put(gameObject);
    }
}
