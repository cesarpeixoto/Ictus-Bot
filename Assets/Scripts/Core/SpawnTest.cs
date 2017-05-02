using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SpawnTest : NetworkBehaviour
{

    public static SpawnTest Instance;
    public GameObject[] prefabs;
    public Transform[] targets;

    // Use this for initialization
    void Start()
    {
        if (isLocalPlayer)
            Instance = this;

        //if (isLocalPlayer)
        //{
        //    CmdSpawnMimions(0, null, this.transform.position + this.transform.forward * 5f, this.transform.position + this.transform.forward * 50f);
        //}

    }

    // Update is called once per frame
    void Update()
    {

    }

    [Command]
    public void CmdSpawnMimions(int prefabIndex, GameObject target, Vector3 spawnPoint, Vector3 targetPosition)
    {
        GameObject mimion = (GameObject)Instantiate(prefabs[prefabIndex], spawnPoint, Quaternion.identity);        
        NetworkServer.Spawn(mimion);
        //mimion.GetComponent<NetworkIdentity>().AssignClientAuthority(GetComponent<NetworkIdentity>().connectionToClient);
        mimion.GetComponent<NetworkDinamicEnity>().SetNewDestiantion(targetPosition);
    }
}
