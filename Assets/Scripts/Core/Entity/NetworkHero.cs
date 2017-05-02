using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkHero : NetworkEntity
{
    private List<NetworkDinamicEnity> _alliesSlected = new List<NetworkDinamicEnity>();
    private Entity _target = null;
    private NetworkSelectorHitState currentSelectorHitState;

    public SelectorStateType currentState = SelectorStateType.Valid;
   

    public static EntityClanType EntityClan { get { return _instance.entityClan; } }
    public static SelectorStateType CurrentState { get { return _instance.currentState; } }

    public NetworkPlayerController _playerController = null;
    public NetworkSelectorController _selectorController = null;
    public NetworkSelectorRaycaster _selectorRaycaster = null;
    private Transform _selector = null;

    public GameObject selectorPrefab = null;

    private static NetworkHero _instance = null;
    public static NetworkHero GetInstance() { return _instance; }

    // Awake is called when the script instance is being loaded
    private void Awake()
    {
        // Fazer a checagem do LocalPLayer aqui!!! Vale a pena todos derivarem de networkbehaviour??        
        _playerController = GetComponent<NetworkPlayerController>();
        _selectorController = GetComponent<NetworkSelectorController>();
                                    
    }

    // Start is called just before any of the Update methods is called the first time
    private void Start()
    {
        if (isLocalPlayer)
        {
            _instance = this;
            GameObject NetworkSelector = (GameObject)Instantiate(selectorPrefab, this.transform.position + Vector3.forward * 2f, this.transform.rotation);
            _selector = NetworkSelector.transform;
            _selectorController.SetSelector(_selector);
            _selectorRaycaster = NetworkSelector.GetComponentInChildren<NetworkSelectorRaycaster>();
        }
        else
        {
            DestroyImmediate(_selectorController);
            DestroyImmediate(this);
            //DestroyImmediate(_selector.gameObject);
        }
    }
    

    // TODO: Otimizar melhor, o operador da comparação não está funcionando como deveria com os arrays.
    public static void SetSelectorState(NetworkSelectorHitState newState)
    {
        _instance.currentSelectorHitState = newState;
        if ((SelectorStateType)newState.selectorState != _instance.currentState)
        {
            _instance.currentState = (SelectorStateType)newState.selectorState;
            _instance._selectorController.SetColor(_instance.currentState);
        }
        // _instance._target = AttackPreferenceRules(ref newState.opponent);
    }

    private static Entity AttackPreferenceRules(ref Entity[] oppenets)
    {
        if (oppenets == null || oppenets.Length == 0)
            return null;

        return oppenets[0];
    }

    public static void Action(Vector3 position)
    {
        switch (_instance.currentState)
        {
            case SelectorStateType.Valid:
                {
                    if (_instance._alliesSlected.Count > 0 && _instance.currentSelectorHitState.allies.Length == 0) // precisa checar se não tem unidades para selecionar!!!!
                    {
                        MoveAction(position);
                    }
                    else if (_instance.currentSelectorHitState.allies.Length > 0)
                    {
                        Select();

                    }
                }
                break;
            case SelectorStateType.Invalid:
                {

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

    public static void CancelAction()
    {

    }


    private static void MoveAction(Vector3 position)
    {
        for (int i = 0; i < _instance._alliesSlected.Count; i++)
        {
            _instance._alliesSlected[i].CmdSetNewDestiantion(position);
        }
    }

    private static void Select()
    {
        for (int i = 0; i < _instance.currentSelectorHitState.allies.Length; i++)
        {
            if (!_instance._alliesSlected.Contains(_instance.currentSelectorHitState.allies[i]))
            {
                _instance.currentSelectorHitState.allies[i].CmdSelection(true);
                _instance._alliesSlected.Add(_instance.currentSelectorHitState.allies[i]);

                // TODO: Implementação de audio.
                // TODO: Implementação de UI; Callback (OnUpdadeHUD??)
            }
        }
        //
    }

    public static void DeSelect()
    {
        for (int i = 0; i < _instance._alliesSlected.Count; i++)
        {
            _instance._alliesSlected[i].CmdSelection(false);
            
        }
        _instance._alliesSlected.Clear();
        //
    }

}
