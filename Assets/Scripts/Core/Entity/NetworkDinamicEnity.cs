using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Networking;

public class NetworkDinamicEnity : NetworkEntity
{
    private NavMeshAgent _navAgent = null;
    private GameObject selection = null;

    // Awake is called when the script instance is being loaded
    private void Awake()
    {
        _navAgent = GetComponent<NavMeshAgent>();
        selection = this.transform.Find("Select").gameObject;
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
        CmdSetNewDestiantion(position);
    }

    [Command]
    public void CmdSetNewDestiantion(Vector3 position)
    {
        RpcSetNewDestiantion(position);
    }

    [ClientRpc]
    private void RpcSetNewDestiantion(Vector3 position)
    {
        _navAgent.SetDestination(position);
    }

    [Command]
    public void CmdSelection(bool condition)
    {
        RpcSelection(condition);
    }

    [ClientRpc]
    public void RpcSelection(bool condition)
    {
        selection.SetActive(condition);
        if(_navAgent.hasPath)
        {
            _navAgent.Stop();
            _navAgent.ResetPath();
        }
    }
}
