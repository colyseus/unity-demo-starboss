using System.Collections;
using System.Collections.Generic;
using LucidSightTools;
using UnityEngine;
using UnityEngine.UI;

public class StarBossLobbyController : LobbyController
{
    [SerializeField]
    private Toggle coopToggle = null;

    public override void CreateRoom()
    {
        connectingCover.SetActive(true);

        string gameModeLogic = coopToggle.isOn ? "starBossCoop" : "starBossTDM";
        roomOptions = new Dictionary<string, object> {{"logic", gameModeLogic }, { "scoreToWin", 3 } };

        LoadMainScene(() => { ExampleManager.Instance.CreateNewRoom(selectRoomMenu.RoomCreationName, roomOptions); });
    }
}
