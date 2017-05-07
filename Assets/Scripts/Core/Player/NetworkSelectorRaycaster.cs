using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;

public class NetworkSelectorRaycaster : MonoBehaviour
{
    public float heightOffset = 10f;
    private Vector3 origin = Vector3.zero;

    private int _navMeshWalkableLayer = 0;
    private int _physicsEntityLayer = 0;
    private int _physicsEnvironmentLayer = 0;
    public static float surfaceHeight = 0.0f;

    // Use this for initialization
    void Start()
    {
        SetOriginHitPoint();
        _navMeshWalkableLayer = NavMesh.GetAreaFromName("Walkable");
        _physicsEntityLayer = LayerMask.NameToLayer("Entity");
        _physicsEnvironmentLayer = LayerMask.NameToLayer("Environment");
    }

    private void FixedUpdate()
    {
        /* Por razões de performance, optamos por utilizar strucs (memória Stack), mandetendo a politica
           de tolerancia zero de alocação de memória em Loops. */

        surfaceHeight = 0.0f;
        SetOriginHitPoint();
        RaycastHit hit;
        if (Physics.Raycast(origin, -Vector3.up, out hit, heightOffset))
        {
            if (hit.collider.gameObject.layer == _physicsEnvironmentLayer)
                surfaceHeight = hit.point.y - this.transform.position.y;

            NetworkSelectorHitState hitState = new NetworkSelectorHitState();

            NavMeshHit navHit;
            NavMesh.Raycast(hit.point, hit.point - Vector3.up, out navHit, NavMesh.AllAreas);
            if (navHit.mask == 1 << _navMeshWalkableLayer)
            {
                Collider[] hitColliders = Physics.OverlapSphere(this.transform.position, 1.8f, 1 << _physicsEntityLayer);
                if (hitColliders.Length > 0)
                {
                    int alliesCount = 0;
                    int opponentsCount = 0;
                    int[] alliesIndexs = new int[5];
                    int[] opponentsIndexs = new int[5];
                    NetworkDinamicEnity[] entity = new NetworkDinamicEnity[hitColliders.Length];
                    for (int i = 0; i < hitColliders.Length; i++)
                    {
                        entity[i] = hitColliders[i].gameObject.GetComponent<NetworkDinamicEnity>();
                        Assert.IsFalse(entity[i] == null);                                      // Se alguma coisa errada estiver na layer Entity

                        if (entity[i].entityClan == NetworkHero.EntityClan)
                        {
                            alliesIndexs[alliesCount] = i;
                            ++alliesCount;
                        }
                        else
                        {
                            opponentsIndexs[alliesCount] = i;
                            ++opponentsCount; ;
                        }
                    }
                    hitState.allies = new NetworkDinamicEnity[alliesCount];
                    for (int i = 0; i < alliesCount; i++)
                        hitState.allies[i] = entity[alliesIndexs[i]];

                    hitState.opponent = new NetworkDinamicEnity[opponentsCount];
                    for (int i = 0; i < opponentsCount; i++)
                        hitState.opponent[i] = entity[opponentsIndexs[i]];

                    if (opponentsCount > 0)
                        hitState.selectorState = (int)SelectorStateType.Opponent;
                    else
                        hitState.selectorState = (int)SelectorStateType.Valid;
                }
            }
            else
            {
                hitState.selectorState = (int)SelectorStateType.Invalid;
            }
            if (hitState.allies == null)
                hitState.allies = new NetworkDinamicEnity[0];
            if (hitState.opponent == null)
                hitState.opponent = new NetworkDinamicEnity[0];
            NetworkHero.GetInstance().SetSelectorState(hitState);
        }
    }


    private void SetOriginHitPoint()
    {
        origin.Set(this.transform.position.x, heightOffset, this.transform.position.z);
    }
}
