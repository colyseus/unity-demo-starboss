using System.Collections;
using System.Collections.Generic;
using LucidSightTools;
using TMPro;
using UnityEngine;

public class StarBossPlayerListItem : MonoBehaviour
{
    public int Score { get; private set; }

#pragma warning disable 0649
    [SerializeField]
    private TextMeshProUGUI playerNameText;

    [SerializeField]
    private TextMeshProUGUI playerKills;
    [SerializeField]
    private TextMeshProUGUI playerDeaths;
    [SerializeField]
    private TextMeshProUGUI playerScore;

    [SerializeField]
    private GameObject isMineBG;
#pragma warning restore 0649

    private string _playerName;
    private bool _isMine;

    public void SetData(string playerName, bool isMine, bool isVersus)
    {
        _playerName = playerName;
        _isMine = isMine;

        UpdateView(isVersus);
    }

    public void SetScores(string score, string kills, string deaths)
    {
        if (int.TryParse(score, out int scoreInt))
        {
            Score = scoreInt;
        }
        else
        {
            LSLog.LogError($"Player List Item - Error parsing score from {score}");
        }

        playerScore.text = score;
        playerKills.text = kills;
        playerDeaths.text = deaths;
    }

    public void UpdateView(bool isVersus)
    {
        isMineBG.SetActive(_isMine);
        playerNameText.text = _playerName;

        playerKills.gameObject.SetActive(isVersus);
        playerDeaths.gameObject.SetActive(isVersus);
    }
}
