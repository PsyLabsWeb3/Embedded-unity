using Quantum;
using UnityEngine;

public abstract class MonoSingleton<T> : QuantumCallbacks
    where T : QuantumCallbacks
{
    public virtual bool DontDestroy()
    {
        return false;
    }

    public static T Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null || Instance == this)
        {
            Instance = gameObject.GetComponent<T>();
            
            if (DontDestroy())
            {
                DontDestroyOnLoad(gameObject);
            }

            OnAwake();
        }
        else if (Instance != this)
        {
            if (gameObject.activeInHierarchy)
            {
                Destroy(this);
            }
        }
    }

    public virtual void OnAwake() { }
}
