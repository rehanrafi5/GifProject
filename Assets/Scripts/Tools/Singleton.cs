using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Instance { get; private set; }

    protected virtual void Awake()
    {
        CacheInstance();
    }

    private void CacheInstance()
    {
        if (Instance == null)
        {
            Instance = GetComponent<T>();
        }
        else
        {
            var type = typeof(T).ToString();

            Debug.Log("Singleton<" + type + "> instance created already.");

            DestroyImmediate(this.gameObject);
        }
    }
}