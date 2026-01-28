using MoreMountains.Tools;
using QFramework;
using SkateGame;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class SkillBarController : MonoBehaviour, IController
{
    [SerializeField] MMProgressBar mmProgressBar;
    IEnergySystem energySystem;

    public IArchitecture GetArchitecture() => GameApp.Interface;

    void Start()
    {
        energySystem = this.GetSystem<IEnergySystem>();
        UpdateBar(energySystem.Energy.Value);
        energySystem.Energy.Register(UpdateBar).UnRegisterWhenGameObjectDestroyed(gameObject);
    }

    public void UpdateBar(int amount) //amount must be 0, 1, 2, 3
    {
        float value = (float)amount * 0.3f;
        mmProgressBar.UpdateBar(value, 0, 0.9f);
    }

    #region ForDebugging
    public void Debug0()
    {
        UpdateBar(0);
    }

    public void Debug1()
    {
        UpdateBar(1);
    }

    public void Debug2()
    {
        UpdateBar(2);
    }

    public void Debug3()
    {
        UpdateBar(3);
    }
    #endregion
}
