using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using UnityEngine.Networking;
using UnityEngine.Networking.Match;

namespace CustonNetwork
{

    public class LobbyPlayer : NetworkLobbyPlayer
    {
        public enum RobotClan { AncientClan, ModernClan }

        // Lista para controle dos clans já selecionados para partida.
        public static List<int> clanInUse = new List<int>();

        /// Referência dos componentes da UI.
        private InputField _nameField = null;
        private Dropdown _clanField = null;
        private Button _joinButton = null;

        /// Sync Vars
        [SyncVar(hook = "SetThisName")]
        public string playerName = "";
        [SyncVar(hook = "SetThisClan")]
        public RobotClan playerClan = RobotClan.AncientClan;
                


        private void Awake()
        {
            _nameField = this.transform.Find("InputField").GetComponent<InputField>();
            _clanField = this.transform.Find("Dropdown").GetComponent<Dropdown>();
            _joinButton = this.transform.Find("JoinButton").GetComponent<Button>();

            // Resolve algum Bug de sicronia do componente, que apontava texto errado do indice escolhido.
            _clanField.RefreshShownValue(); 
        }



        /// Métodos sobrescritos de NetworkLobbyPlayer.


        // This is a hook that is invoked on all player objects when entering the lobby.
        // Note: isLocalPlayer is not guaranteed to be set until OnStartLocalPlayer is called. (Importante)
        public override void OnClientEnterLobby()
        {
            base.OnClientEnterLobby();
            MenuManager.RegisterNewClient(this);
            // Personalizando o UIPlayer de acordo com a situação dele. (na maquina de quem ele está)
            if (isLocalPlayer)
            {
                SetupUILocalPlayer();
            }
            else
            {
                SetupUIOpponentPlayer();
            }
            // Atualiza os dados na UI.
            SetThisName(playerName);
            SetThisClan(playerClan);
        }

        // This is invoked on behaviours that have authority, based on context and NetworkIdentity.localPlayerAuthority.
        public override void OnStartAuthority()
        {
            base.OnStartAuthority();
            // Quando este callback ocorre, temos a garantia de que este componente é do Localplayer.
            SetupUILocalPlayer();
        }

        // This function is called when the a client player calls SendReadyToBeginMessage() or SendNotReadyToBeginMessage().
        public override void OnClientReady(bool readyState)
        {
            base.OnClientReady(readyState);

            // Então, aqui é um meio de atualizar... recebemos aqui a mensagem de que algém está pronto...
            // Podemos testar se é o LocalPLayer para fazer coisas diferetnes...

            if(readyState)
            {
                // TODO: Configurar animação ou coisas para o estado já clicou para entrar na partida, e esta aguardando o oponente.
                _nameField.interactable = false;
                _clanField.interactable = false;
                _joinButton.interactable = false;
            }
        }


        /// Configurações padrão da UI.        

        // Configura a UI do componente com propriedades no padrão do painel de localplayer.
        private void SetupUILocalPlayer()
        {
            _nameField.interactable = true;            
            _nameField.onEndEdit.RemoveAllListeners();
            _nameField.onEndEdit.AddListener(OnNameChanged);

            _clanField.interactable = true;
            _clanField.onValueChanged.RemoveAllListeners();
            _clanField.onValueChanged.AddListener(OnClanChanged);

            _joinButton.interactable = true;
            _joinButton.transform.GetChild(0).GetComponent<Text>().text = "Ingressar";
            _joinButton.onClick.RemoveAllListeners();
            _joinButton.onClick.AddListener(OnReady);
            
            CmdNameChanged("Player " + (MenuManager.GetClientsContentCount() - 1).ToString());
            CmdOnClanChanged((int)playerClan);
            
            // TODO: Quando entra, já soma o valor, então seria só para atualziar... mas precisa ver lá o que esta função faz (OnPlayersNumberModified).
            //if (LobbyManager.s_Singleton != null) LobbyManager.s_Singleton.OnPlayersNumberModified(0);
        }

        // Configura a UI do componente com propriedades no padrão do painel de oponente.
        private void SetupUIOpponentPlayer()
        {
            _nameField.interactable = false;
            _clanField.interactable = false;
            _joinButton.interactable = false;
            _joinButton.transform.GetChild(0).GetComponent<Text>().text = "Configurando";
        }


        /// Eventos para os componentes da UI.

        // Evento que indica que o nome foi alterado no nameField.
        public void OnNameChanged(string name)
        {
            // Evento da unity não recebe Commands para o servidor, usamos esta função de intermediária.
            CmdNameChanged(name);
        }

        // Evento que indica que o clan foi alterado no clanField.
        public void OnClanChanged(int clan)
        {
            // Mesmo que OnNameChanged.
            CmdOnClanChanged(clan);
        }

        // Evento que indica que o jogador está pronto para iniciar a partida.
        public void OnReady()
        {
            // This is used on clients to tell the server that this player is ready for the game to begin
            SendReadyToBeginMessage();
        }

        /// Callback para atualização da interface durante atuação das Sync Vard

        // Altera na UI o conteudo do nome do jogador, de acordo com o dado sincronizado pelo servidor.
        public void SetThisName(string newName)
        {
            playerName = newName;
            _nameField.text = playerName;
        }

        // Altera na UI o conteudo do Clan do jogador, de acordo com o dado sincronizado pelo servidor.
        public void SetThisClan(RobotClan newClan)
        {
            playerClan = newClan;
            _clanField.value = (int)playerClan;
            _clanField.value = (int)playerClan;
        }


        /// Commands (comandos executados no servidor).

        [Command] // Comando para alterar o nome no Servidor.
        public void CmdNameChanged(string name)
        {
            // O nome é alterado no Servidor, e sincronizado para todos os clients pela sync var responsável.
            playerName = name;
        }
        

        [Command] // Comando para alterar o clan no Servidor.
        public void CmdOnClanChanged(int clan)
        {
            if (clanInUse.Count == 0) // Ninguém escolheu nada, escolho primeiro.
            {
                clanInUse.Add(clan);
                playerClan = (RobotClan)clan;
            }
            else if (clanInUse.Count == 1 && clanInUse[0] == (int)playerClan && (int)playerClan != clan) // Tem um usado, mas este um sou eu, quero trocar se for outro.
            {
                playerClan = (RobotClan)clan;
                clanInUse[0] = clan;
            }
            else if (clanInUse.Count == 1 && clanInUse[0] != clan) // Tem um usado, e minha escolha é diferente deste usado, posso selecionar.
            {
                playerClan = (RobotClan)clan;
                clanInUse.Add(clan);
            }
            else if (clanInUse.Count == 1 && clanInUse[0] == clan) // Selecionei um usado, minha escolha é igual ao usado, não posso, seleciona automaticamente o livre.
            {
                int freeClan = clanInUse[0] == 0 ? 1 : 2;
                playerClan = (RobotClan)freeClan;
                clanInUse.Add(freeClan);
            }
            int param1 = clanInUse.Count;
            int param2 = clanInUse.Count > 0 ? clanInUse[0] : 0;
            int param3 = clanInUse.Count > 1 ? clanInUse[1] : 0;

            RpcOnClanChanged(param1, param2, param3);
        }

        // Atualiza a lista clanInUse em todos os clientes, para ficar identico ao servidor.
        [ClientRpc]
        public void RpcOnClanChanged(int count, int pos1, int pos2)
        {
            clanInUse.Clear();
            if (count > 0)
                clanInUse.Add(pos1);

            if(count > 1)
                clanInUse.Add(pos2);
        }

       
    }
}