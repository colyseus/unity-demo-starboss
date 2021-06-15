using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class GameUIController : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField]
    private Button exitButton = null;

    [SerializeField]
    private Button readyButton = null;

    [SerializeField]
    private TextMeshProUGUI pingLabel;
#pragma warning restore 0649
    public UnityEvent onPlayerReady;
    public UnityEvent onExit;

    public void UpdatePlayerReadiness(bool showButton)
    {
        readyButton.gameObject.SetActive(showButton);
    }

    public void AllowExit(bool allowed)
    {
        exitButton.gameObject.SetActive(allowed);
    }

    public void ButtonOnReady()
    {
        onPlayerReady?.Invoke();
    }

    public void ButtonOnExit()
    {
        onExit?.Invoke();
    }

    public virtual void Update()
    {
        pingLabel.text = $"Ping: {ExampleManager.Instance.GetRoundtripTime}ms";
    }
}