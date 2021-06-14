using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Colyseus;
using Colyseus.Schema;
using GameDevWare.Serialization;
using LucidSightTools;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StarBossGameManager : ExampleGameManager
{
    public static StarBossGameManager Instance { get; private set; }

    public StarBossGameUIController uiController;

    public BossController bossController;

    private string currentGameState = "";
    private string lastGameState = "";

    private bool _showCountDown = false;

    public bool RoundInProgress { get; private set; } = false;

    public bool IsCoop { get; private set; }
    public bool JoinComplete { get; private set; } = false;

    public delegate void OnUpdateClientTeam(int teamIndex, string clientID);
    public static event OnUpdateClientTeam onUpdateClientTeam;

    public Transform tdmSpawnCenter;
    public float tdmMinSpawnVariance = 200f;
    public float tdmMaxSpawnVariance = 500f;
    public int winningTeam = -1;

    [SerializeField]
    private List<StarBossTeam> teams = new List<StarBossTeam>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    protected override void OnEnable()
    {
        base.OnEnable();


        ExampleRoomController.onRoomStateChanged += OnRoomStateChanged;
        ExampleRoomController.onBeginRoundCountDown += OnBeginRoundCountDown;
        ExampleRoomController.onBeginRound += OnBeginRound;
        ExampleRoomController.onRoundEnd += OnRoundEnd;
        ExampleRoomController.onJoined += OnJoinedRoom;
        ExampleRoomController.onTeamUpdate += OnTeamUpdate;
        ExampleRoomController.onTeamReceive += OnFullTeamUpdate;

        onViewAdded += OnShipCreated;

        uiController.UpdateCountDownMessage("");
        uiController.UpdateGeneralMessageText("");
        uiController.ToggleBossHealth(false);
        bossController.ToggleBoss(false);


    }

    protected override void OnDisable()
    {
        base.OnDisable();

        ExampleRoomController.onRoomStateChanged -= OnRoomStateChanged;
        ExampleRoomController.onBeginRoundCountDown -= OnBeginRoundCountDown;
        ExampleRoomController.onBeginRound -= OnBeginRound;
        ExampleRoomController.onRoundEnd -= OnRoundEnd;
        ExampleRoomController.onJoined -= OnJoinedRoom;
        ExampleRoomController.onTeamUpdate -= OnTeamUpdate;
        ExampleRoomController.onTeamReceive -= OnFullTeamUpdate;

        onViewAdded -= OnShipCreated;
    }

    /// <summary>
    /// Returns a random spawn point around the <see cref="tdmSpawnCenter"/> using <see cref="tdmMinSpawnVariance"/> and <see cref="tdmMaxSpawnVariance"/>
    /// </summary>
    /// <param name="teamIndex"></param>
    /// <returns></returns>
    public Vector3 GetTDMSpawnPoint(int teamIndex)
    {
        Vector3 pos = tdmSpawnCenter.position;

        if (teamIndex == 0)
        {
            pos += (-tdmSpawnCenter.right * Random.Range(tdmMinSpawnVariance, tdmMaxSpawnVariance));
        }
        else if (teamIndex == 1)
        {
            pos += (tdmSpawnCenter.right * Random.Range(tdmMinSpawnVariance, tdmMaxSpawnVariance));
        }

        pos.z += Random.Range(-tdmMaxSpawnVariance, tdmMaxSpawnVariance);

        return pos;
    }

    public void SetCurrentUserAttributes(Dictionary<string, string> attributes)
    {
        ExampleManager.NetSend("setAttribute",
            new ExampleAttributeUpdateMessage
            {
                userId = ExampleManager.Instance.CurrentUser.id,
                attributesToSet = attributes
            });
    }

    private void OnJoinedRoom(string customLogic)
    {
        IsCoop = string.Equals(customLogic, "starBossCoop");
        JoinComplete = true;
    }

    private void OnBeginRoundCountDown()
    {
        LSLog.LogImportant($"Round Count Down Has Begun!", LSLog.LogColor.cyan);

        _showCountDown = true;
    }

    private void OnBeginRound(int bossHealth)
    {
        StartCoroutine(BeginRoutine(bossHealth));
    }

    private IEnumerator BeginRoutine(int bossHealth)
    {
        if (IsCoop == false)
        {
            PlayerSpaceshipController pc = GetOwnersShip();

            if (pc)
            {
                pc.PositionAtSpawn();
            }
        }

        RoundInProgress = true;
        if (IsCoop)
        {
            uiController.ToggleBossHealth(true);

            bossController.ToggleBoss(true, bossHealth);
        }
        else
        {
            winningTeam = -1;
        }

        yield return new WaitForSeconds(1.0f);

        _showCountDown = false;
        uiController.UpdateCountDownMessage("");
    }

    private void OnRoundEnd()
    {
        LSLog.LogImportant($"Round Ended!", LSLog.LogColor.lime);
        StartCoroutine(RoundEndRoutine());
    }

    private IEnumerator RoundEndRoutine()
    {
        RoundInProgress = false;
        PlayerSpaceshipController ownersShip = GetOwnersShip();
        if (IsCoop)
        {
            bossController.RoundEnded();

            uiController.ShowRoundOverMessage("VICTORY! THE WORM IS DEFEATED!");

            // Wait for the boss controller to not be busy
            while (bossController.IsBusy)
            {
                yield return new WaitForEndOfFrame();
            }

            uiController.ToggleBossHealth(false);
        }
        else
        {
            //We may not have the winning team yet, need to hold here
            StartCoroutine(HoldForWinner());
        }

        ResetAllShipDamage();

        uiController.ToggleBossHealth(false);

        uiController.UpdatePlayerReadiness(true);
    }

    /// <summary>
    /// Reset damage for all the local ships.
    /// </summary>
    private void ResetAllShipDamage()
    {
        IndexedDictionary<string, ExampleNetworkedEntityView> entityViews = ExampleManager.Instance.GetEntityViews();
        foreach (KeyValuePair<string, ExampleNetworkedEntityView> view in entityViews)
        {
            PlayerSpaceshipController pc = view.Value as PlayerSpaceshipController;

            if (pc)
            {
                pc.ResetDamage();
            }
        }
    }

    IEnumerator HoldForWinner()
    {
        //We reset winning team to -1 at the start of every round
        while (winningTeam < 0)
        {
            yield return new WaitForSeconds(0.25f);
        }
        PlayerSpaceshipController ownersShip = GetOwnersShip();
        bool isWinner = ownersShip.TeamIndex == winningTeam;
        uiController.ShowRoundOverMessage(isWinner ? "VICTORY!" : "Defeat...");
    }

    private void UpdateCountDown(MapSchema<string> attributes)
    {
        if (!_showCountDown)
        {
            return;
        }

        if(attributes.TryGetValue("countDown", out string countDown))
        {
            uiController.UpdateCountDownMessage(countDown);
        }
    }

    private void UpdateBossHealth(MapSchema<string> attributes)
    {
        if (attributes.TryGetValue("bossHealth", out string bossHealth))
        {
            if (int.TryParse(bossHealth, out int health))
            {
                bossController.UpdateBossHealth(health);
            }
        }
    }

    private void UpdateGeneralMessage(MapSchema<string> attributes)
    {
        if (attributes.TryGetValue("generalMessage", out string generalMessage))
        {
            uiController.UpdateGeneralMessageText(generalMessage);
        }
    }

    private void UpdateGameStates(MapSchema<string> attributes)
    {
        if (attributes.TryGetValue("currentGameState", out string currentServerGameState))
        {
            currentGameState = currentServerGameState;
        }

        if (attributes.TryGetValue("lastGameState", out string lastServerGameState))
        {
            lastGameState = lastServerGameState;
        }

        if (attributes.TryGetValue("winningTeamId", out string currentWinningTeam))
        {
            if(!int.TryParse(currentWinningTeam, out winningTeam))
            {
                LSLog.LogError($"Failed to parse currentWinningTeam: {currentWinningTeam}");
            }
        }
    }

    private void OnRoomStateChanged(MapSchema<string> attributes)
    {
        UpdateGameStates(attributes);
        UpdateGeneralMessage(attributes);
        UpdateCountDown(attributes);
        UpdateBossHealth(attributes);
    }

    private void OnTeamUpdate(int teamIdx, string clientID, bool added)
    {
        StarBossTeam team = GetOrCreateTeam(teamIdx);

        if (added)
        {
            if (team.AddPlayer(clientID))
            {
                //Alert anyone that needs to know, clientID has been added to teamIdx
                onUpdateClientTeam?.Invoke(teamIdx, clientID);
            }
        }
        else
        {
            team.RemovePlayer(clientID);
        }
    }

    private void OnFullTeamUpdate(int teamIdx, string[] clients)
    {
        StarBossTeam team = GetOrCreateTeam(teamIdx);
        for (int i = 0; i < clients.Length; ++i)
        {
            if (team.AddPlayer(clients[i]))
            {
                //Alert anyone that needs to know, clientID has been added to teamIdx
                onUpdateClientTeam?.Invoke(teamIdx, clients[i]);
            }
        }
    }

    private StarBossTeam GetOrCreateTeam(int teamIdx)
    {
        StarBossTeam team = null;
        for (int i = 0; i < teams.Count; ++i)
        {
            if (teams[i].teamIndex.Equals(teamIdx))
            {
                team = teams[i];
            }
        }

        if (team == null)
        {
            //We have not created this team yet
            team = new StarBossTeam();
            team.teamIndex = teamIdx;
            teams.Add(team);
        }

        return team;
    }

    public int GetTeamIndex(string clientID)
    {
        for (int i = 0; i < teams.Count; ++i)
        {
            if (teams[i].ContainsClient(clientID))
            {
                return teams[i].teamIndex;
            }
        }

        LSLog.LogError($"Client {clientID} is not on a team!"); //We should not be asking for teams if we're not expecting to have them
        return -1;
    }

    public bool AreUsersSameTeam(PlayerSpaceshipController clientA, PlayerSpaceshipController clientB)
    {
        if (IsCoop)
        {
            //Working together
            return true;
        }
        else
        {
            return clientA.TeamIndex.Equals(clientB.TeamIndex);
        }
    }

    public void RegisterKill(string attackerID, string deathID)
    {
        ExampleManager.CustomServerMethod("playerKilled", new object[] { attackerID, deathID});
    }

    protected override void OnNetworkRemove(ExampleNetworkedEntity entity, ColyseusNetworkedEntityView view)
    {
        base.OnNetworkRemove(entity, view);
        
        // Remove the disconnected player
        Destroy(view.gameObject);
    }

    /// <summary>
    /// Used with button input when the user is ready to start a round of play
    /// </summary>
    public void PlayerReadyToPlay()
    {
        uiController.UpdatePlayerReadiness(false);

        SetCurrentUserAttributes(new Dictionary<string, string> { { "readyState", "ready" } });

    }

    /// <summary>
    /// Used with button input when the user wants to return to the lobby
    /// </summary>
    public void OnQuitGame()
    {
        if (ExampleManager.Instance.IsInRoom)
        {
            //Find playerController for this player
            PlayerSpaceshipController pc = GetPlayerView< PlayerSpaceshipController>(ExampleManager.Instance.CurrentNetworkedEntity.id);
            if (pc != null)
            {
                pc.enabled = false; //Stop all the messages and updates
            }

            ExampleManager.Instance.LeaveAllRooms(() =>
            {
                ExampleManager.Instance.ClearCollectionsAndUser();
                SceneManager.LoadScene(0);
            });
        }
    }

    public PlayerSpaceshipController GetOwnersShip()
    {
        ExampleNetworkedEntityView view = ExampleManager.Instance.GetEntityView(ExampleManager.Instance.CurrentNetworkedEntity.id);
        if (view != null)
        {
            PlayerSpaceshipController pc = view as PlayerSpaceshipController;
            if (pc)
            {
                return pc;
            }
        }
        
        LSLog.LogError($"Could not find a player ship for owner with ID {ExampleManager.Instance.CurrentNetworkedEntity.id}");
        return null;
    }

    private void OnDestroy()
    {
#if UNITY_EDITOR
        ExampleManager.Instance.OnEditorQuit();
#endif
    }
    
    private void OnShipCreated(ColyseusNetworkedEntityView newView)
    {
        if (newView.TryGetComponent(out PlayerSpaceshipController controller))
        {
            if (!controller.IsMine)
            {
                controller.InitializeObjectForRemote();
            }
        }
    }
}
