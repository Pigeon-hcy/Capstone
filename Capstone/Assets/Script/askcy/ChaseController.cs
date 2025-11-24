using UnityEngine;

public class ChaseController : MonoBehaviour
{
    public ChaseEnemy chaseEnemy;
    public enum EnterMode {
        StartChase,
        StopChase
    }

    public EnterMode enterMode = EnterMode.StartChase;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (chaseEnemy != null)
            {
                switch (enterMode)
                {
                    case EnterMode.StartChase:
                        chaseEnemy.StartChase();
                        break;
                    case EnterMode.StopChase:
                        chaseEnemy.StopChase();
                        break;
                }
            }
        }
    }
}
