using System;
using System.Collections;
using System.Collections.Generic;
using LucidSightTools;
using UnityEngine;

public class BossController : MonoBehaviour
{
    public bool IsBusy //{ get; private set; } = false;
    {
        get
        {
            return wormAI.PathInProgress;
        }
    }

    [SerializeField]
    private GameObject bossRoot;

    [SerializeField]
    private WormAI wormAI;

    private Dictionary<string, string> attributeUpdate = new Dictionary<string, string>();

    private void OnEnable()
    {
        ExampleRoomController.onBossPathReady += OnBossPathReady;
    }

    private void OnDisable()
    {
        ExampleRoomController.onBossPathReady -= OnBossPathReady;
    }

    private void Start()
    {
        wormAI.SetWormCallbacks(BossTookDamage, BossPathCompleted);
    }

    public void ToggleBoss(bool bossActive, int bossHealth = 0)
    {
        bossRoot.SetActive(bossActive);

        if (bossActive)
        {
            wormAI.ResetWorm(bossHealth);

            SetBossAttributeReady();
        }
        else
        {
            wormAI.StopWormBehavior();
        }
    }

    public void UpdateBossHealth(int health)
    {
        wormAI.UpdateHealthUI(health);
    }

    private void OnBossPathReady(Vector3 startPosition, Vector3 peakPosition, Vector3 endPosition)
    {
        // Update the worm's path variants and target
        wormAI.UpdatePathStartEnd(startPosition, peakPosition, endPosition);

        // Begin the worm's path behavior
        wormAI.BeginWormBehavior();
    }

    public void RoundEnded()
    {
        StartCoroutine(Co_WaitForPathToFinish());
    }

    private IEnumerator Co_WaitForPathToFinish()
    {
        yield return new WaitUntil(() => wormAI.PathInProgress == false);

        ToggleBoss(false);
    }

    private void BossTookDamage(int damage)
    {
        ExampleManager.CustomServerMethod("bossTookDamage", new object[] { ExampleManager.Instance.CurrentNetworkedEntity.id, damage });
    }

    /// <summary>
    /// Callback when the boss completes its current path when targeting a player
    /// </summary>
    private void BossPathCompleted()
    {
        //LSLog.LogImportant($"Boss Controller - current path finished");

        // Let the server know this boss is ready for new path data
        SetBossAttributeReady();
    }

    private void SetBossAttributeReady()
    {
        attributeUpdate.Clear();
        attributeUpdate.Add("bossReadyState", "bossReady");

        StarBossGameManager.Instance.SetCurrentUserAttributes(attributeUpdate);
    }

    private PlayerSpaceshipController GetPlayerTarget(string playerId)
    {
        if (string.IsNullOrEmpty(playerId) == false)
        {
            //LSLog.LogImportant($"Boss Controller - Get Player Target - Target = {currentTarget}", LSLog.LogColor.cyan);

            return StarBossGameManager.Instance.GetPlayerView<PlayerSpaceshipController>(playerId);
        }

        return null;
    }
}
