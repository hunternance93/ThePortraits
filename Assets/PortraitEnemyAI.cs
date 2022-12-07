using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PortraitEnemyAI : MonoBehaviour
{
    private NavMeshAgent agent;
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.destination = Camera.main.transform.position;
    }

    private void Update()
    {
        agent.destination = Camera.main.transform.position;
    }
}
