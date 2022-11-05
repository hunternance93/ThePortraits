using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Shout out to "VR With Andrew" on YT for the tutorial

public class PourDetector : MonoBehaviour
{
    // Set this in inspector to determine the spill point.
    [SerializeField] private float pourThreshhold = 0f;

    public Transform spoutMouth = null;
    private LineRenderer waterStream = null;

    [SerializeField] private Animator animator = null;

    [SerializeField] private float animatorNormalizedTime = 0f;
    [SerializeField] private float currentAngle = 0f;

    [SerializeField] private float waterSpeed = 5.0f;
    [SerializeField] private float spillDownThreshold = 0.36f;
    [SerializeField] private float stopSpillingThreshhold = 0.78f;

    private Vector3 waterLanding = Vector3.zero;

    public void Start()
    {
        currentAngle = spoutMouth.rotation.x * Mathf.Rad2Deg;

        waterStream = GetComponentInChildren<LineRenderer>();
        waterStream.SetPosition(0, spoutMouth.position);
        waterStream.SetPosition(1, spoutMouth.position);

        waterLanding = new Vector3(spoutMouth.position.x, spoutMouth.position.y - 1.0f, spoutMouth.position.z);
    }

    public void Update()
    {
        waterLanding.x = spoutMouth.position.x;
        waterLanding.y = spoutMouth.position.y - 1.0f;
        waterLanding.z = spoutMouth.position.z;

        animatorNormalizedTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
        animatorNormalizedTime = animatorNormalizedTime - (int)(animatorNormalizedTime);

        currentAngle = spoutMouth.rotation.x * Mathf.Rad2Deg;

        if (Mathf.Abs(currentAngle) < pourThreshhold)
        {
            // Shishi is not spilling yet
            waterStream.SetPosition(0, spoutMouth.position);
            waterStream.SetPosition(1, spoutMouth.position);
        }
        else
        {
            if (animatorNormalizedTime > spillDownThreshold &&
                animatorNormalizedTime < stopSpillingThreshhold)
            {
                // Shishi is spilling water, so stream should start from top-down
                waterStream.SetPosition(0, spoutMouth.position);
                waterStream.SetPosition(1, Vector3.MoveTowards(waterStream.GetPosition(1), waterLanding, Time.deltaTime * waterSpeed));
            }
            else if (animatorNormalizedTime > stopSpillingThreshhold)
            {
                // Shishi is coming up, so water stream should end from top-down
                waterStream.SetPosition(0, Vector3.MoveTowards(waterStream.GetPosition(0), waterStream.GetPosition(1), Time.deltaTime * waterSpeed));
                waterStream.SetPosition(1, waterStream.GetPosition(1));
            }
        }
    }

}
