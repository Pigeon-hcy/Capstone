using UnityEngine;
using SkateGame;
using MoreMountains.Feedbacks;

/// <summary>
/// 实例化后在场景中查找玩家并持续跟随（玩家较晚生成时会每帧重试直到找到）。
/// </summary>
public class High_Speed_P : MonoBehaviour
{
    public bool facingRight = true;

    public MMF_Player highSpeedEffect;
    public ParticleSystem highSpeedP;
    [SerializeField]
    Vector3 worldOffset;

    [Tooltip("0 = 每帧与目标位置重合；大于 0 时用 SmoothDamp 平滑跟随")]
    [SerializeField]
    float smoothTime = 0;

    Transform _player;
    Vector3 _smoothVelocity;

    public float localScaleRight = 4f;
    public float localScaleLeft = 4f +180f;

    void Start()
    {
        TryBindPlayer();
    }

    void LateUpdate()
    {
        if(facingRight)
        {
            transform.localScale = new Vector3(transform.localScale.x, localScaleRight, transform.localScale.z);
        }
        else
        {
            transform.localScale = new Vector3(transform.localScale.x, localScaleLeft, transform.localScale.z);
        }
        if (_player == null)
        {
            TryBindPlayer();
            return;
        }

        Vector3 targetPos = _player.position + worldOffset;
        if (smoothTime > 0f)
        {
            transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref _smoothVelocity, smoothTime);
        }
        else
        {
            transform.position = targetPos;
        }
    }

    void TryBindPlayer()
    {
        PlayerController pc = Object.FindFirstObjectByType<PlayerController>();
        if (pc != null)
        {
            _player = pc.transform;
            if (highSpeedEffect != null)
                pc.highSpeedWindEffect = highSpeedEffect;
            return;
        }

        GameObject tagged = GameObject.FindGameObjectWithTag("Player");
        if (tagged != null)
            _player = tagged.transform;
    }

    public void PlayHighSpeedP()
    {
        highSpeedP.Play();
    }

    public void StopHighSpeedP()
    {
        highSpeedP.Stop();
    }
}
