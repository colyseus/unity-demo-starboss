using System.Collections;
using System.Collections.Generic;
using LucidSightTools;
using TMPro;
using UnityEngine;

public class StarBossPlayerInfoView : MonoBehaviour
{
    [SerializeField]
    private StarBossTeamInfoView teamAView;
    [SerializeField]
    private StarBossTeamInfoView teamBView;

    private bool _isVersus;

    public void SetData(bool isVersus)
    {
        _isVersus = isVersus;

        UpdateView();
    }

    public void AddPlayer(ColyseusNetworkedEntity playerEntity)
    {
        // Determine what team the player is on
        if (_isVersus == false)
        {
            teamAView.AddPlayer(playerEntity);
        }
        else
        {
            //If we ever add more teams this will need to be adjusted
            int teamIdx = StarBossGameManager.Instance.GetTeamIndex(playerEntity.ownerId);
            if (teamIdx == 0)
            {
                teamAView.AddPlayer(playerEntity);
            }
            else if (teamIdx == 1)
            {
                teamBView.AddPlayer(playerEntity);
            }
        }
    }

    public void UpdatePlayerScores()
    {
        teamAView.UpdatePlayerScores();

        if (teamBView.gameObject.activeInHierarchy)
        {
            teamBView.UpdatePlayerScores();
        }
    }

    public void RemovePlayer(ColyseusNetworkedEntity playerEntity)
    {
        // Determine what team the player is on
        if (_isVersus == false)
        {
            teamAView.RemovePlayer(playerEntity);
        }
        else
        {
            if (teamAView.ContainsPlayer(playerEntity))
            {
                teamAView.RemovePlayer(playerEntity);
            }
            else if (teamBView.ContainsPlayer(playerEntity))
            {
                teamBView.RemovePlayer(playerEntity);
            }
        }
    }

    private void UpdateView()
    {
        // The team A view will always be active as it will be used for the co-op game mode
        teamAView.SetData(_isVersus, _isVersus ? "Team A" : "Players");

        // Toggle the team B view
        teamBView.gameObject.SetActive(_isVersus);

        teamBView.SetData(_isVersus, "Team B");
    }
}
