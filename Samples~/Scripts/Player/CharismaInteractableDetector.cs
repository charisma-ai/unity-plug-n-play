using UnityEngine;

namespace CharismaSDK.PlugNPlay
{
    /// <summary>
    /// Component used to pick up interactable in a Camera's Cone of view
    /// </summary>
    public class CharismaInteractableDetector : MonoBehaviour
    {
        /// <summary>
        /// Current interactable in view
        /// </summary>
        public CharismaInteractableEntity CurrentInteractable => _currentInteractable;

        private CharismaInteractableEntity[] _interactables;

        [ReadOnly]
        [SerializeField]
        private CharismaInteractableEntity _currentInteractable;

        [SerializeField]
        private Camera _playerPerspectiveCamera;

        // Start is called before the first frame update
        void Start()
        {
            GrabAllInteractables();
        }

        // Update is called once per frame
        void Update()
        {
            FindValidInteractable();
            UpdateValidInteractable();
        }

        public void GrabAllInteractables()
        {
            _interactables = FindObjectsOfType<CharismaInteractableEntity>();
        }

        private void FindValidInteractable()
        {
            if (_currentInteractable == default)
            {
                foreach (var interactable in _interactables)
                {
                    if (!interactable.CanBeInteractedWith)
                    {
                        continue;
                    }

                    if (interactable.IsInRange(this.transform.position))
                    {
                        if (IsPointInsideView(interactable.transform.position, _playerPerspectiveCamera.transform.position,
                            _playerPerspectiveCamera.transform.forward, interactable.Range))
                        {
                            _currentInteractable = interactable;
                            return;
                        }
                    }
                }
            }
        }

        private void UpdateValidInteractable()
        {
            if (_currentInteractable != default)
            {
                var outsideOfView = !IsPointInsideView(_currentInteractable.transform.position, _playerPerspectiveCamera.transform.position,
                    _playerPerspectiveCamera.transform.forward, _currentInteractable.Range);
                var outsideOfRange = Vector3.Distance(this.transform.position, _currentInteractable.transform.position) > _currentInteractable.Range;
                var hasBeenInteracted = !_currentInteractable.CanBeInteractedWith;

                // Clear the interactable if its outside of range, view or has been interacted with
                if (outsideOfView
                    || outsideOfRange
                    || hasBeenInteracted)
                {
                    _currentInteractable = default;
                }
            }
        }

        private bool IsPointInsideView(Vector3 point, Vector3 coneOrigin, Vector3 coneDirection, float maxDistance, int maxAngle = 30)
        {
            var distanceToConeOrigin = (point - coneOrigin).magnitude;

#if UNITY_EDITOR
            Debug.DrawLine(coneOrigin, point, Color.red);
            Debug.DrawLine(coneOrigin, coneOrigin + coneDirection * maxDistance, Color.green);
#endif

            if (distanceToConeOrigin < maxDistance)
            {
                var pointDirection = point - coneOrigin;
                var angle = Vector3.Angle(coneDirection, pointDirection);
                if (angle < maxAngle)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
