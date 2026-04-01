using UnityEngine;

public class walkSound : MonoBehaviour
{
    private Animator animator;
    void Awake() => animator = GetComponent<Animator>();
    private bool IsActionLayerActive => animator.GetLayerWeight(1) > 0.5f;

    public void _playWalk()
    {
        if (IsActionLayerActive) return;
        Debug.Log("Move SFX " + AudioManager.Instance.fmodCheckMove());
        AudioManager.Instance.fmodPlayWalk();
    }
    public void _walkTrue()
    {
        AudioManager.Instance.fmodMoveTrue();
        Debug.Log("Walk True");
    }
    public void _walkFalse()
    {
        AudioManager.Instance.fmodMoveFalse();
        Debug.Log("Walk False");
    }
}
