using UnityEngine;

public class ChaseController : MonoBehaviour
{
    public ChaseEnemy chaseEnemy;
    public enum EnterMode {
        StartChase,
        StopChase,
        BeginChase
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
                        chaseEnemy.gameObject.SetActive(true);
                        chaseEnemy.RestartChase();
                        break;
                    case EnterMode.StopChase:
                        chaseEnemy.StopChase();
                        break;
                    case EnterMode.BeginChase:
                        chaseEnemy.gameObject.SetActive(true);
                        chaseEnemy.StartCoroutine(chaseEnemy.BeginChase());
                        break;
                }
            }
        }
    }
}
