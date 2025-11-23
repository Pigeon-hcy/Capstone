using UnityEngine;

public class ChaseEnemy : MonoBehaviour
{

    public Transform player;
    public float ChaseSpeedMin = 5f;
    public float ChaseSpeedMax = 10f;
    public float DistanceAfterSpawn;
    public float minDistance = 2f; // 最小距离，此时使用ChaseSpeedMin
    public float maxDistance = 10f; // 最大距离，此时使用ChaseSpeedMax
    public float decelerationRate = 5f; // 减速率
    
    public bool isChasing = false;
    private float currentSpeed = 0f; // 当前移动速度

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 如果没有指定player，尝试查找玩家
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isChasing && player != null)
        {
            // 计算与玩家的距离
            float distance = Vector3.Distance(transform.position, player.position);
            
            // 根据距离计算速度（距离越近速度越慢，距离越远速度越快）
            float normalizedDistance = Mathf.Clamp01((distance - minDistance) / (maxDistance - minDistance));
            currentSpeed = Mathf.Lerp(ChaseSpeedMin, ChaseSpeedMax, normalizedDistance);
            
            // 向右移动（x轴正方向）
            Vector3 moveDirection = Vector3.right;
            transform.position += moveDirection * currentSpeed * Time.deltaTime;
            
            // 跟随玩家的y轴
            Vector3 newPosition = transform.position;
            newPosition.y = player.position.y;
            transform.position = newPosition;
        }
        else if (!isChasing && currentSpeed > 0f)
        {
            // 缓速停下：逐渐减速
            currentSpeed -= decelerationRate * Time.deltaTime;
            currentSpeed = Mathf.Max(0f, currentSpeed); // 确保速度不为负
            
            // 继续向右移动直到速度降为0
            Vector3 moveDirection = Vector3.right;
            transform.position += moveDirection * currentSpeed * Time.deltaTime;
        }
    }

    public void StartChase()
    {
        isChasing = true;
    }

    public void StopChase()
    {
        isChasing = false;
    }

    public void ResetChaser()
    {
        // 将chaser放置在距离玩家DistanceAfterSpawn的位置之后启动chase
        if (player != null)
        {
            Vector3 spawnPosition = player.position;
            spawnPosition.x -= DistanceAfterSpawn; // 在玩家左侧DistanceAfterSpawn距离处生成
            spawnPosition.y = player.position.y; // 保持相同的y轴位置
            transform.position = spawnPosition;
            StartChase();
        }
    }
}
