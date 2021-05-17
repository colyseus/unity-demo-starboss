using System.Collections;
using System.Collections.Generic;
using GameDevWare.Serialization;
using LucidSightTools;
using UnityEngine;
using UnityStandardAssets.Cameras;
using TMPro;
using UnityEngine.Events;

public class PlayerWatcher : MonoBehaviour
{
    public GameObject watcher;
    public FreeLookCam freeLookCam;
    public TextMeshProUGUI playerNameText;

    public UnityEvent playerWatcherActivated;
    public UnityEvent playerWatcherDeactivated;

    public bool viewUs = true;

    public ExampleNetworkedEntityView target = null;

    private IndexedDictionary<string, ExampleNetworkedEntityView> entityViews;

    // Start is called before the first frame update
    void Start()
    {
        entityViews = ExampleManager.Instance.GetEntityViews();
        target = null;
        playerNameText.text = "";
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            TogglePlayerWatcher();
        }
    }

    private void TogglePlayerWatcher()
    {
        watcher.SetActive(!watcher.activeInHierarchy);

        if (watcher.activeInHierarchy)
        {
            if (viewUs)
            {
                // Set first player to be Us
                //LSLog.LogImportant($"Current Entity ID = {ExampleManager.Instance.CurrentNetworkedEntity.id}");
                target = ExampleManager.Instance.GetEntityView(ExampleManager.Instance.CurrentNetworkedEntity.id);

                //LSLog.LogImportant($"Entity View Count = {entityViews.Count} Found Current User Target = {target != null}");

                //for (int i = 0; i < entityViews.Keys.Count; i++)
                //{
                //    LSLog.LogImportant($"Entity Key[{i}] = {entityViews.Keys[i]}");
                //}
            }

            if (target)
            {
                SetTarget(target.transform, GetTargetName());

                //LSLog.LogImportant($"Set Target - Id = {target.Id}");

            }
            else
            {
                LSLog.LogImportant($"No target to observe");
            }
        }

        if(watcher.activeInHierarchy)
            playerWatcherActivated?.Invoke();
        else 
            playerWatcherDeactivated?.Invoke();
    }

    public void ViewNextPlayer(int dir)
    {
        if (target)
        {
            // Get index of current target
            int currentIdx = entityViews.IndexOf(target.Id);

            currentIdx += dir;

            // Clamp index
            if (currentIdx < 0)
            {
                currentIdx = entityViews.Count - 1;
            }
            else if (currentIdx > entityViews.Count - 1)
            {
                currentIdx = 0;
            }

            string entityKey = entityViews.Keys[currentIdx];

            if (entityViews.ContainsKey(entityKey))
            {
                target = entityViews[entityKey];
            }
            else
            {
                LSLog.LogError($"No entity for entity Key - {entityKey} to Index - {currentIdx}");
            }
        }
        else if(entityViews.Keys.Count > 0)
        {

            target = entityViews[entityViews.Keys[0]];
        }

        if (target)
        {
            SetTarget(target.transform, GetTargetName());
        }
        else
        {
            LSLog.LogImportant($"No Target to switch to...");
        }
    }

    private string GetTargetName()
    {
        return string.IsNullOrEmpty(target.UserName) ? target.Id : target.UserName;
    }

    public void SetTarget(Transform target, string playerName = "")
    {
        freeLookCam.SetTarget(target);

        if (string.IsNullOrEmpty(playerName) == false)
            playerNameText.text = playerName;
    }
}