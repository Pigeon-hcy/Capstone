using UnityEngine;

namespace BaseUtility
{
    public enum TempActions
    {
        KillPlayer
    }

    public static class DamageSystem
    {
        public static void ProcessDamage( EffectPackage package, IAttackable target)
        {
            if (package.directKillPlayer)
            {
                Debug.Log("PlayerDie!");
                MessageSystem.Instance.Send(TempActions.KillPlayer, null);
            }

            
        }
    }

    public interface IAttackable
    {
    }

}