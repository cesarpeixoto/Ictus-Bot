/* ==============================================================================================================
Projeto Integrado IV - V
Curso: Tecnologia em Jogos Digitais - 4o/5o Semestres - 2017
Professor: Enric Llagostera
//---------------------------------------------------------------------------------------------------------------
RESUMO GERAL:

Esta classe deriva da classe NetworkEntity, que estabelecem as entidades do jogo, e, por via de consequência, é 
derivada de NetworkBehaviour. Portanto, esta classe deverá desenvolver três papeis de fundamental importância.

Primeiramente, por conta da herança, faz com que o Heroi seja uma entidade no jogo, podendo interagir com outras 
entidades, e ser marcada como alvo. (Atividade exclusiva do nado Client).

A segunda função e a mais importante, se dá em função da arquitetura do jogo, que estabelece o Herói como detentor 
da ClientAuthority na partida. Portanto, ela deverá atuar como GameManager, centralizando as funções de regras 
de negócio (que poderá ser delegada para outras classes não script), como a gestão da economia, monitoramento das 
condições de vitória e derrota, gerenciamento das unidades estáticas e dinâmicas, entre outras (Atividade exclusiva 
do lado Server).

A terceira função, por via de consequência, é fazer a sincronia destas informações, entre o Server e os demais 
Clients, portanto, será detentora das Synchronized Variables, Networked Events, enviar Commands e 
chamadas de Client RPC.

Por razões de conveniência, esta classe seguirá o padrão de projeto Singleton.

//---------------------------------------------------------------------------------------------------------------
DEPENDÊNCIAS:

Class / Struct
SelectorStateType
NetworkDinamicEnity
NetworkSelectorHitState

Components
NetworkPlayerController
NetworkSelectorController
NetworkSelectorRaycaster

//---------------------------------------------------------------------------------------------------------------

$Creator: Cesar Peixoto $
$Notice: (C) Copyright 2017 by Cesar Peixoto. All Rights Reserved. $     Finalizado em 22/03/2017
=============================================================================================================== */


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkHero : NetworkEntity
{
//  ▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀
    // Membros Públicos.

    // Estado atual do cursor do Seletor.
    public SelectorStateType currentState = SelectorStateType.Valid;

//  ▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀
    // Membros Privados.

    // Lista contendo as entidades válidas selecionadas no momento.
    private List<NetworkDinamicEnity> _alliesSlected = new List<NetworkDinamicEnity>();
    // Contem os dados atuais coletados pelo Seletor.
    private NetworkSelectorHitState _currentSelectorHitState;

    // Referência dos componentes do Player
    private NetworkPlayerController _playerController = null;
    private NetworkSelectorController _selectorController = null;
    private NetworkSelectorRaycaster _selectorRaycaster = null;

    // Referência do Cursor do Seletor.
    private Transform _selector = null;


    public static EntityClanType EntityClan { get { return _instance.entityClan; } }
    public static SelectorStateType CurrentState { get { return _instance.currentState; } }
    //public static bool IsLocalPlayer { get { return _instance.isLocalPlayer; } }

    private static NetworkHero _instance = null;
    public static NetworkHero GetInstance() { return _instance; }

    public GameObject testePrefab;

    // Awake is called when the script instance is being loaded
    private void Awake()
    {
        // Inicializa as referências
        _playerController = GetComponent<NetworkPlayerController>();
        _selectorController = GetComponent<NetworkSelectorController>();
    }

    // This is invoked on behaviours that have authority, based on context and the LocalPlayerAuthority value on the NetworkIdentity
    public override void OnStartAuthority()
    {
        // Instancia única é garantida apenas no GameObject que possui LocalPlayerAuthority.
        _instance = this;
    }    

    // Start is called just before any of the Update methods is called the first time
    private void Start()
    {
        if (isLocalPlayer)
        {
            StartCoroutine(FindSelectorRaycaster());
            CmdSpawnMimions();
        }
        else
        {
            //if(!isServer)
            //    DestroyImmediate(this);
            DestroyImmediate(_selectorController);            
        }
    }

    [Command]
    public void CmdSpawnMimions()
    {
        GameObject mimion = (GameObject)Instantiate(testePrefab, this.transform.position + this.transform.forward * 2f, Quaternion.identity);
        NetworkServer.SpawnWithClientAuthority(mimion, this.gameObject);
        mimion.GetComponent<NetworkDinamicEnity>().SetCommander(this.transform);
        //mimion.GetComponent<NetworkIdentity>().AssignClientAuthority(GetComponent<NetworkIdentity>().connectionToClient);
        //mimion.GetComponent<NetworkDinamicEnity>().CmdSelection(true);
    }

    private IEnumerator FindSelectorRaycaster()
    {
        GameObject SelectorCursor = null;
        do
        {
            SelectorCursor = GameObject.FindWithTag("Selector Cursor");
            yield return null;

        } while (SelectorCursor == null);

        _selector = SelectorCursor.transform;
        _selectorRaycaster = _selector.GetComponentInChildren<NetworkSelectorRaycaster>();
    }


    // TODO: Otimizar melhor, o operador da comparação não está funcionando como deveria com os arrays.
    public void SetSelectorState(NetworkSelectorHitState newState)
    {
        _currentSelectorHitState = newState;
        if ((SelectorStateType)newState.selectorState != _instance.currentState)
        {
            this.currentState = (SelectorStateType)newState.selectorState;
            _selectorController.SetColor(this.currentState);
        }
        // _instance._target = AttackPreferenceRules(ref newState.opponent);
    }

    private Entity AttackPreferenceRules(ref Entity[] oppenets)
    {
        if (oppenets == null || oppenets.Length == 0)
            return null;

        return oppenets[0];
    }

    public void SetAction(Vector3 position)
    {
        switch (_instance.currentState)
        {
            case SelectorStateType.Valid:
                {
                    if (_alliesSlected.Count > 0 && _currentSelectorHitState.allies.Length == 0) // precisa checar se não tem unidades para selecionar!!!!
                    {
                        SendCommand(CommandType.GoTo, position);
                    }
                    else if (_currentSelectorHitState.allies.Length > 0)
                    {
                        Select();
                    }
                }
                break;
            case SelectorStateType.Invalid:
                {
                    // TODO: Feedback negativo, de que não pode executar ordem.
                }
                break;
            case SelectorStateType.Opponent:
                {

                }
                break;
            default:
                break;
        }
    }

    public void CancelAction()
    {

    }


    private void SendCommand(CommandType command)
    {
        for (int i = 0; i < _alliesSlected.Count; i++)
        {
            _alliesSlected[i].ReciveCommand(command);
        }
    }

    private void SendCommand(CommandType command, Vector3 position)
    {
        for (int i = 0; i < _alliesSlected.Count; i++)
        {
            _alliesSlected[i].ReciveCommand(command, position);
        }
    }



    public void SendFollow()
    {
        for (int i = 0; i < _alliesSlected.Count; i++)
        {
            if (_alliesSlected[i].currentCommand == CommandType.NoOrder)
                _alliesSlected[i].ReciveCommand(CommandType.Follow);
        }
    }

    private void MoveAction(Vector3 position)
    {
        for (int i = 0; i < _alliesSlected.Count; i++)
        {
            _alliesSlected[i].ReciveCommand(CommandType.GoTo, position);
        }
    }

    private void Select()
    {
        for (int i = 0; i < _currentSelectorHitState.allies.Length; i++)
        {
            if (!_alliesSlected.Contains(_currentSelectorHitState.allies[i]))
            {
                //_instance.transform.GetComponent<NetworkIdentity>().AssignClientAuthority(_instance.currentSelectorHitState.allies[i].GetComponent<NetworkIdentity>().connectionToClient);
                _currentSelectorHitState.allies[i].Select();
                _alliesSlected.Add(_currentSelectorHitState.allies[i]);
            }
        }
    }

    public void DeSelect()
    {
        for (int i = 0; i < _alliesSlected.Count; i++)
        {
            _alliesSlected[i].CancelSelection();            
        }
        _alliesSlected.Clear();
    }

}
