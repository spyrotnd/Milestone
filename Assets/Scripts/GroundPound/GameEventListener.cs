using UnityEngine;
using UnityEngine.Events;

public class GameEventListener : MonoBehaviour
{
    [Tooltip("The event asset to listen for.")]
    public GameEventSO gameEvent;

    [Tooltip("What happens when the event is raised. Wire particles, sounds, camera shake or UI here in the Inspector.")]
    public UnityEvent response;

    private void OnEnable()
    {
        if (gameEvent != null) gameEvent.RegisterListener(OnEventRaised);
    }

    private void OnDisable()
    {
        if (gameEvent != null) gameEvent.UnregisterListener(OnEventRaised);
    }

    private void OnEventRaised()
    {
        response.Invoke();
    }
}
