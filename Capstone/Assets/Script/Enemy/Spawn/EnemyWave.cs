using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyWave", menuName = "Scriptable Objects/EnemyWave")]
public class EnemyWave : ScriptableObject
{

   [SerializeField] 
    private OneTypeSpawnInfo[] infos = new OneTypeSpawnInfo[6];
     // 只公开你想给别的系统访问的内容
    public IReadOnlyList<OneTypeSpawnInfo> GetSpawnInfos()
    {
        List<OneTypeSpawnInfo> result = new List<OneTypeSpawnInfo>();

        foreach (var info in infos)
        {
            if (info == null) continue;
            if (info.spawnPoses == null || info.spawnPoses.Count == 0) continue;

            result.Add(info);
        }

        return result;
    }
}

public enum EnemyType
{
    BasicEnemy,
    BoostEnemy,
    FlyEnemy,
    ThrowEnemy,
    DongEnemy
}

[Serializable]
public class OneTypeSpawnInfo
{
    public EnemyType eType;
    public List<Vector3> spawnPoses;
}