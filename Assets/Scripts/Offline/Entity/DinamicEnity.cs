using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DinamicEnity : Entity
{
    private NavMeshAgent _navAgent = null;

    // Awake is called when the script instance is being loaded
    private void Awake()
    {
        _navAgent = GetComponent<NavMeshAgent>();
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetNewDestiantion(Vector3 position)
    {
        _navAgent.SetDestination(position);
    }
}
