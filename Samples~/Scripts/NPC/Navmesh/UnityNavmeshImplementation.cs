using System;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Implementation of abstract navmesh component
/// Based around Unity's in-built navmeshAgent
/// Requires Unity navigation mesh to be built for the environment
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
internal class UnityNavmeshImplementation : NavmeshComponent
{
    [SerializeField]
    private NavMeshAgent _agent;

    public override float Velocity => _agent.velocity.magnitude;

    public override bool IsPathing => _currentNavMeshTarget != default;
    public override bool IsApproaching => _isApproaching;

    private Transform _currentNavMeshTarget;

    private bool _isApproaching;

    private void Update()
    {
        if (_currentNavMeshTarget != default &&
            !_agent.pathPending &&
            _agent.remainingDistance <= _agent.stoppingDistance)
        {
            _agent.ResetPath();

            SetHasReachedTarget();

            _currentNavMeshTarget = default;
        }
        else
        {
            _isApproaching = _agent.remainingDistance < _agent.stoppingDistance * 1.2f;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (_agent.hasPath)
        {
            for (int i = 1; i < _agent.path.corners.Length; i++)
            {
                Gizmos.DrawLine(_agent.path.corners[i - 1], _agent.path.corners[i]);
            }
        }
    }

    public override void SetNavMeshTarget(Transform target)
    {
        base.SetNavMeshTarget(target);

        _currentNavMeshTarget = target;
        _agent.SetDestination(_currentNavMeshTarget.position);
        _isApproaching = false;
    }

    public override void SetStoppingDistance(float distance)
    {
        _agent.stoppingDistance = distance;
    }

    public override void ClearGoToTarget()
    {
        _agent.ResetPath();
        _currentNavMeshTarget = default;
        _isApproaching = false;
    }
}