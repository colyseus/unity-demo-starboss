using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Colyseus;
using LucidSightTools;
using TMPro;
using UnityEngine;

public class StarBossGameUIController : GameUIController
{
#pragma warning disable 0649
    [SerializeField]
    private TextMeshProUGUI generalMessageText;
    [SerializeField]
    private TextMeshProUGUI countDownText;

    [SerializeField]
    private TextMeshProUGUI roundOverMessageText;

    [SerializeField]
    private GameObject bossHealth;

    [SerializeField]
    private StarBossPlayerTag playerTagPrefab;

    [SerializeField]
    private RectTransform playerTagRoot;

    [SerializeField]
    private TextMeshProUGUI playerJoinMsgPrefab;

    [SerializeField]
    private RectTransform playerJoinMsgRoot;

    [SerializeField]
    private StarBossPlayerInfoView playerInfo;

    [SerializeField]
    private CanvasGroup loadingCover;
#pragma warning restore 0649
    private PlayerSpaceshipController myShip;

    private Queue<GameObject> playerJoinMessages;
    private float currentMsgUpdate = 0;

    public Camera cam;

    private Dictionary<PlayerSpaceshipController, StarBossPlayerTag> playerTags;

    public bool IsReady { get; private set; } = false;

    private void OnEnable()
    {
        PlayerSpaceshipController.onPlayerShipActivated += OnPlayerShipActivated;
        PlayerSpaceshipController.onPlayerShipDeactivated += OnPlayerShipDeactivated;

        StarBossGameManager.onViewRemoved += OnViewRemoved;
        ExampleRoomController.onPlayerJoined += OnPlayerJoined;

        // For player list
        ExampleRoomController.onAddNetworkEntity += OnAddNetworkEntity;
        ExampleRoomController.onRemoveNetworkEntity += OnRemoveNetworkEntity;
    }

    private void OnDisable()
    {
        PlayerSpaceshipController.onPlayerShipActivated -= OnPlayerShipActivated;
        PlayerSpaceshipController.onPlayerShipDeactivated -= OnPlayerShipDeactivated;

        StarBossGameManager.onViewRemoved -= OnViewRemoved;
        ExampleRoomController.onPlayerJoined -= OnPlayerJoined;

        ExampleRoomController.onAddNetworkEntity -= OnAddNetworkEntity;
        ExampleRoomController.onRemoveNetworkEntity -= OnRemoveNetworkEntity;
    }

    private IEnumerator Start()
    {
        yield return StartCoroutine(Init());
    }

    private IEnumerator Init()
    {
        if (!IsReady)
        {

            IsReady = true;

            roundOverMessageText.gameObject.SetActive(false);

            playerTags = new Dictionary<PlayerSpaceshipController, StarBossPlayerTag>();
            playerJoinMessages = new Queue<GameObject>();

            playerInfo.gameObject.SetActive(false);

            while (StarBossGameManager.Instance.JoinComplete == false)
            {
                yield return new WaitForEndOfFrame();
            }

            playerInfo.SetData(StarBossGameManager.Instance.IsCoop == false);

            float t = 0.0f;
            while (t < 1.0f)
            {
                loadingCover.alpha = Mathf.Lerp(1, 0, t);
                yield return new WaitForEndOfFrame();
                t += Time.deltaTime;
            }
            loadingCover.gameObject.SetActive(false);
        }
    }

    private void OnPlayerShipActivated(PlayerSpaceshipController shipController)
    {
        if (!IsReady)
        {
            StartCoroutine(Init());
        }

        if (shipController.IsMine)
        {
            myShip = shipController;
            return;
        }

        if (playerTags.ContainsKey(shipController) == false)
        {
            StarBossPlayerTag newPlayerTag = Instantiate(playerTagPrefab);

            newPlayerTag.transform.SetParent(playerTagRoot);
            newPlayerTag.SetPlayerTag(string.IsNullOrEmpty(shipController.UserName) ? shipController.Id : shipController.UserName);

            playerTags.Add(shipController, newPlayerTag);
        }
    }

    private void OnPlayerShipDeactivated(PlayerSpaceshipController shipController)
    {
        if (shipController.IsMine) return;

        if (playerTags.ContainsKey(shipController))
        {
            StarBossPlayerTag playerTag = playerTags[shipController];

            Destroy(playerTag.gameObject);

            playerTags.Remove(shipController);
        }
    }

    private void OnPlayerJoined(string playerUserName)
    {
        if (!IsReady)
        {
            StartCoroutine(Init());
        }

        if (string.IsNullOrEmpty(playerUserName) == false)
        {
            TextMeshProUGUI msg = Instantiate(playerJoinMsgPrefab);

            msg.transform.SetParent(playerJoinMsgRoot);

            msg.text = $"Player Joined: {playerUserName}";

            playerJoinMessages.Enqueue(msg.gameObject);
        }
    }

    private void OnViewRemoved(ColyseusNetworkedEntityView view)
    {
        PlayerSpaceshipController controller = view as PlayerSpaceshipController;

        if (controller)
        {
            TextMeshProUGUI msg = Instantiate(playerJoinMsgPrefab);

            msg.transform.SetParent(playerJoinMsgRoot);

            msg.text = $"Player Left: {controller.UserName}";

            playerJoinMessages.Enqueue(msg.gameObject);
        }
        else
        {
            LSLog.LogError("Failed to convert view into PlayerSpaceshipController!");
        }
    }

    private void OnAddNetworkEntity(ColyseusNetworkedEntity entity)
    {
        StartCoroutine(WaitAddEntity(entity));
    }

    private IEnumerator WaitAddEntity(ColyseusNetworkedEntity entity)
    {
        while (!StarBossGameManager.Instance.JoinComplete)
        {
            yield return new WaitForEndOfFrame();
        }
        playerInfo.AddPlayer(entity);
    }

    private void OnRemoveNetworkEntity(ColyseusNetworkedEntity entity, ColyseusNetworkedEntityView view)
    {
        playerInfo.RemovePlayer(entity);
    }

    public void UpdateGeneralMessageText(string message)
    {
        generalMessageText.text = message;
    }

    public void UpdateCountDownMessage(string message)
    {
        countDownText.text = message;
    }

    public void ToggleBossHealth(bool show)
    {
        bossHealth.SetActive(show);
    }

    public void ShowRoundOverMessage(string message)
    {
        StartCoroutine(DisplayRoundOVerMessageRoutine(message));
    }

    private IEnumerator DisplayRoundOVerMessageRoutine(string message)
    {
        roundOverMessageText.text = message;
        roundOverMessageText.gameObject.SetActive(true);

        yield return new WaitForSeconds(5);

        roundOverMessageText.gameObject.SetActive(false);
    }

    public override void Update()
    {
        base.Update();

        UpdatePlayerJoinMessage();

        playerInfo.gameObject.SetActive(StarBossGameManager.Instance.JoinComplete && Input.GetKey(KeyCode.Tab));

        if(playerInfo.gameObject.activeInHierarchy)
            playerInfo.UpdatePlayerScores();
    }

    public void LateUpdate()
    {
        UpdatePlayerTags();
    }

    private void UpdatePlayerTags()
    {
        foreach (KeyValuePair<PlayerSpaceshipController, StarBossPlayerTag> pair in playerTags)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(playerTagRoot, RectTransformUtility.WorldToScreenPoint(cam, pair.Key.transform.position), null, out Vector2 pos))
            {
                bool isFriendly = true;
                if (myShip != null)
                {
                    isFriendly = StarBossGameManager.Instance.AreUsersSameTeam(myShip, pair.Key);
                }
                pair.Value.UpdateTag(pos, pair.Key.IsAlive && (cam.ScreenToViewportPoint(cam.WorldToViewportPoint(pair.Key.transform.position)).z > 0) ? 1 : 0, isFriendly);
            }
        }
    }

    private void UpdatePlayerJoinMessage()
    {
        if (playerJoinMessages.Count > 0)
        {
            currentMsgUpdate += Time.unscaledDeltaTime;

            // remove a join message every 5 seconds
            if (currentMsgUpdate >= 5)
            {
                GameObject msg = playerJoinMessages.Dequeue();

                Destroy(msg);

                currentMsgUpdate = 0;
            }

        }
        else
        {
            currentMsgUpdate = 0;
        }
    }
}
