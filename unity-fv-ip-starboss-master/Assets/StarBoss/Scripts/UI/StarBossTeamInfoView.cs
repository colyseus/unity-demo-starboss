using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using LucidSightTools;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class StarBossTeamInfoView : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI teamNameText;

    [SerializeField]
    private GameObject headerKills;

    [SerializeField]
    private GameObject headerDeaths;

    [SerializeField]
    private StarBossPlayerListItem playerListItemPrefab;

    [SerializeField]
    private RectTransform playerListContent;

    private Dictionary<string, StarBossPlayerListItem> playerListItems;
    private List<ColyseusNetworkedEntity> playerEntities;
    private List<StarBossPlayerListItem> listItemHelper = new List<StarBossPlayerListItem>();

    private string _teamName;
    private bool _isVersus;

    public void SetData(bool isVersus, string teamName)
    {
        _isVersus = isVersus;
        _teamName = teamName;

        UpdateView();
    }

    private void UpdateView()
    {
        if (playerListItems == null)
        {
            playerListItems = new Dictionary<string, StarBossPlayerListItem>();
        }

        teamNameText.text = _teamName;

        headerKills.SetActive(_isVersus);
        headerDeaths.SetActive(_isVersus);

        foreach (KeyValuePair<string, StarBossPlayerListItem> entry in playerListItems)
        {
            entry.Value.UpdateView(_isVersus);
        }
    }

    public void UpdatePlayerScores()
    {
        if (playerEntities == null)
        {
            playerEntities = new List<ColyseusNetworkedEntity>();
        }

        if (playerListItems == null)
        {
            playerListItems = new Dictionary<string, StarBossPlayerListItem>();
        }

        listItemHelper.Clear();

        string score = "---";
        string kills = "---";
        string deaths = "---";
        // Update score values
        for (int i = 0; i < playerEntities.Count; i++)
        {
            // Get the player's score
            if (!playerEntities[i].attributes.TryGetValue("score", out score))
            {
                score = "0";
            }

            if (_isVersus)
            {
                if (!playerEntities[i].attributes.TryGetValue("kills", out kills))
                {
                    kills = "0";
                }

                if (!playerEntities[i].attributes.TryGetValue("deaths", out deaths))
                {
                    deaths = "0";
                }
            }

            // Update the list item associated with that player entity
            if (playerListItems.TryGetValue(playerEntities[i].id, out StarBossPlayerListItem listItem))
            {
                listItem.SetScores(score, kills, deaths);
                listItemHelper.Add(listItem);
            }
        }
        
        // Sort list of entities by score value so higher scores will appear on the top of the list
        listItemHelper.Sort((a, b) =>
        {
            return b.Score.CompareTo(a.Score);
        });

        // Order player list items in the container by their index
        for (int i = 0; i < listItemHelper.Count; i++)
        {
            listItemHelper[i].transform.SetSiblingIndex(i);
        }
    }

    public void AddPlayer(ColyseusNetworkedEntity playerEntity)
    {
        if (playerListItems == null)
        {
            playerListItems = new Dictionary<string, StarBossPlayerListItem>();
        }

        if (playerEntities == null)
        {
            playerEntities = new List<ColyseusNetworkedEntity>();
        }

        if (playerListItems.ContainsKey(playerEntity.id) == false)
        {
            StarBossPlayerListItem listItem = Instantiate(playerListItemPrefab);

            if (playerEntity.attributes.TryGetValue("userName", out string userName))
            {

            }
            else
            {
                userName = playerEntity.id;
            }

            listItem.SetData(userName,
                string.Equals(ExampleManager.Instance.CurrentNetworkedEntity.id, playerEntity.id), _isVersus);

            listItem.transform.SetParent(playerListContent);

            playerListItems.Add(playerEntity.id, listItem);
            playerEntities.Add(playerEntity);
        }
        else
        {
            LSLog.LogError($"A player with Id {playerEntity.id} already exists on the list!");
        }
    }

    public void RemovePlayer(ColyseusNetworkedEntity playerEntity)
    {
        if (playerListItems == null)
        {
            return;
        }

        if (playerListItems.ContainsKey(playerEntity.id))
        {
            StarBossPlayerListItem listItem = playerListItems[playerEntity.id];

            playerListItems.Remove(playerEntity.id);

            Destroy(listItem.gameObject);

            playerEntities?.Remove(playerEntity);
        }
        else
        {
            LSLog.LogError($"No player with Id {playerEntity.id} exists on the list!");
        }
    }

    public bool ContainsPlayer(ColyseusNetworkedEntity playerEntity)
    {
        if (playerListItems == null)
        {
            return false;
        }

        return playerListItems.ContainsKey(playerEntity.id);
    }

}
