using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Networking;

public class NetworkDinamicEnity : NetworkEntity
{
    public CommandType currentCommand = CommandType.NoOrder;
    private NavMeshAgent _navAgent = null;

    private Transform _commander = null;
    private GameObject selection = null;

    public bool selected = false;


    // Awake is called when the script instance is being loaded
    private void Awake()
    {
        _navAgent = GetComponent<NavMeshAgent>();
        selection = this.transform.Find("Select").gameObject;
    }

    // Update is called every frame, if the MonoBehaviour is enabled
    private void Update()
    {
        if (currentCommand == CommandType.NoOrder)
            return;

        if (!_navAgent.hasPath || _navAgent.isPathStale && !selected)
        {
            // Parar de se mover.
            //CmdStopMove();
            currentCommand = CommandType.NoOrder;
        }
    }

    public void SetCommander(Transform commander)
    {
        _commander = commander;
    }

    public void Select()
    {
        selected = true;
        CmdSelect(true);
    }

    public void CancelSelection()
    {
        selected = false;
        CmdSelect(false);
    }

    public void ReciveCommand(CommandType command)
    {
        switch (command)
        {
            case CommandType.NoOrder:
                break;
            case CommandType.Follow:
                if (currentCommand == CommandType.Follow && _navAgent.hasPath)
                    return;                
                CmdSetFollow(_commander.position);
                
                break;
            case CommandType.CancelOrder:
                break;
            default:
                break;
        }
    }

    public void ReciveCommand(CommandType command, Vector3 position)
    {
        switch (command)
        {
            case CommandType.GoTo:
                currentCommand = CommandType.GoTo;
                CmdSetNewDestiantion(position);
                break;
            case CommandType.Attack:
                // Definir como será o ataque!!!
                break;
            default:
                break;
        }
    }


    [Command]
    public void CmdStopMove()
    {
        RpcStopMove();
    }

    [ClientRpc]
    public void RpcStopMove()
    {
        if (_navAgent.isActiveAndEnabled)
        {
            _navAgent.isStopped = true;
            _navAgent.ResetPath();
        }
    }

    [Command]
    public void CmdSetFollow(Vector3 position)
    {
        RpcSetFollow(position);
    }

    [ClientRpc]
    private void RpcSetFollow(Vector3 position)
    {
        _navAgent.stoppingDistance = 4f;
        _navAgent.SetDestination(position);
    }

    [Command]
    public void CmdSetNewDestiantion(Vector3 position)
    {
        RpcSetNewDestiantion(position);
    }

    [ClientRpc]
    private void RpcSetNewDestiantion(Vector3 position)
    {
        _navAgent.stoppingDistance = 0f;
        _navAgent.SetDestination(position);
    }

    [Command]
    public void CmdSelect(bool condition)
    {
        RpcSelect(condition);
    }

    [ClientRpc]
    public void RpcSelect(bool condition)
    {
        selection.SetActive(condition);
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
