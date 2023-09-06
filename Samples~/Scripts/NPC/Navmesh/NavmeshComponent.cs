using System;
using UnityEngine;

/// <summary>
/// Abstract navmesh implementation
/// Used in case of custom navmesh/movement implementations
/// </summary>
internal abstract class NavmeshComponent : MonoBehaviour
{
    /// <summary>
    /// Returns if the object has reached the target destination
    /// </summary>
    public bool HasReachedTarget => _hasReachedTarget;

    /// <summary>
    /// Current movement velocity of the gameobject this component is attached to
    /// </summary>
    public abstract float Velocity { get; }

    /// <summary>
    /// Returns if the object has a target destination destination
    /// </summary>
    public abstract bool IsPathing { get; }

    /// <summary>
    /// Returns if the object is actively moving towards the target destination
    /// </summary>
    public abstract bool IsApproaching { get; }

    private bool _hasReachedTarget;

    /// <summary>
    /// Sets the distance for the gameobject to consider its destination reached.
    /// For example, if distance is 0.5, the navmesh should stop when in 0.5 units away from the target
    /// </summary>
    public abstract void SetStoppingDistance(float distance);

    /// <summary>
    /// Removes target destination
    /// </summary>
    public abstract void ClearGoToTarget();

    /// <summary>
    /// Interanl function to set that a target has been reached
    /// Should be used only within Navmesh implementation
    /// </summary>
    internal void SetHasReachedTarget()
    {
        _hasReachedTarget = true;
    }

    /// <summary>
    /// Target navmesh destination setter
    /// </summary>
    public virtual void SetNavMeshTarget(Transform target)
    {
        _hasReachedTarget = false;
    }
}