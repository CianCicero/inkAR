using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Helper class for executing actions on the main Unity thread from background threads
/// </summary>
public class MainThreadDispatcher : MonoBehaviour
{
    private static MainThreadDispatcher _instance;
    
    // Queue of actions to be executed on the main thread
    private readonly Queue<Action> _executionQueue = new Queue<Action>();
    
    // Thread-safe lock object
    private readonly object _lockObject = new object();
    
    // Singleton instance
    public static MainThreadDispatcher Instance
    {
        get
        {
            if (_instance == null)
            {
                // Create a GameObject with MainThreadDispatcher attached
                GameObject go = new GameObject("MainThreadDispatcher");
                _instance = go.AddComponent<MainThreadDispatcher>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }
    
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void Update()
    {
        // Process all queued actions
        lock (_lockObject)
        {
            while (_executionQueue.Count > 0)
            {
                _executionQueue.Dequeue().Invoke();
            }
        }
    }
    
    /// <summary>
    /// Enqueues an action to be executed on the main thread
    /// </summary>
    /// <param name="action">Action to execute</param>
    public void Enqueue(Action action)
    {
        lock (_lockObject)
        {
            _executionQueue.Enqueue(action);
        }
    }
    
    /// <summary>
    /// Executes an action on the main thread and waits for it to complete
    /// </summary>
    /// <param name="action">Action to execute</param>
    /// <returns>Task that completes when the action is done</returns>
    public Task EnqueueAsync(Action action)
    {
        TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
        
        Enqueue(() =>
        {
            try
            {
                action();
                tcs.SetResult(true);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });
        
        return tcs.Task;
    }
}