using QFramework;
using UnityEngine;

namespace SkateGame
{
    public interface ICollisionSystem : ISystem
    {
        void GroundCheck(Vector2 position);
        void GroundSnap(Collider2D col, Rigidbody2D rb);
        (bool, float) WallCheck(Vector2 leftPosition, Vector2 rightPosition, float rayDistance);
        bool CheckCrash(Vector2 velocity, float angle);
        (bool, float) CheckFloorBelow(Vector2 origin);
    }

    public class CollisionSystem : AbstractSystem, ICollisionSystem
    {
        private IPlayerModel playerModel;
        protected override void OnInit()
        {
            playerModel = this.GetModel<IPlayerModel>();
        }
        public void GroundCheck(Vector2 position)
        {
            // 使用多个射线检测来提高准确性
            Vector2 rayStart = position;
            Vector2 rayDirection = Quaternion.Euler(0, 0, playerModel.CurrentRotationDeg.Value) * Vector2.down;
            float rayDistance = playerModel.Config.Value.groundCheckDistance; // 减少检测距离，避免误判

            // 主射线检测
            RaycastHit2D hit = Physics2D.Raycast(rayStart, rayDirection, rayDistance, playerModel.Config.Value.groundLayer);

            // 如果主射线没检测到，尝试左右偏移的射线
            if (hit.collider == null)
            {
                Vector2 leftRayStart = rayStart + Vector2.left * playerModel.Config.Value.groundCheckOffset;
                Vector2 rightRayStart = rayStart + Vector2.right * playerModel.Config.Value.groundCheckOffset;
                RaycastHit2D leftHit = Physics2D.Raycast(leftRayStart, rayDirection, rayDistance, playerModel.Config.Value.groundLayer);
                RaycastHit2D rightHit = Physics2D.Raycast(rightRayStart, rayDirection, rayDistance, playerModel.Config.Value.groundLayer);
                if (leftHit.collider != null) hit = leftHit;
                else if (rightHit.collider != null) hit = rightHit;
            }

            bool grounded = hit.collider != null;
            bool slidingWall = false;
            float angle = 0f;
            if (grounded)
            {
                angle = Vector2.Angle(Vector2.up, hit.normal);
                if (angle > playerModel.Config.Value.groundCheckAngle)
                {
                    slidingWall = true;
                    playerModel.SlidingWallAngle.Value = angle * Mathf.Sign(Vector3.Cross(Vector2.up, hit.normal).z);
                    grounded = false;
                }
            }

            // rotate if grounded; clamp per-frame change to avoid single-frame flip at curved ramp bottoms
            float targetAngle = grounded && angle > 0f
                ? angle * Mathf.Sign(Vector3.Cross(Vector2.up, hit.normal).z)
                : 0f;
            playerModel.TargetRotationDeg.Value = Mathf.MoveTowards(
                playerModel.TargetRotationDeg.Value, targetAngle, playerModel.Config.Value.maxRotationSnapDeg);
            playerModel.WasGrounded.Value = playerModel.IsGrounded.Value;
            playerModel.IsGrounded.Value = grounded;
            playerModel.IsSlidingWall.Value = slidingWall;
        }

        public void GroundSnap(Collider2D col, Rigidbody2D rb)
        {
            // Don't snap if moving away from ground surface (e.g. jumping)
            Vector2 groundNormal = (Vector2)(Quaternion.Euler(0, 0, playerModel.CurrentRotationDeg.Value) * Vector2.up);
            if (Vector2.Dot(rb.linearVelocity, groundNormal) > 0f) return;

            Vector2 rayDirection = Quaternion.Euler(0, 0, playerModel.CurrentRotationDeg.Value) * Vector2.down;
            float rayDistance = playerModel.Config.Value.groundCheckDistance;
            int layer = playerModel.Config.Value.groundLayer;

            var filter = new ContactFilter2D();
            filter.SetLayerMask(layer);
            filter.useLayerMask = true;
            RaycastHit2D[] results = new RaycastHit2D[4];
            int count = col.Cast(rayDirection, filter, results, rayDistance);
            for (int i = 0; i < count; i++)
            {
                float a = Vector2.Angle(Vector2.up, results[i].normal);
                if (a <= playerModel.Config.Value.groundCheckAngle)
                {
                    rb.position += rayDirection * results[i].distance;
                    break;
                }
            }
        }
        
        public (bool, float) WallCheck(Vector2 leftPosition, Vector2 rightPosition, float rayDistance)
        {
            Vector2 rayStart = playerModel.IsFacingRight.Value ? rightPosition : leftPosition;
            Vector2 rayDirection = Vector2.right * (playerModel.IsFacingRight.Value ? 1 : -1);
            // 主射线检测
            RaycastHit2D hit = Physics2D.Raycast(rayStart, rayDirection, rayDistance, playerModel.Config.Value.groundLayer);

            // 如果主射线没检测到，尝试向上偏移的射线
            if (hit.collider == null)
            {
                Vector2 upRayStart = rayStart + Vector2.up * playerModel.Config.Value.wallCheckOffset;

                RaycastHit2D upHit = Physics2D.Raycast(upRayStart, rayDirection, rayDistance, playerModel.Config.Value.groundLayer);
                
                if (upHit.collider != null)
                {
                    hit = upHit;
                }
            }
            bool isNearWall = hit.collider != null;
            float angle = 0f;
            if (isNearWall)
            {
                float sign = Mathf.Sign(Vector3.Cross(Vector2.up, hit.normal).z);
                angle = Vector2.Angle(Vector2.up, hit.normal) * sign;
                if (Mathf.Abs(angle) < playerModel.Config.Value.groundCheckAngle) isNearWall = false;
            }
            return (isNearWall, angle);
        }

        public (bool, float) CheckFloorBelow(Vector2 origin)
        {
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, playerModel.Config.Value.wallCheckDistanceFarFast, playerModel.Config.Value.groundLayer);
            bool hitFloor = hit.collider != null && Vector2.Angle(Vector2.up, hit.normal) <= playerModel.Config.Value.groundCheckAngle;
            float floorAngle = hitFloor ? Vector2.Angle(Vector2.up, hit.normal) : 0f;
            return (hitFloor, floorAngle);
        }

        /// <summary>
        /// 检查玩家是否撞墙，检测玩家速度与墙法线方向的夹角，
        /// 如果夹角大于一定角度并且速度大于一定值，则认为玩家撞墙
        /// </summary>
        /// <param name="velocity">玩家速度</param>
        /// <param name="angle">墙角度</param>
        public bool CheckCrash(Vector2 velocity, float angle)
        {
            Vector2 normal = Quaternion.Euler(0f, 0f, angle) * Vector2.up;
            float angleDifference = Mathf.Abs(playerModel.TargetRotationDeg.Value - angle);
            // 计算玩家速度在墙法线方向上的分量
            float velocityTowardWall = Vector2.Dot(velocity, -normal);
            if (velocityTowardWall > playerModel.Config.Value.crashVelocity
                && angleDifference > playerModel.Config.Value.crashAngleDiff
                && Mathf.Abs(angle) > playerModel.Config.Value.crashAngle)
                return true;
            return false;
        }
    }
}
