using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BaseUtility;

namespace SkateGame
{
    public class DongDongEnemy : BasicEnemyController
    {
        private float _checkRange;
        private float _fallTime;
        private AnimationCurve _fallCurve;
        private AnimationCurve _resetCurve;
        private float _resetTime;
        private float _resetDelay;

        public Detector detector;
        public Transform shootPosTrans;
        private Vector3 _startPos;
        private Vector3 _tarPos;
        private float _time;
        
        private bool _attacking = false;
        private Coroutine _atkCoroutine;

        private bool _getPlayer = false;
        
        protected override void Start()
        {
            base.Start();
            var cfg = config as DongEnemyConfig;
            _checkRange = cfg.checkWidth;
            _fallCurve = cfg.fallCurve;
            _fallTime = cfg.fallTime;
            _resetCurve = cfg.resetCurve;
            _resetTime = cfg.resetTime;
            _resetDelay = cfg.resetDelay;
            SetFallRange();
            
            detector.Open(new List<string>(new []{"Player"}), _=>
            {
                _getPlayer = true;
            });
        }

        protected override void Guard(Transform pTrans, float guard)
        {
            base.Guard(pTrans, guard);
            GuardProcess = 0;
            //检测pTrans是否在一定范围内
            if (_getPlayer && !_attacking)
                GuardProcess = 1;
            //如果在就把guard process设置为1
        }

        protected override void AtkTowardsPlayer(Transform pTrans)
        {
            _attacking = true;
            _getPlayer = false;
            detector.Close();
            _atkCoroutine = StartCoroutine(FallAndResetCoroutine());
        }
        
        private IEnumerator FallAndResetCoroutine()
        {
            // 如果落点需要动态算（通常是这样）
            SetFallRange();
            if(dmgBox == null)
            {
                dmgBox = reportBoxFactory.CreateHitbox(transform);
            }
            dmgBox.OpenBox(AtkTags, new EffectPackage(0),new Vector3(1.1f,1.1f,1.1f) );
            
            // 下砸
            _startPos = transform.position;
            _time = 0f;

            while (_time < _fallTime)
            {
                _time += Time.deltaTime;
                float t = Mathf.Clamp01(_time / _fallTime);
                float curveT = _fallCurve.Evaluate(t);

                transform.position = Vector3.Lerp(_startPos, _tarPos, curveT);
                yield return null;
            }

            transform.position = _tarPos;

            dmgBox.CloseBox();
            dmgBox = null;
            // 落地延迟
            if (_resetDelay > 0f)
                yield return new WaitForSeconds(_resetDelay);

            // 回位
            _time = 0f;

            while (_time < _resetTime)
            {
                _time += Time.deltaTime;
                float t = Mathf.Clamp01(_time / _resetTime);
                float curveT = _resetCurve.Evaluate(t);

                transform.position = Vector3.Lerp(_tarPos, _startPos, curveT);
                yield return null;
            }

            transform.position = _startPos;

            // 完整一次攻击结束
            _attacking = false;
            _atkCoroutine = null;
            detector.Open(new List<string>(new []{"Player"}), _=> _getPlayer = true);
        }

        private void SetFallRange()
        {
            _startPos = transform.position;
            detector.transform.position = shootPosTrans.position;
            Vector3 desiredScale = new Vector3(_checkRange, 1, 1);
            
            Vector2 origin = shootPosTrans.position;
            Vector2 direction = Vector2.down;

            RaycastHit2D hit = Physics2D.Raycast(
                origin,
                direction,
                Mathf.Infinity,
                GroundLayer
            );

            if (hit.collider != null)
            {
                float distance = origin.y - hit.point.y;
                desiredScale.y = distance + 0.5f;
                _tarPos = hit.point + Vector2.up * (transform.localScale.y * 0.5f);
            }

            // 如果你是要直接用 scale
            detector.transform.localScale = desiredScale;
            
        }
    }
}

