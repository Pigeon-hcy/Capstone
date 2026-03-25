using QFramework;
using UnityEngine;

public interface IEnergySystem : ISystem
{
    /// <summary>
    /// 当前能量（0 ~ MaxEnergy）
    /// </summary>
    BindableProperty<int> Energy { get; }

    /// <summary>
    /// 最大能量
    /// </summary>
    int MaxEnergy { get; }

    void AddEnergy(int amount = 1);
    bool ConsumeEnergy(int amount = 1);
    void ResetEnergy();
    bool HasEnoughEnergy(int amount = 1);
}

public class EnergySystem : AbstractSystem, IEnergySystem
{
    public int MaxEnergy => 1;

    public BindableProperty<int> Energy { get; }
        = new BindableProperty<int>(0);

    protected override void OnInit()
    {
        ResetEnergy();
    }

    #region Public Methods

    public void AddEnergy(int amount = 1)
    {
        if (amount <= 0) return;

        Energy.Value = Mathf.Min(MaxEnergy, Energy.Value + amount);
    }

    public bool ConsumeEnergy(int amount = 1)
    {
        if (!HasEnoughEnergy(amount))
            return false;

        Energy.Value -= amount;
        return true;
    }

    public void ResetEnergy()
    {
        Energy.Value = MaxEnergy;
    }

    public bool HasEnoughEnergy(int amount = 1)
    {
        return Energy.Value >= amount;
    }

    #endregion
}