using UnityEngine;


namespace BulletToolKit
{
    public class GravityDirectionMoveComp : DirectionMoveCompo
    {
        [SerializeField]
        private float gravity = -7f;

        private float verticalVelocity;

        protected override void Update()
        {
            if (!canMove) return;

            // 重力累积
            verticalVelocity += gravity * Time.deltaTime;

            // 组合速度
            Vector3 velocity =
                moveDir * speed +
                Vector3.up * verticalVelocity;

            transform.position += velocity * Time.deltaTime;

            // 朝向仍然只看水平方向（不被重力影响）
            if (moveDir.sqrMagnitude > 0.0001f)
            {
                transform.rotation =
                    Quaternion.LookRotation(moveDir) * Quaternion.Euler(0, -90, 0);
            }

            lifeTime -= Time.deltaTime;
            if (lifeTime <= 0f)
            {
                Destroy(gameObject);
            }
        }

        public void ResetGravity()
        {
            verticalVelocity = 0f;
        }
    }
}
