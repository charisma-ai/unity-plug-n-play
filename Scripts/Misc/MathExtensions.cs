using UnityEngine;


namespace CharismaSDK.PlugNPlay
{
    #region Helper Methods
    public static partial class MathExtensions
    {
        public static Vector3 Y2Z(this Vector2 input, float newY = 0)
        {
            return new Vector3(input.x, newY, input.y);
        }

        public static Vector2Int XZ(this Vector3Int input)
        {
            return new Vector2Int(input.x, input.z);
        }

        public static Vector2 XZ(this Vector3 input)
        {
            return new Vector2(input.x, input.z);
        }

        public static Vector3Int Y2Z(this Vector2Int input, int newY)
        {
            return new Vector3Int(input.x, newY, input.y);
        }

        public static Vector2 Slerp(this Vector2 a, Vector2 b, float t)
        {
            float dot = Vector2.Dot(a, b);
            dot = Mathf.Clamp(dot, -1f, 1f);
            float theta = Mathf.Acos(dot) * t;
            Vector2 relativeVec = b - a * dot;
            relativeVec.Normalize();
            return (a * Mathf.Cos(theta)) + relativeVec * Mathf.Sin(theta);
        }

        public static float SnappedSmoothDampTo(this float current, float target,
            ref float currentVelocity, float snapDistance, ref bool moving, float smoothTime,
            bool angle = false) => SnappedSmoothDampTo(current, target, ref currentVelocity,
            snapDistance, ref moving,
            smoothTime, Mathf.Infinity, angle);

        public static float SnappedSmoothDampTo(this float current, float target,
            ref float currentVelocity, float snapDistance, ref bool moving, float smoothTime,
            float maxSpeed, bool angle = false)
        {
            if (angle)
            {
                target = target.UnwrapAngle() % 360;
            }

            if (Mathf.Abs(target - current) < snapDistance)
            {
                moving = false;
                currentVelocity = 0;
                return target;
            }
            else
            {
                moving = true;
                return angle
                    ? Mathf.SmoothDampAngle(current, target, ref currentVelocity, smoothTime, maxSpeed)
                    : Mathf.SmoothDamp(current, target, ref currentVelocity, smoothTime, maxSpeed);
            }
        }

        public static float UnwrapAngle(this float angle)
        {
            if (angle >= 0)
                return angle;

            angle = -angle % 360;

            return 360 - angle;
        }
    }

    #endregion
}
