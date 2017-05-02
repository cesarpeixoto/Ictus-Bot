using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Networking;
using UnityEngine.Networking.Match;

namespace CustonNetwork
{
    public class LobbyManager : NetworkLobbyManager
    {
        [Header("Players Prefabs")]
        [Space(5)]
        public GameObject[] playerPrefabs = null;

        private Dictionary<int, LobbyPlayer.RobotClan> clanIntentity = new Dictionary<int, LobbyPlayer.RobotClan>();


        private static LobbyManager _instance = null;
        public static LobbyManager GetInstance() { return _instance; }



        // https://docs.unity3d.com/ScriptReference/MonoBehaviour.Awake.html
        // Awake is called when the script instance is being loaded        
        private void Awake()
        {
            if(_instance != null)
            {
                Destroy(this.gameObject);
            }
            else
            {
                _instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
        }

        // Use this for initialization
        void Start()
        {
            // Registra os prefabs para serem usados pelo cliente remoto.
            for (int i = 0; i < playerPrefabs.Length; i++)
                ClientScene.RegisterPrefab(playerPrefabs[i]);
        }

        // https://docs.unity3d.com/ScriptReference/Networking.NetworkManager.OnMatchCreate.html
        // Callback that happens when a NetworkMatch.CreateMatch request has been processed on the server.
        public override void OnMatchCreate(bool success, string extendedInfo, MatchInfo matchInfo)
        {
            // Para debug, e talvez para fazer um log.
            if(success)
            {
                Debug.Log("Sala criada com sucesso, IP do Host: " + matchInfo.address + " na porta: " + matchInfo.port + " Serviço  Relay server = " + matchInfo.usingRelay);
                Debug.Log("Sua sala aparecerá no lobby de outros jogadores em busca de uma boa partida.");
            }
            else
            {
                Debug.Log("Erro na criação da sala, mesangem original: " + extendedInfo);
            }
            base.OnMatchCreate(success, extendedInfo, matchInfo);
        }

        // https://docs.unity3d.com/ScriptReference/Networking.NetworkManager.StartHost.html
        // This hook is invoked when a host is started.
        public override void OnStartHost()
        {
            Debug.Log("Iniciando Host, a partida irá começar quando o número necessário de jogadores ingressarem na sala.");
            base.OnStartHost();

            // TODO: Receber o código de abrir o Lobby por um Delegate, para deixar tudo bem customizavel, sem acoplamento da classe de menu

            // Alterar para o menu de Lobby.. Configurar a saida de lá como cancelamento da partida, etc, etc..
            MenuManager.ChangeWindowTo(MenuManager.MenuWindow.Lobby);  // Altera o fluxo para o Lobby.
        }


        // TODO: Trabalhar os métodos para desconectar o server, como fica o client nisso, ou so server rejeitar um client.

        // https://docs.unity3d.com/ScriptReference/Networking.NetworkLobbyManager.OnLobbyServerCreateLobbyPlayer.html
        // This allows customization of the creation of the lobby-player object on the server.
        public override GameObject OnLobbyServerCreateLobbyPlayer(NetworkConnection conn, short playerControllerId)
        {            
            // TODO: Aqui é um bom lugar para cusmomizar o LobbyPLayer
            // com como está a coisa lá no lobby, etc...

            // Observação do código deles é:
            //we want to disable the button JOIN if we don't have enough player
            //But OnLobbyClientConnect isn't called on hosting player. So we override the lobbyPlayer creation

            // Código original do Lobby deles
            //GameObject obj = Instantiate(lobbyPlayerPrefab.gameObject) as GameObject;

            //LobbyPlayer newPlayer = obj.GetComponent<LobbyPlayer>();
            //newPlayer.ToggleJoinButton(numPlayers + 1 >= minPlayers);


            //for (int i = 0; i < lobbySlots.Length; ++i)
            //{
            //    LobbyPlayer p = lobbySlots[i] as LobbyPlayer;

            //    if (p != null)
            //    {
            //        p.RpcUpdateRemoveButton();
            //        p.ToggleJoinButton(numPlayers + 1 >= minPlayers);
            //    }
            //}

            //return obj;

            // Para saber, lobbySlots é da classe base, que é https://docs.unity3d.com/ScriptReference/Networking.NetworkLobbyManager-lobbySlots.html



            return base.OnLobbyServerCreateLobbyPlayer(conn, playerControllerId);
        }

        // https://docs.unity3d.com/ScriptReference/Networking.NetworkManager.OnMatchJoined.html
        // Callback that happens when a NetworkMatch.JoinMatch request has been processed on the server.
        public override void OnMatchJoined(bool success, string extendedInfo, MatchInfo matchInfo)
        {
            base.OnMatchJoined(success, extendedInfo, matchInfo);

            // Também devemos testar aqui se conseguiu ingressar ou se deu algum erro.

            Debug.Log("Sucesso(" + success + ") ao ingressar na partida. " + matchInfo);
            
            // Podemos fazer preparações aqui!!!
        }

        // https://docs.unity3d.com/ScriptReference/Networking.NetworkManager.OnClientConnect.html
        // Called on the client when connected to a server.
        public override void OnClientConnect(NetworkConnection conn)
        {
            // Conectou
            base.OnClientConnect(conn);

            // Bom lugar para registrar mensagens customizadas...

            if (!NetworkServer.active)
            {//only to do on pure client (not self hosting client)
                MenuManager.ChangeWindowTo(MenuManager.MenuWindow.Lobby);
            }
        }

        // https://docs.unity3d.com/ScriptReference/Networking.NetworkLobbyManager.OnLobbyServerPlayersReady.html
        // This is called on the server when all the players in the lobby are ready.
        public override void OnLobbyServerPlayersReady()
        {
            // Checa se todos os players estão prontos para seguir.
            bool allready = true;
            for (int i = 0; i < lobbySlots.Length; ++i)
            {
                if (lobbySlots[i] != null)
                    allready &= lobbySlots[i].readyToBegin;
            }

            // Se estiverem, carrega a Scene... // Bom lugar para chamar uma courotine para fazer a contagem regressiva.
            if (allready)
            {
                for (int i = 0; i < lobbySlots.Length; i++)
                {
                    clanIntentity.Add(lobbySlots[i].connectionToClient.connectionId, lobbySlots[i].gameObject.GetComponent<LobbyPlayer>().playerClan);
                }                
                ServerChangeScene(playScene);
            }
        }

        // https://docs.unity3d.com/ScriptReference/Networking.NetworkManager.OnClientDisconnect.html
        // Called on clients when disconnected from a server.
        public override void OnClientDisconnect(NetworkConnection conn)
        {
            base.OnClientDisconnect(conn);

            MenuManager.ChangeWindowTo(MenuManager.MenuWindow.P2PMenu);
        }

        // https://docs.unity3d.com/ScriptReference/Networking.NetworkManager.OnClientError.html
        // Called on clients when a network error occurs.
        public override void OnClientError(NetworkConnection conn, int errorCode)
        {
            MenuManager.ChangeWindowTo(MenuManager.MenuWindow.P2PMenu);
            Debug.Log("Cient error : " + (errorCode == 6 ? "timeout" : errorCode.ToString()));
        }

        // https://docs.unity3d.com/ScriptReference/Networking.NetworkLobbyManager.OnLobbyClientSceneChanged.html
        // This is called on the client when the client is finished loading a new networked scene.
        public override void OnLobbyClientSceneChanged(NetworkConnection conn)
        {
            this.transform.GetChild(1).gameObject.SetActive(false);
        }

        public override GameObject OnLobbyServerCreateGamePlayer(NetworkConnection conn, short playerControllerId)
        {
            int prefabIndex = (int)clanIntentity[conn.connectionId];
            Transform position = GetStartPosition();
            GameObject player = (GameObject)Instantiate(playerPrefabs[prefabIndex], position.position, position.rotation);
            return player;

            //return base.OnLobbyServerCreateGamePlayer(conn, playerControllerId);
        }

        public override bool OnLobbyServerSceneLoadedForPlayer(GameObject lobbyPlayer, GameObject gamePlayer)
        {
            //Debug.Log("Jogador do Clã " + lobbyPlayer.GetComponent<LobbyPlayer>().playerClan.ToString());
            //short playerControllerId = lobbyPlayer.GetComponent<NetworkIdentity>().playerControllerId;
            //NetworkConnection temp = gamePlayer.GetComponent<NetworkIdentity>().connectionToServer;
            //NetworkConnection temp2 = gamePlayer.GetComponent<NetworkIdentity>().connectionToClient;
            //Debug.Log("O LobbyPlayer contem o ID: " + playerControllerId + " e ConnectionID é " + lobbyPlayer.GetComponent<NetworkIdentity>().connectionToClient.connectionId);
            //playerControllerId = gamePlayer.GetComponent<NetworkIdentity>().playerControllerId;
            //Debug.Log("O GamePLayer contem o ID: " + playerControllerId + " e ConnectionID é ");//+ gamePlayer.GetComponent<NetworkIdentity>().connectionToClient.connectionId);

            return base.OnLobbyServerSceneLoadedForPlayer(lobbyPlayer, gamePlayer);
        }

        public override void OnServerDisconnect(NetworkConnection conn)
        {
            NetworkServer.DestroyPlayersForConnection(conn);
            if (conn.lastError != NetworkError.Ok)
            {
                if (LogFilter.logError)
                {
                    Debug.LogError("ServerDisconnected due to error: " + conn.lastError);
                }
            }
            //base.OnServerDisconnect(conn);
        }




        // Update is called once per frame
        void Update()
        {

        }
    }

}
