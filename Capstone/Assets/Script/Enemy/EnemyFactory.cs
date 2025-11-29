using UnityEngine;

[CreateAssetMenu(fileName = "EnemyFactory", menuName = "Factory/EnemyFactory")]
public class EnemyFactory : ScriptableObject
{
    public EnemyType eType;

    public GameObject CreateEnemy(Vector3 pos, Quaternion rot)
    {
        // 1. 加载 prefab
        string path = PathReference.GetEnemyPathByType(eType);
        GameObject prefab = Resources.Load<GameObject>(path);

        // 2. 安全检查
        if (prefab == null)
        {
            Debug.LogError($"[EnemyFactory] Cannot load prefab at path: {path}. Check Resources folder!");
            return null;
        }

        // 3. 实例化
        GameObject enemy = Instantiate(prefab, pos, rot);
        

        return enemy;
    }

   
}