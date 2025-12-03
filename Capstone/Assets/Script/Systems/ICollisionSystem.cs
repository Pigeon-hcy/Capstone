using QFramework;
using UnityEngine;

namespace SkateGame
{
    public interface ICollisionSystem : ISystem
    {
        void GroundCheck(Vector2 position);
        (bool, float) WallCheck(Vector2 leftPosition, Vector2 rightPosition, float rayDistance);
        void CheckCrash(Vector2 velocity, float angle);
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
            Vector2 rayDirection = Vector2.down;
            rayDirection = Quaternion.Euler(0, 0, playerModel.CurrentRotationDeg.Value) * rayDirection;
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

                if (leftHit.collider != null)
                {
                    hit = leftHit;
                }
                else if (rightHit.collider != null)
                {
                    hit = rightHit;
                }
            }
            
            bool grounded = hit.collider != null;
            float angle = 0f;
            if (grounded)
            {
                angle = Vector2.Angle(Vector2.up, hit.normal);
                if (angle > playerModel.Config.Value.groundCheckAngle) grounded = false;
            }

            // rotate if grounded
            playerModel.TargetRotationDeg.Value = grounded && angle > 0f 
                ? angle * Mathf.Sign(Vector3.Cross(Vector2.up, hit.normal).z)
                : 0f;
            playerModel.WasGrounded.Value = playerModel.IsGrounded.Value;
            playerModel.IsGrounded.Value = grounded;
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
         public void CheckCrash(Vector2 velocity, float angle)
         {
            Vector2 normal = Quaternion.Euler(0f, 0f, angle) * Vector2.up;
            float relativeVelocity = Vector2.Dot(velocity, -normal);
            if (relativeVelocity > playerModel.Config.Value.crashVelocity)
            {
                /*
                 need to add crash effect
                */
                var respawnSystem = this.GetSystem<IRespawnSystem>();
                respawnSystem.RespawnPlayer();
            }
         }
    }   
}
