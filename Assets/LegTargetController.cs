using System.Collections.Generic;
using System;
using UnityEngine;

[System.Serializable]
public class LegsTransformData
{
    public Transform Target;
    public Transform Hint;

    [NonSerialized] public Vector3 startAnimationPosition = Vector3.zero;
    [NonSerialized] public float _movingAnimationTimeLeft = 0;
    [NonSerialized] public Vector3 RealTarget;
    [NonSerialized] public Vector3 TargetShift;
    [NonSerialized] public Vector3 HintShift;
    [NonSerialized] public float cooldownTime; // Cooldown time for independent movement
    [NonSerialized] public bool isMoving; // Check if this leg is currently moving
}

public class LegTargetController : MonoBehaviour
{
    public float distancetomove = 0;
    [SerializeField] private List<LegsTransformData> _legsTargets;
    [SerializeField] private Transform _spider;
    [SerializeField] private float _legsMoveAnimationTime = 0.5f;
    [SerializeField] private float _heightLegsAnimation = 0.3f;
    [SerializeField] private AnimationCurve _legsHeightAnimationCurve;
    [SerializeField] private float _cooldownDuration = 0.5f; // Cooldown duration for each leg to start moving independently

    void Start()
    {
        foreach (var legsTarget in _legsTargets)
        {
            legsTarget.TargetShift = legsTarget.Target.position - _spider.position;
            legsTarget.HintShift = legsTarget.Hint.position - _spider.position;
            legsTarget.cooldownTime = UnityEngine.Random.Range(0, _cooldownDuration); // Randomize initial cooldowns for staggered movement
            legsTarget.isMoving = false;
        }
    }

    void Update()
    {
        bool anyLegIsMoving = false;

        foreach (var legsTarget in _legsTargets)
        {
            // Update the RealTarget and Hint positions based on the spider's movement
            legsTarget.RealTarget = _spider.position + legsTarget.TargetShift;
            legsTarget.Hint.position = _spider.position + legsTarget.HintShift;

            // Perform a raycast to find the ground position for the RealTarget
            if (Physics.Raycast(legsTarget.RealTarget + Vector3.up * 3, Vector3.down, out RaycastHit hit, 6f))
            {
                legsTarget.RealTarget = hit.point + Vector3.down * 0.02f;
            }

            // Check if this leg is currently moving
            if (legsTarget.isMoving)
            {
                anyLegIsMoving = true; // Set the flag if any leg is currently moving
                MoveLegTowardsTarget(legsTarget); // Continue moving the leg towards its RealTarget
            }
            else if (!anyLegIsMoving && legsTarget.cooldownTime <= 0)
            {
                // If no other leg is moving, check if this leg needs to start moving
                float distance = Vector3.Distance(legsTarget.Target.position, legsTarget.RealTarget);
                if (distance > distancetomove) // Start moving if the leg is far from its target
                {
                    StartLegMovement(legsTarget);
                    anyLegIsMoving = true; // Set this leg as the active moving leg
                }
            }
            else
            {
                // If not moving, count down the cooldown timer
                legsTarget.cooldownTime -= Time.deltaTime;
            }
        }
    }

    private void StartLegMovement(LegsTransformData legsTarget)
    {
        // Initialize leg movement
        legsTarget._movingAnimationTimeLeft = _legsMoveAnimationTime;
        legsTarget.startAnimationPosition = legsTarget.Target.position;
        legsTarget.isMoving = true;
        legsTarget.cooldownTime = _cooldownDuration; // Reset cooldown for staggered effect
    }

    private void MoveLegTowardsTarget(LegsTransformData legsTarget)
    {
        // Animate the leg towards the RealTarget
        float factor = 1 - legsTarget._movingAnimationTimeLeft / _legsMoveAnimationTime;
        legsTarget._movingAnimationTimeLeft -= Time.deltaTime;

        Vector3 position = Vector3.Lerp(legsTarget.startAnimationPosition, legsTarget.RealTarget, factor);
        position.y += Mathf.Lerp(0, _heightLegsAnimation, _legsHeightAnimationCurve.Evaluate(factor));
        legsTarget.Target.position = position;

        // Stop the movement when time is up
        if (legsTarget._movingAnimationTimeLeft <= 0)
        {
            legsTarget.isMoving = false;
        }
    }
}
