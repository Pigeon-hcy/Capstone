using System.Collections;
using UnityEngine;

public static class PathReference 
{
    public static string ReportBoxPath = "ReportBox";

    public static string GetEnemyPathByType(EnemyType type)
    {
        return type switch
        {
            EnemyType.BasicEnemy => "BasicEnemy",
            EnemyType.BoostEnemy => "BoostEnemy",
            _=>"BasicEnemy"
        };
    }
}
