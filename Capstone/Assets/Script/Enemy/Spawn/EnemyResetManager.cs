using System.Collections.Generic;
using UnityEngine;
using BaseUtility;

namespace SkateGame
{
    public class EnemyResetManager : MessageBehavior
    {
        [System.Serializable]
        private class EnemyInitData
        {
            public BasicEnemyController enemy;
            public Vector3 pos;
            public Quaternion rot;
        }

        public List<BasicEnemyController> enemyToReset;
        public Transform inLevelEnemyParent;
        
        private List<EnemyInitData> _initialData = new List<EnemyInitData>();

        private void Start()
        {
            // 注册重置事件
            SafeRegister(GameStateEnum.PlayerRespawn, ResetHandler);

            // 记录每个敌人的初始状态
            _initialData.Clear();
            foreach (Transform child in inLevelEnemyParent)
            {
                BasicEnemyController enemy = child.GetComponentInChildren<BasicEnemyController>();
                if (enemy != null)
                {
                    enemyToReset.Add(enemy);
                }
            }
            foreach (var en in enemyToReset)
            {
                if (en == null) continue;
                _initialData.Add(new EnemyInitData
                {
                    enemy = en,
                    pos = en.transform.position,
                    rot = en.transform.rotation
                });
            }
        }

        private void ResetHandler(MessageBox box, MonoBehaviour sender)
        {
            //Debug.LogError("ResetHandler");
            // 1. 先删除所有当前敌人
            foreach (var data in _initialData)
            {
                if (data.enemy != null)
                    Destroy(data.enemy.gameObject);
            }

            // 2. 重新生成
            for (int i = 0; i < _initialData.Count; i++)
            {
                var data = _initialData[i];
                var prefabRef = data.enemy.prefabRef;

                if (prefabRef == null || prefabRef == null)
                {
                    Debug.LogError($"Enemy {data.enemy.name} missing PrefabRef or its prefab is null!");
                    continue;
                }

                // 生成新的敌人

                GameObject newEnemy = prefabRef.CreateEnemy(data.pos, data.rot);
                    

                // 更新数组引用为新的敌人（以便下一次 Reset 使用）
                BasicEnemyController newController = newEnemy.GetComponentInChildren<BasicEnemyController>();
                _initialData[i].enemy = newController;
                enemyToReset[i] = newController;
            }
        }
    }
}

