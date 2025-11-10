using QFramework;
using UnityEngine;

namespace SkateGame
{
    public interface ICollisionSystem : ISystem
    {
        void GroundCheck(Vector2 position);
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
            playerModel.WasGrounded.Value = playerModel.IsGrounded.Value;
            playerModel.IsGrounded.Value = grounded;
            if (grounded)
            {
                float angle = Vector2.Angle(Vector2.up, hit.normal);
                float sign = Mathf.Sign(Vector3.Cross(Vector2.up, hit.normal).z);
                playerModel.TargetRotationDeg.Value = angle * sign;
            }
            else
            {
                playerModel.TargetRotationDeg.Value = 0f;
            }
        }
    }
}
