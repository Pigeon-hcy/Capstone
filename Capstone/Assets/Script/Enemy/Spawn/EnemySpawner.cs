using System.Collections.Generic;
using SkateGame;
using Unity.Mathematics;
using UnityEngine;

public class EnemySpawner : MessageBehavior
{
    public List<EnemyWave> waves;

    private int indexCount = 0;

    private List<GameObject> recordSpawnedEn;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SafeRegister(EnemyMessage.Die,EnemyDieHandler);
        StartSpawn();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void StartSpawn()
    {
        indexCount = 0;
        SpawnAWave(indexCount);
    }

    private void SpawnAWave(int index)
    {
        recordSpawnedEn = new List<GameObject>();
        EnemyWave wave = waves[Mathf.Clamp(index,0,waves.Count-1)];
        IReadOnlyList<OneTypeSpawnInfo> spawnInfos = wave.GetSpawnInfos();
        if(spawnInfos==null|| spawnInfos.Count == 0)
            return;

        foreach(OneTypeSpawnInfo info in spawnInfos)
        {
            GameObject prefab = Resources.Load<GameObject>(PathReference.GetEnemyPathByType(info.eType));
            foreach(Vector3 pos in info.spawnPoses)
            {
                recordSpawnedEn.Add(Instantiate(prefab, pos, quaternion.identity));
            }
            
        }
    }

    public void EnemyDieHandler(MessageBox box, MonoBehaviour sender)
    {
        if(recordSpawnedEn==null)
            return;

        if(recordSpawnedEn.Contains(box.gmo))
            recordSpawnedEn.Remove(box.gmo);

        if(recordSpawnedEn.Count ==0)
            OneWaveFinish();
    }

    private void OneWaveFinish()
    {
        indexCount+=1;
        if(indexCount>=waves.Count)
        {
            FightClear();
            return;
        }
            
        SpawnAWave(indexCount);

    }

    private void FightClear()
    {
        Debug.Log("You have done all the fight!");
    }
}
