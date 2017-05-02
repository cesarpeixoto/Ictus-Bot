using UnityEngine;
using UnityEngine.UI;

public class ClanToggleUpdater : MonoBehaviour
{
    // This function is called when the object becomes enabled and active
    private void Start()
    {        
        GetComponent<Toggle>().interactable = (!CustonNetwork.LobbyPlayer.clanInUse.Contains(transform.GetSiblingIndex() - 1));
    }
}
