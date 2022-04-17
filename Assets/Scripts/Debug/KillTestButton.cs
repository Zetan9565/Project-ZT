using UnityEngine;

public class KillTestButton : MonoBehaviour
{
    public EnemyInformation enemy;

    public void OnClick()
    {
        foreach (var enemy in FindObjectsOfType<Enemy>())
            if (enemy.Info == this.enemy)
            {
                enemy.Death();
                break;
            }
    }
}