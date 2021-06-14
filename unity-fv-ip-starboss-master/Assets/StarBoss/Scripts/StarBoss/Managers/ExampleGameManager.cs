using Colyseus;
using LucidSightTools;
using UnityEngine;

public class ExampleGameManager : MonoBehaviour
{
    public delegate void OnViewAdded(ColyseusNetworkedEntityView view);
    public static event OnViewAdded onViewAdded;

    public delegate void OnViewRemoved(ColyseusNetworkedEntityView view);
    public static event OnViewRemoved onViewRemoved;

    public ColyseusNetworkedEntityView prefab;

    protected virtual void OnEnable()
    {
        ExampleRoomController.onAddNetworkEntity += OnNetworkAdd;
        ExampleRoomController.onRemoveNetworkEntity += OnNetworkRemove;
    }

    protected virtual void OnDisable()
    {
        ExampleRoomController.onAddNetworkEntity -= OnNetworkAdd;
        ExampleRoomController.onRemoveNetworkEntity -= OnNetworkRemove;
    }

    protected virtual void OnNetworkAdd(ExampleNetworkedEntity entity)
    {
        LSLog.LogImportant($"Game Manager - OnNetworkAdd");

        if (ExampleManager.Instance.HasEntityView(entity.id))
        {
            LSLog.LogImportant("View found! For " + entity.id);
        }
        else
        {
            LSLog.LogImportant("No View found for " + entity.id);
            CreateView(entity);
        }
    }

    protected virtual void OnNetworkRemove(ExampleNetworkedEntity entity, ColyseusNetworkedEntityView view)
    {
        RemoveView(view);
    }

    private void CreateView(ExampleNetworkedEntity entity)
    {
        //LSLog.LogImportant("print: " + JsonUtility.ToJson(entity));
        ColyseusNetworkedEntityView newView = Instantiate(prefab);
        ExampleManager.Instance.RegisterNetworkedEntityView(entity, newView);
        newView.gameObject.SetActive(true);

        LSLog.LogImportant($"Game Manager - New View Created!");

        onViewAdded?.Invoke(newView);
    }

    public T GetPlayerView<T>(string entityID) where T : ColyseusNetworkedEntityView
    {
        if (ExampleManager.Instance.HasEntityView(entityID))
        {
            return ExampleManager.Instance.GetEntityView(entityID) as T;
        }

        LSLog.LogError($"No player controller with id {entityID} found!");
        return null;
    }

    private void RemoveView(ColyseusNetworkedEntityView view)
    {
        view.SendMessage("OnEntityRemoved", SendMessageOptions.DontRequireReceiver);

        onViewRemoved?.Invoke(view);
    }
}