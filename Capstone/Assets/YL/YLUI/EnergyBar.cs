using QFramework;
using SkateGame;
using UnityEngine;
using System.Collections.Generic;

public class EnergyBar : MonoBehaviour, IController
{
    IEnergySystem energySystem;

    [SerializeField] List<EnergyBarChild> energyBarChildren;

    public IArchitecture GetArchitecture() => GameApp.Interface;

    void Start()
    {
        energySystem = this.GetSystem<IEnergySystem>();
        UpdateBar(energySystem.Energy.Value);
        energySystem.Energy.Register(UpdateBar).UnRegisterWhenGameObjectDestroyed(gameObject);
    }

    public void UpdateBar(int amount) //amount must be 0, 1, 2, 3
    {
        if (energyBarChildren.Count < amount)
        {
            Debug.LogWarning("Need More Energy Bar Children");
            return;
        }

        for (int i = 0; i < energyBarChildren.Count; i++)
        {
            if (i < amount)
            {
                energyBarChildren[i].Activate();
            }
            else energyBarChildren[i].Deactivate();
        }
    }

    public void DebugInt(int num)
    {
        UpdateBar(num);
    }
}
