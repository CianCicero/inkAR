using System;
using System.Collections.Generic;
using UnityEngine;

public class UnityMainThreadDispatcher : MonoBehaviour
{
    private static readonly Queue<Action> _executionQueue = new Queue<Action>();

    // Singleton instance
    private static UnityMainThreadDispatcher _instance;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        // Execute all queued actions on the main thread
        while (_executionQueue.Count > 0)
        {
            var action = _executionQueue.Dequeue();
            action.Invoke();
        }
    }

    // Add an action to be executed on the main thread
    public static void Enqueue(Action action)
    {
        lock (_executionQueue)
        {
            _executionQueue.Enqueue(action);
        }
    }
}
