using UnityEngine;

public class PatrolPoint : MonoBehaviour
{
    public bool WaitForRotate = false;
    public bool EndPatrol = false;
    [Tooltip("Enable this to true on the final patrol point if this route is circular (if enemy should immediately go to the first point after this instead of backtracking)")]
    public bool CircularRoute = false;
    [Tooltip("Enable this to true if you want this node to be skipped when going backwards through patrol")]
    public bool SkipOnBackwards = false;
    [Tooltip("How long the enemy waits when arriving at this point")]
    public float WaitTime = 0;
    [Tooltip("(Angler Only) Set to false for patrol points you don't want enemy to turn around to (ones in corners, basically, to prevent player from getting cornered)")]
    public bool CanTurnAround = true;
}
