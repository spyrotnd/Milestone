using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "NewGameEvent", menuName = "Game Events/Game Event")]
public class GameEventSO : ScriptableObject
{
    private readonly List<UnityAction> listeners = new List<UnityAction>();

    public void Raise()
    {
        for (int i = listeners.Count - 1; i >= 0; i--)
        {
            listeners[i].Invoke();
        }
    }

    public void RegisterListener(UnityAction listener)
    {
        if (!listeners.Contains(listener))
        {
            listeners.Add(listener);
        }
    }

    public void UnregisterListener(UnityAction listener)
    {
        listeners.Remove(listener);
    }
}
