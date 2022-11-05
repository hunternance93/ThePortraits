using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stream : MonoBehaviour
{
    private LineRenderer lineRenderer = null;
    private ParticleSystem splashParticles = null;

    private Coroutine pourRoutine = null;
    private Vector3 targetPosition = Vector3.zero;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        splashParticles = GetComponentInChildren<ParticleSystem>();
    }

    private void Start()
    {
        MoveToPosition(0, transform.position);
        MoveToPosition(1, transform.position);
    }

    public void Begin()
    {
        StartCoroutine(UpdateParticle());
        pourRoutine = StartCoroutine(BeginPour());

    }

    private IEnumerator BeginPour()
    {
        while (gameObject.activeSelf)
        {
            targetPosition = FindEndpoint();

            MoveToPosition(0, transform.position);
            AnimateToPosition(1, targetPosition);

            yield return null;
        }
        Destroy(gameObject);
    }

    public void End()
    {
        StopCoroutine(pourRoutine);
        pourRoutine = StartCoroutine(EndPour());
    }

    private IEnumerator EndPour()
    {
        while (!HasReachedPosition(0, targetPosition))
        {
            AnimateToPosition(0, targetPosition);
            AnimateToPosition(1, targetPosition);
        }
        yield return null;
    }

    private Vector3 FindEndpoint()
    {
        RaycastHit hit;
        Ray ray = new Ray(transform.position, Vector3.down);

        Physics.Raycast(ray, out hit, 2.0f);
        Vector3 endpoint = hit.collider ? hit.point : ray.GetPoint(2.0f);

        return endpoint;
    }

    private void MoveToPosition(int index, Vector3 targetPosition)
    {
        lineRenderer.SetPosition(index, targetPosition);
    }

    private void AnimateToPosition(int index, Vector3 targetPosition)
    {
        Vector3 currentPoint = lineRenderer.GetPosition(index);
        Vector3 newPositon = Vector3.MoveTowards(currentPoint, targetPosition, Time.deltaTime * 1.75f);
        lineRenderer.SetPosition(index, newPositon);
    }
    private bool HasReachedPosition(int index, Vector3 targetPosition)
    {
        Vector3 currentPosition = lineRenderer.GetPosition(index);

        return currentPosition == targetPosition;
    }

    private IEnumerator UpdateParticle()
    {
        while (gameObject.activeSelf)
        {
            splashParticles.gameObject.transform.position = targetPosition;

            bool isHitting = HasReachedPosition(1, targetPosition);
            splashParticles.gameObject.SetActive(isHitting);

            yield return null;
        }
    }
}
