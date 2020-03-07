using UnityEngine;
using UnityEngine.UI;

public class BuildingFlag : MonoBehaviour
{
    [SerializeField]
    private Text timeText;

    private CanvasGroup canvasGroup;

    private Building building;
    // Start is called before the first frame update
    public void Init(Building building)
    {
        this.building = building;
        Update();
    }

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (!canvasGroup) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvasGroup.blocksRaycasts = false;
    }

    void Update()
    {
        if (!building || !building.gameObject) ObjectPool.Put(gameObject);
        else
        {
            timeText.text = "建造中[" + building.leftBuildTime.ToString("F2") + "s]";
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
        if (building.IsBuilt)
        {
            timeText.text = "已建成";
            ObjectPool.Put(gameObject, 2);
        }
    }
}
