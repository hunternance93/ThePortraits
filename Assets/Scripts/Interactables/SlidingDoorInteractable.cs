using System.Collections;
using UnityEngine;

public class SlidingDoorInteractable : MonoBehaviour, IInteractable
{
    [Tooltip("Sound effect that will play when the door is opened")]
    [SerializeField] private AudioSource openNoise = null;
    [Tooltip("How long does it take for door to open/close")]
    [SerializeField] private float doorAnimationLength = .5f;
    [SerializeField] private Transform leftDoor = null;
    [SerializeField] private Transform rightDoor = null;
    [SerializeField] private float distanceToSlide = 1;
    [SerializeField] private bool reverseLeft = false;
    [SerializeField] private bool reverseRight = false;
    [SerializeField] private bool startOpen = false;

    private int reverseLeftInt = 1;
    private int reverseRightInt = 1;

    private bool doorOpen = false;
    private Coroutine doorRoutine = null;

    private void Awake()
    {
        if (reverseLeft) reverseLeftInt = -1;
        if (reverseRight) reverseRightInt = -1;
        if (startOpen) InstantlyOpen();
    }

    public void Interacted()
    {
        if (doorRoutine != null) return;

        if (openNoise != null) openNoise.Play();

        if (doorOpen) doorRoutine = StartCoroutine(CloseSlidingDoor());
        else doorRoutine = StartCoroutine(OpenSlidingDoor());
    }

    private IEnumerator OpenSlidingDoor()
    {
        doorOpen = true;
        float timer = 0;
        Vector3 startingLeftDoorPos = leftDoor.localPosition;
        Vector3 startingRightDoorPos = rightDoor.localPosition;
        do
        {
            timer += Time.deltaTime;
            leftDoor.localPosition = new Vector3(startingLeftDoorPos.x, startingLeftDoorPos.y, Mathf.Lerp(startingLeftDoorPos.z, startingLeftDoorPos.z - (distanceToSlide * reverseLeftInt), timer / doorAnimationLength));
            rightDoor.localPosition = new Vector3(startingRightDoorPos.x, startingRightDoorPos.y, Mathf.Lerp(startingRightDoorPos.z, startingRightDoorPos.z + (distanceToSlide * reverseRightInt), timer / doorAnimationLength));
            yield return null;
        } while (timer < doorAnimationLength);
        leftDoor.localPosition = new Vector3(startingLeftDoorPos.x, startingLeftDoorPos.y, startingLeftDoorPos.z - (distanceToSlide * reverseLeftInt));
        rightDoor.localPosition = new Vector3(startingRightDoorPos.x, startingRightDoorPos.y, startingRightDoorPos.z + (distanceToSlide * reverseRightInt));
        doorRoutine = null;
    }

    private IEnumerator CloseSlidingDoor()
    {
        doorOpen = false;
        float timer = 0;
        Vector3 startingLeftDoorPos = leftDoor.localPosition;
        Vector3 startingRightDoorPos = rightDoor.localPosition;
        do
        {
            timer += Time.deltaTime;
            leftDoor.localPosition = new Vector3(startingLeftDoorPos.x, startingLeftDoorPos.y, Mathf.Lerp(startingLeftDoorPos.z, startingLeftDoorPos.z + (distanceToSlide * reverseLeftInt), timer / doorAnimationLength));
            rightDoor.localPosition = new Vector3(startingRightDoorPos.x, startingRightDoorPos.y, Mathf.Lerp(startingRightDoorPos.z, startingRightDoorPos.z - (distanceToSlide * reverseRightInt), timer / doorAnimationLength));
            yield return null;
        } while (timer < doorAnimationLength);
        leftDoor.localPosition = new Vector3(startingLeftDoorPos.x, startingLeftDoorPos.y, startingLeftDoorPos.z + (distanceToSlide * reverseLeftInt));
        rightDoor.localPosition = new Vector3(startingRightDoorPos.x, startingRightDoorPos.y, startingRightDoorPos.z - (distanceToSlide * reverseRightInt));
        doorRoutine = null;
    }

    private void InstantlyOpen()
    {
        doorOpen = true;
        leftDoor.localPosition = new Vector3(leftDoor.localPosition.x, leftDoor.localPosition.y, leftDoor.localPosition.z - (distanceToSlide * reverseLeftInt));
        rightDoor.localPosition = new Vector3(rightDoor.localPosition.x, rightDoor.localPosition.y, rightDoor.localPosition.z + (distanceToSlide * reverseRightInt));
    }
}
