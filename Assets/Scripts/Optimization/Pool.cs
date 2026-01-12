using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pool : MonoBehaviour
{
    #region Serialized Fields
    [Header("Setup")]
    [SerializeField] GameObject _prefab;
    [SerializeField] Transform _parent;
    [SerializeField] WindowType _windowType;
    [SerializeField] PoolItemType _poolType;

    [Header("Watchlist")]
    [Tooltip("The amount of items to create in the pool.")]
    [SerializeField] private int _poolCount;
    [Tooltip("The Available Items ready for use.")]
    [SerializeField] private int _counter;
    [SerializeField] private List<GameObject> _listPool = new List<GameObject>();
    [SerializeField] private List<bool> _listUsed = new List<bool>(); // checker if the pool object is already used
    #endregion // Serialized Fields

    #region Public API
    public int AvailableItems => _counter;

    public bool IsPoolCompatible(PoolItemType poolType, WindowType windowType)
        => (_poolType == poolType && _windowType == windowType);

    public GameObject TakeFromPool()
    {
        for (int i = _listPool.Count - 1; i >= 0; i--)
        {
            // it is better to use the _listUsed for checking if the pool object is already used
            // instead of the _listPool's activeSelf or activeInHierarchy
            if (!_listUsed[i]) 
            {
                _listPool[i].SetActive(true);
                _listUsed[i] = true;
                _counter--;
                PoolManager.Instance.UpdateAvailableCount(_windowType, _poolType);
                return _listPool[i];
            }
        }

        return null;
    }

    public void ReturnToPool(GameObject gameObject)
    {
        if (_parent == null)
        {
            return;
        }

        if (_listPool.Contains(gameObject))
        {
            gameObject.SetActive(false);
            _listUsed[_listPool.IndexOf(gameObject)] = false;
            gameObject.transform.SetParent(_parent);
            RectTransform rectTransform = gameObject.transform as RectTransform;
            rectTransform.localPosition = Vector3.zero;
            rectTransform.localEulerAngles = Vector3.zero;
            rectTransform.localScale = new Vector3(1, 1, 1);
            _counter++;
            PoolManager.Instance.UpdateAvailableCount(_windowType, _poolType);
        }
    }

    public void SetPoolCount(int poolCount)
    {
        _poolCount = poolCount;
    }

    public void Generate()
    {
        if (_parent == null)
        {
            return;
        }

        int count = CalculateNeededItems();

        if (count <= 0)
        {
            Debug.LogWarning($"Cannot Generate Pool with {count} items.");
            return;
        }

        GameObject temp = null;
        for (int i = 0; i < count; i++)
        {
            temp = Instantiate(_prefab, _parent);
            temp.SetActive(false);

            _listPool.Add(temp);
            _listUsed.Add(false);
        }

        _counter = _poolCount;
        PoolManager.Instance.UpdateAvailableCount(_windowType, _poolType);
    }

    public void Clear()
    {
        if (_parent == null)
        {
            return;
        }

        _listPool.Clear();
        _listUsed.Clear();
        _counter = 0;
        PoolManager.Instance.UpdateAvailableCount(_windowType, _poolType);

        while (_parent.childCount > 0)
        {
            DestroyImmediate(_parent.GetChild(0).gameObject);
        }
    }
    #endregion // Public API

    #region Private Methods
    private int CalculateNeededItems()
    {
        if (_parent == null)
        {
            Debug.LogWarning($"Parent for type {_poolType} and window {_windowType} is null.");
            return 0;
        }

        return (_poolCount - _parent.childCount);
    }
    #endregion // Private Methods
}
