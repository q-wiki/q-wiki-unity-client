using JetBrains.Annotations;
using UnityEngine;

/// <summary>
/// This implementation of the singleton pattern was originally made by XenoRo:
/// https://gamedev.stackexchange.com/users/16498/xenoro
/// In this thread it is implied that it is free to use:
/// https://gamedev.stackexchange.com/a/151547
/// </summary>
/// <typeparam name="T">The class which should be turned into a singleton.</typeparam>
public abstract class Singleton<T> : Singleton where T : MonoBehaviour
{
    #region  Properties

    [NotNull]
    public static T Instance
    {
        get
        {
            if (Quitting)
            {
                Debug.LogWarning(
                    $"[{nameof(Singleton)}<{typeof(T)}>] Instance will not be returned because the application is quitting.");
                return null;
            }

            lock (Lock)
            {
                if (_instance != null)
                    return _instance;
                var instances = FindObjectsOfType<T>();
                var count = instances.Length;
                if (count > 0)
                {
                    if (count == 1)
                        return _instance = instances[0];
                    Debug.LogWarning(
                        $"[{nameof(Singleton)}<{typeof(T)}>] There should never be more than one {nameof(Singleton)} of type {typeof(T)} in the scene, but {count} were found. The first instance found will be used, and all others will be destroyed.");
                    for (var i = 1; i < instances.Length; i++)
                        Destroy(instances[i]);
                    return _instance = instances[0];
                }

                Debug.Log(
                    $"[{nameof(Singleton)}<{typeof(T)}>] An instance is needed in the scene and no existing instances were found, so a new instance will be created.");
                return _instance = new GameObject($"({nameof(Singleton)}){typeof(T)}")
                    .AddComponent<T>();
            }
        }
    }

    #endregion

    #region  Fields

    [CanBeNull] private static T _instance;

    [NotNull]
    private static readonly object Lock = new object();

    [SerializeField] private bool _persistent = true;

    #endregion

    #region  Methods

    private void Awake()
    {
        if (_persistent)
            DontDestroyOnLoad(gameObject);
        OnAwake();
    }

    protected virtual void OnAwake()
    {
    }

    #endregion
}

public abstract class Singleton : MonoBehaviour
{
    #region  Properties

    public static bool Quitting { get; private set; }

    #endregion

    #region  Methods

    private void OnApplicationQuit()
    {
        Quitting = true;
    }

    #endregion
}