using System;
using System.Collections;
using System.Collections.Generic;
using Colyseus;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyController : MonoBehaviour
{
    [SerializeField]
    protected GameObject connectingCover = null;

    [SerializeField]
    private CreateUserMenu createUserMenu = null;

    //Variables to initialize the room controller
    public string roomName = "";

    [SerializeField]
    protected RoomSelectionMenu selectRoomMenu = null;

    protected Dictionary<string, object> roomOptions = null;

    private void Awake()
    {
        createUserMenu.gameObject.SetActive(true);
        selectRoomMenu.gameObject.SetActive(false);
        connectingCover.SetActive(true);
    }

    protected virtual IEnumerator Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        while (!ExampleManager.IsReady)
        {
            yield return new WaitForEndOfFrame();
        }

        Initialize();
    }

    protected virtual void Initialize()
    {
        ExampleManager.Instance.Initialize(roomName, roomOptions);
        ExampleManager.onRoomsReceived += OnRoomsReceived;
        connectingCover.SetActive(false);
    }

    private void OnDestroy()
    {
        ExampleManager.onRoomsReceived -= OnRoomsReceived;
    }

    public void CreateUser()
    {
        string desiredUserName = createUserMenu.UserName;
        PlayerPrefs.SetString("UserName", desiredUserName);

        ColyseusSettings clonedSettings = ExampleManager.Instance.CloneSettings();
        clonedSettings.colyseusServerAddress = createUserMenu.ServerURL;
        clonedSettings.colyseusServerPort = createUserMenu.ServerPort;
        clonedSettings.useSecureProtocol = createUserMenu.UseSecure;

        ExampleManager.Instance.OverrideSettings(clonedSettings);

        ExampleManager.Instance.InitializeClient();

        ExampleManager.Instance.UserName = desiredUserName;
        //Do user creation stuff
        createUserMenu.gameObject.SetActive(false);
        selectRoomMenu.gameObject.SetActive(true);
        selectRoomMenu.GetAvailableRooms();
    }

    public virtual void CreateRoom()
    {
        connectingCover.SetActive(true);
        string desiredRoomName = selectRoomMenu.RoomCreationName;
        LoadMainScene(() =>
        {
            ExampleManager.Instance.CreateNewRoom(desiredRoomName);
        });
    }

    public void JoinRoom(string id)
    {
        connectingCover.SetActive(true);
        LoadMainScene(()=>
        {
            ExampleManager.Instance.JoinExistingRoom(id);
        });
    }

    public void OnConnectedToServer()
    {
        connectingCover.SetActive(false);
    }

    private void OnRoomsReceived(StarBossRoomAvailable[] rooms)
    {
        selectRoomMenu.HandRooms(rooms);
    }

    private IEnumerator LoadSceneAsync(string scene, Action onComplete)
    {
        Scene currScene = SceneManager.GetActiveScene();
        AsyncOperation op = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
        while (op.progress <= 0.9f)
        {
            //Wait until the scene is loaded
            yield return new WaitForEndOfFrame();
        }

        onComplete.Invoke();
        op.allowSceneActivation = true;
        SceneManager.UnloadSceneAsync(currScene);
    }

    protected void LoadMainScene(Action onComplete)
    {
        StartCoroutine(LoadSceneAsync("Scene_Dev_Environment", onComplete));
    }

    public virtual string GetModeTag(bool coop)
    {
        //Not implemented in base
        return "";
    }
}