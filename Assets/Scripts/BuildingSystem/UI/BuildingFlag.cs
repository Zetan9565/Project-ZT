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

    private bool IsValid => building && building.gameObject && building.Data;

    public void Init(BuildingPreview building)
    {
        this.building = building;
        StartCoroutine(WaitToHide());
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
        if (building && building.Data)
        {
            timeText.text = building.Data.IsBuilt ? "已建成" : $"建造中[{building.Data.leftBuildTime.ToString("F2")}s]";
            Vector3 viewportPoint = Camera.main.WorldToViewportPoint(building.transform.position + building.BuildingFlagOffset);
            float sqrDistance = Vector3.SqrMagnitude(Camera.main.transform.position - building.transform.position);
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
    }

    private IEnumerator WaitToHide()
    {
        yield return new WaitUntil(() => !IsValid || building.Data.IsBuilt);
        if (!IsValid)
        {
            StopAllCoroutines();
            ObjectPool.Put(gameObject);
            yield break;
        }
        else yield return new WaitForSeconds(2);
        ObjectPool.Put(gameObject);
        StopAllCoroutines();
    }
}
