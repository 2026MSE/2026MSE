using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugLobbyJoinButton : MonoBehaviour
{

    private void OnEnable()
    {
        GetComponent<UnityEngine.UI.Button>().onClick.AddListener(joinDebugPlayers());
    }

    private UnityEngine.Events.UnityAction joinDebugPlayers()
    {
        return () =>
        {
            foreach (var debugPlayer in PlayerManager.instance.debug_players)
            {
                if(PlayerManager.instance.currentRoom.playerIds.Contains(debugPlayer.id))
                {
                    continue;
                }
                ServerManager.instance.RoomRequest(new GameActionRequest { playerId = debugPlayer.id, roomId = PlayerManager.instance.currentRoom.roomId } , ServerManager.RoomActionType.Join).Forget();
            }
        };
    }
}
