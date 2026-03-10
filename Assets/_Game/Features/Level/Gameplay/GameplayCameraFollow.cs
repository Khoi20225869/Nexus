using UnityEngine;

namespace Game.Features.Level.Gameplay
{
    public sealed class GameplayCameraFollow : MonoBehaviour
    {
        [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);
        [SerializeField] [Min(0f)] private float smoothTime = 0.05f;

        private Transform _target;
        private Vector3 _velocity;

        public void SetTarget(Transform target, bool snapImmediately = false)
        {
            _target = target;
            if (snapImmediately && _target != null)
            {
                transform.position = _target.position + offset;
                _velocity = Vector3.zero;
            }
        }

        private void LateUpdate()
        {
            if (_target == null)
            {
                return;
            }

            var targetPosition = _target.position + offset;
            if (smoothTime <= 0f)
            {
                transform.position = targetPosition;
                return;
            }

            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _velocity, smoothTime);
        }
    }
}
