using System.Collections.Generic;
using SkateGame;
using Unity.VisualScripting;
using UnityEngine;

public class BlackShadow : MonoBehaviour
{
    [Header("黑影要去的节点，不算初始位置")]
    public List<Transform> targetSequence = new List<Transform>();

    [Header("变形动画时间")]
    public float transformTime = 0.5f;
    [Header("移动速度")]
    public float moveSpeed = 5f;

    protected BlackShadowSM selfSM;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = FindFirstObjectByType<PlayerController>().transform;
        selfSM = new BlackShadowSM(this);
    }

    //从一个点到另一个点的逻辑
    //首先等待一段时间，变化成一个动画
    //然后移动过去
    //然后在等待一定时间变回来

    // Update is called once per frame
    void Update()
    {
        selfSM.UpdateState();

        //如果在屏幕中就触发dizzy
        if (IsInScreen(Camera.main, transform.position))
            MessageSystem.Instance.Send(PostProTag.Dizzy, this);

    }

    bool IsInScreen(Camera cam, Vector3 worldPos)
    {
        Vector3 v = cam.WorldToViewportPoint(worldPos);
        return v.x >= 0f && v.x <= 1f &&
            v.y >= 0f && v.y <= 1f;
    }

    protected class BlackShadowSM : AbsStatemachine<BlackShadowState, BlackShadow>
    {
        private int index = 0;
        private Transform target;

        private float timeCount = 0;

        public BlackShadowSM(BlackShadow f) : base(f)
        { }

        public override void SwitchWhenStart(BlackShadowState newState)
        {
            base.SwitchWhenStart(newState);
            switch (newState)
            {
                case BlackShadowState.disappear:
                    Destroy(father.gameObject);
                    break;
            }
        }

        public override void SwitchWhenUpdate(BlackShadowState curState)
        {
            base.SwitchWhenUpdate(curState);
            switch (curState)
            {
                case BlackShadowState.empty:
                    //如果被碰到，进入下一个状态;
                    //如果没有，进disappear
                    if (father.IsPlayerNearWrapper())
                    {
                        if (father.targetSequence.Count > index)
                        {
                            target = father.targetSequence[index];
                            index += 1;
                            StartState(BlackShadowState.transforming);
                        }
                        else
                        {
                            StartState(BlackShadowState.disappear);
                        }
                    }
                    break;

                case BlackShadowState.transforming:
                    timeCount += Time.deltaTime;
                    if (timeCount > father.transformTime)
                        StartState(BlackShadowState.moving);
                    break;

                case BlackShadowState.moving:
                    Vector2 pos = father.transform.position;
                    Vector2 targetPos = target.position;

                    Vector2 dir = targetPos - pos;
                    float dist = dir.magnitude;
                    float step = father.moveSpeed * Time.deltaTime;

                    if (dist <= step * 5)
                    {
                        // 吸附到目标
                        father.transform.position = targetPos;
                        StartState(BlackShadowState.transformingBack);
                    }
                    else
                    {
                        // 固定速度前进，不越界
                        father.transform.position = pos + dir.normalized * step;
                    }
                    break;

                case BlackShadowState.transformingBack:
                    timeCount += Time.deltaTime;
                    if (timeCount > father.transformTime)
                        StartState(BlackShadowState.empty);
                    break;

            }
        }

        public override void SwitchWhenEnd(BlackShadowState lastState)
        {
            base.SwitchWhenEnd(lastState);

            timeCount = 0;
        }
    }

    void OnDrawGizmos()
    {
        if (!ifDrawGizmos)
            return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectRadius);
    }

    [Header("检测玩家进行移动设置")]
    public float detectRadius;
    public bool ifDrawGizmos = false;

    private Transform player;

    protected bool IsPlayerNearWrapper()
    {
        if (player == null)
            player = FindFirstObjectByType<PlayerController>().transform;

        if (player == null)
            return false;

        return IsPlayerNear2D(transform, player, detectRadius);
    }

    bool IsPlayerNear2D(Transform self, Transform player, float radius)
    {
        Vector2 a = self.position;
        Vector2 b = player.position;

        return (a - b).sqrMagnitude <= radius * radius;
    }
}

public enum BlackShadowState
{
    empty,
    transforming,
    moving,
    transformingBack,
    disappear
}
