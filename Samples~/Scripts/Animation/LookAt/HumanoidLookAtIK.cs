using UnityEngine;

namespace CharismaSDK.PlugNPlay
{
    public class HumanoidLookAtIK : BaseLookAt
    {
        private Animator _animator;
        private Transform _headPosition;
        private Transform _npcTransform;
        private Transform _headBone;

        // pass this stuff via Constructor
        private float _weight = 1.0f;
        private float _bodyWeight = 0.1f;
        private float _headWeight = 0.5f;
        private float _eyesWeight = 0.5f;
        private float _clampWeight = 0.7f;

        private float _revertToForwardSpeedMin = .9f;
        private float _revertToForwardSpeedMax = .15f;
        private float _lookAtSpeed = 2f;

        private Vector3 _smoothedLookAtVector;
        private Vector3 _smoothedLookAtPosition;

        private bool _lookAtPositionSet;
        private GameObject _lookAtTarget;
        private Vector3 _lookAtTargetPosition;

        internal HumanoidLookAtIK(Animator animator, Transform parentNpcObject, Transform headPosition)
        {
            _animator = animator;
            _headPosition = headPosition;
            _npcTransform = parentNpcObject;
            _headBone = _animator.GetBoneTransform(HumanBodyBones.Head);
        }

        #region public 
        public override void Update()
        {
            UpdateTargetTracking();
            UpdateIK();
        }

        public override void ClearLookAt()
        {
            _lookAtTarget = default;
            _lookAtPositionSet = false;
        }

        public override bool HasLookAtTarget()
        {
            return _lookAtPositionSet;
        }

        public override bool IsFacingTarget()
        {
            return (_smoothedLookAtPosition - _lookAtTargetPosition).magnitude < 0.3f;
        }

        public override void DisableLookAt()
        {
            var speed = _lookAtSpeed;

            Vector3 myPos = _headBone.position;
            Vector3 targetVector = _headBone.transform.forward;
            Vector3 targetVectorNorm = targetVector.normalized;

            float t = Mathf.Clamp01(Time.deltaTime * speed);

            //Interpolate to the target vector, using spherical interpolation in the XZ plane, and linear interpolation in the Y axis, to simulate our neck's limited range of motion.
            float distance = Mathf.Lerp((_smoothedLookAtPosition - myPos).magnitude, targetVector.magnitude, t);

            _smoothedLookAtVector = _smoothedLookAtVector.XZ()
                .Slerp(targetVectorNorm.XZ(), t)
                .Y2Z(Mathf.LerpAngle(_smoothedLookAtVector.y, targetVectorNorm.y, t)).normalized;

            _smoothedLookAtPosition = myPos + _smoothedLookAtVector * distance;

            _animator?.SetLookAtPosition(_smoothedLookAtPosition);
        }

        public override void SetLookAtTarget(GameObject gameobject, bool instant)
        {
            _lookAtTarget = gameobject;
            _lookAtTargetPosition = gameobject.transform.position;

            if (instant)
            {
                SnapLookToTarget(gameobject.transform.position);
            }

            _lookAtPositionSet = true;
        }

        public override void SetLookAtTarget(Vector3 position, bool instant)
        {
            _lookAtTargetPosition = position;

            if (instant)
            {
                SnapLookToTarget(position);
            }

            _lookAtPositionSet = true;
        }

        #endregion

        #region private

        private void UpdateTargetTracking()
        {
            var speed = _lookAtSpeed;
            if (!_lookAtPositionSet)
            {
                // try to look forward
                DisableLookAt();
                speed = UnityEngine.Random.Range(_revertToForwardSpeedMin, _revertToForwardSpeedMax);
            }
            else
            {
                _lookAtTargetPosition = _lookAtTarget.transform.position;
            }

            if ((_smoothedLookAtPosition - _lookAtTargetPosition).magnitude < 0.3f)
            {
                SnapLookToTarget(_lookAtTargetPosition);
                return;
            }

            Vector3 myPos = _headBone.position;
            Vector3 targetVector = _lookAtTargetPosition - myPos;
            Vector3 targetVectorNorm = targetVector.normalized;

            float t = Mathf.Clamp01(Time.deltaTime * speed);

            //Interpolate to the target vector, using spherical interpolation in the XZ plane, and linear interpolation in the Y axis, to simulate our neck's limited range of motion.
            float distance = Mathf.Lerp((_smoothedLookAtPosition - myPos).magnitude, targetVector.magnitude, t);

            _smoothedLookAtVector = _smoothedLookAtVector.XZ()
                .Slerp(targetVectorNorm.XZ(), t)
                .Y2Z(Mathf.LerpAngle(_smoothedLookAtVector.y, targetVectorNorm.y, t)).normalized;

            _smoothedLookAtPosition = myPos + _smoothedLookAtVector * distance;
        }

        private void UpdateIK()
        {
            if (_animator == default)
            {
                return;
            }

            _animator.SetLookAtPosition(_smoothedLookAtPosition);
            _animator.SetLookAtWeight(_weight, _bodyWeight, _headWeight, _eyesWeight, _clampWeight);
        }

        private void SnapLookToTarget(Vector3 targetPosition)
        {
            _smoothedLookAtPosition = targetPosition;
        }

        #endregion
    }
}
