using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using CustonNetwork;


public class MenuManager : MonoBehaviour
{
    public enum MenuWindow { MainMenu, P2PMenu, LocalLanMenu, Lobby, Servers }
    private static MenuManager _instance = null;

    public InputField roomName = null;

    public RectTransform clientsContent = null;
    public RectTransform serversContent = null;
    public GameObject serverData = null;

    public float teste = 0.0f;
    private LobbyManager _lobbyManager = null;
    public MenuItensBase<MenuWindow> menuWindow = null;
    private void Awake()
    {
        _instance = this;
        _lobbyManager = LobbyManager.GetInstance();
        menuWindow = new MenuItensBase<MenuWindow>(this.transform);
    }

    public static int GetClientsContentCount()
    {
        return _instance.clientsContent.childCount;
    }

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    public static void RegisterNewClient(LobbyPlayer newClient)
    {
        newClient.transform.SetParent(_instance.clientsContent, false);
    }

    public void RequestServerList()
    {
        _lobbyManager.StartMatchMaker();
        _lobbyManager.matchMaker.ListMatches(0, 5, "", true, 0, 0, DrawServerList); // Cada pagina contém 5 partidas.
        ChangeWindowTo(MenuWindow.Servers);
    }

    // Nem preciso falar que tudo isso vai ser refeito né!!!!!
    public void DrawServerList(bool success, string extendedInfo, List<UnityEngine.Networking.Match.MatchInfoSnapshot> matches)
    {
        if(success)
        {
            if(matches.Count == 0)
            {
                Debug.Log("Nenhum Servidor Encontrado.");
            }
            else
            {
                // Limpa o quadro atual.
                for (int i = 0; i < serversContent.childCount; i++)
                {
                    Destroy(serversContent.gameObject);
                }

                for (int i = 0; i < matches.Count; i++)
                {
                    GameObject server = (GameObject)Instantiate(serverData);
                    server.transform.SetParent(serversContent, false);
                    server.transform.GetChild(0).GetComponent<Text>().text = matches[i].name;
                    server.transform.GetChild(1).GetComponent<Text>().text = "Players " + matches[i].currentSize.ToString() + "/" + matches[i].maxSize.ToString();
                    server.transform.GetChild(2).GetComponent<Button>().onClick.RemoveAllListeners();
                    UnityEngine.Networking.Types.NetworkID networkID = matches[i].networkId;

                    server.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(() => { JoinMatch(networkID, _lobbyManager); });
                }

            }
        }
        else
        {
            Debug.Log("Erro ao ao buscar servidores: " + extendedInfo);
        }
    }

    private void JoinMatch(UnityEngine.Networking.Types.NetworkID networkID, LobbyManager lobbyManager)
    {
        lobbyManager.matchMaker.JoinMatch(networkID, "", "", "", 0, 0, lobbyManager.OnMatchJoined);
        //lobbyManager.backDelegate = lobbyManager.StopClientClbk;
        //lobbyManager._isMatchmaking = true;
        //lobbyManager.DisplayIsConnecting();
    }

    public static void ChangeWindowTo(MenuWindow newWindow)
    {
        _instance.menuWindow.ChangeWindowTo(newWindow);
    }

    public void Close()
    {
        menuWindow.CloseRootMenu();
    }

    public void Open()
    {
        menuWindow.OpenRootMenu();
    }

    [EnumAction(typeof(MenuWindow))]
    public void ChangeWindowTo(int newWindow)
    {
        ChangeWindowTo((MenuWindow)newWindow);
    }

    public void BackMenu()
    {
        menuWindow.ReturnWindow();
    }

    // Parte de servidor

    // Botão que cria a sala para partida.
    public void OnClickCreateMatchmakingGame()
    {
        // Para debug, e talvez para fazer um log.
        Debug.Log("Iniciando o Serviço Unity MatchMaker - Host: " + _lobbyManager.matchHost + " na porta: " + _lobbyManager.matchPort);
        _lobbyManager.StartMatchMaker();

        Debug.Log("Solicitando ao Unity MatchMaker Services a criação de uma sala de nome " + roomName.text + " para " + _lobbyManager.maxPlayers + " jogadores\n " +
                  " do tipo aberta, sem requisição de password para partida, sem restrição de habilidade e sem restrição de dominio.");

        _lobbyManager.matchMaker.CreateMatch(roomName.text, (uint)_lobbyManager.maxPlayers, true, "", "", "", 0, 0, _lobbyManager.OnMatchCreate);

        // TODO: Exibir aquela caixa padrão com a mensagem conectando...
        // TODO: Passar um callback de cancelamento para o botão da janela dessa mensagem.
    }


}


//[AttributeUsage(AttributeTargets.Method)]
//public class EnumActionAttribute : PropertyAttribute
//{
//    public Type enumType;

//    public EnumActionAttribute(Type enumType)
//    {
//        this.enumType = enumType;
//    }
//}
