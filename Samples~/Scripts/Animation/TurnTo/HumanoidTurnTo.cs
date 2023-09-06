using UnityEngine;

namespace CharismaSDK.PlugNPlay
{
    public class HumanoidTurnTo : BaseTurnTo
    {
        private Animator _animator;

        private Transform _npcTransform;

        private HumanoidNPCAnimationConfig _config;

        private const int ROTATE_ANGLE_CUTOFF = 45;

        private static readonly int _rotateHash = Animator.StringToHash("Rotate");

        private bool _turnToTargetSet;
        private GameObject _turnToTarget;
        private Vector3 _turnToTargetPosition;

        public HumanoidTurnTo(Animator animator, Transform parentNpcObject, HumanoidNPCAnimationConfig config)
        {
            _animator = animator;
            _npcTransform = parentNpcObject;
            _config = config;
        }

        public override void Update()
        {
            UpdateTargetOrientation();
        }

        public override void SetTurnToTarget(GameObject gameObject)
        {
            _turnToTarget = gameObject;
            _turnToTargetPosition = gameObject.transform.position;
            _turnToTargetSet = true;
        }

        public override void SetTurnToTarget(Vector3 position)
        {
            _turnToTargetPosition = position;
            _turnToTargetSet = true;
        }

        public override void ClearTurnTo()
        {
            _turnToTarget = default;
            _turnToTargetSet = false;
            _animator.applyRootMotion = false;
        }

        private void UpdateTargetOrientation()
        {
            if (!_turnToTargetSet)
            {
                return;
            }

            float combinedEulerAngles = _animator.transform.eulerAngles.y;
            Vector3 targetPos = _turnToTarget == default ? _turnToTargetPosition : _turnToTarget.transform.position;

            float angleToTarget = Mathf.DeltaAngle(Vector2.SignedAngle(
                    _npcTransform.position.XZ() - targetPos.XZ(),
                    Vector2.down),
                combinedEulerAngles);

            _animator.applyRootMotion = true;

            switch (angleToTarget)
            {
                case > 180 - ROTATE_ANGLE_CUTOFF:
                    _animator.SetInteger(_rotateHash, 180);
                    break;

                case < -180 + ROTATE_ANGLE_CUTOFF:
                    _animator.SetInteger(_rotateHash, -180);
                    break;

                case >= ROTATE_ANGLE_CUTOFF:
                    _animator.SetInteger(_rotateHash, 90);
                    break;

                case <= -ROTATE_ANGLE_CUTOFF:
                    _animator.SetInteger(_rotateHash, -90);
                    break;

                default:
                    _animator.SetInteger(_rotateHash, 0);
                    break;
            }

            // apply new rotation to parent of animator controller
            var animatorPosition = _animator.transform.position;
            _npcTransform.transform.rotation = _animator.transform.rotation;
            _animator.transform.localRotation = Quaternion.identity;
            _animator.transform.position = animatorPosition;
        }

    }
}
