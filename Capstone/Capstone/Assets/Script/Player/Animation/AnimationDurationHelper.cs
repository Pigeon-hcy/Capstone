using UnityEngine;
using QFramework;

namespace SkateGame
{
    public static class AnimationDuratioSetter
    {
        public static float GetClipLength(Animator animator, string clipName)
        {
            var clips = animator.runtimeAnimatorController.animationClips;
            for (int i = 0; i < clips.Length; i++)
            {
                var c = clips[i];
                if (c != null && c.name == clipName)
                {
                    return c.length;
                }
            }
            return -1f;
        }
    }
}
