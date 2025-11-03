using SkateGame;
using UnityEngine;
using QFramework;


public class PlayerAnimationManager : MonoBehaviour, ICanGetModel, IBelongToArchitecture
{
    public Animator animator;
    public SpriteRenderer spriteRenderer;
    public IPlayerModel playerModel;

    public IArchitecture GetArchitecture() => GameApp.Interface;

    void Start()
    {
        playerModel = this.GetModel<IPlayerModel>();
    }

    // Update is called once per frame
    void Update()
    {
        if (playerModel.IsGrounded.Value)
        {
            spriteRenderer.flipX = Input.GetAxisRaw("Horizontal") < 0 ? true : Input.GetAxisRaw("Horizontal") > 0 ? false : spriteRenderer.flipX;
        }

    }
}
