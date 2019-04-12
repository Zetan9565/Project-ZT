using UnityEngine;

public class KillTestButton : MonoBehaviour {

    public Enemy enermy;

    public void OnClick()
    {
        enermy.Death();
    }
}
