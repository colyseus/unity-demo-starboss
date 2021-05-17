using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class GameUIController : MonoBehaviour
{
    [SerializeField]
    private Button exitButton = null;

    [SerializeField]
    private Button readyButton = null;

    [SerializeField]
    private TextMeshProUGUI pingLabel;

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