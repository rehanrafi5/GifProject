using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ObjectAction
{
    Added,
    Removed,
    Modified
}

public class ObjectSaveState
{
    public Vector2 AnchorMin;
    public Vector2 AnchorMax;
    public Vector2 AnchoredPosition;
    public Vector2 SizeDelta;
    public Vector3 LocalScale;
    public Quaternion Rotation;
    public int Number;

    public ObjectSaveState(RectTransform transform)
    {
        CopyTransform(transform);
    }

    public void CopyTransform(RectTransform transform)
    {
        AnchorMin = transform.anchorMin;
        AnchorMax = transform.anchorMax;
        AnchoredPosition = transform.anchoredPosition;
        SizeDelta = transform.sizeDelta;
        Rotation = transform.rotation;
        LocalScale = transform.localScale;
    }
}

public class ActionSaveState
{
    public ObjectAction Action;
    public PaintObject PaintObject;
    public Sprite Sprite;
    public int InstanceId;
    public int SiblingIndex;
    public bool IsSetPlaced;
    public int SetGroup;
    public ObjectSaveState PreviousObjectState;
    public ObjectSaveState CurrentObjectState;

    public ActionSaveState(ObjectAction action, PaintObject paintObject, Sprite sprite, int instanceId, int siblingIndex, bool isSetPlaced, int setGroup)
    {
        Action = action;
        PaintObject = paintObject;
        Sprite = sprite;
        InstanceId = instanceId;
        SiblingIndex = siblingIndex;
        IsSetPlaced = isSetPlaced;
        SetGroup = setGroup;
    }
}

public class UndoManager : Singleton<UndoManager>
{
    [SerializeField] private Button _undoButton;
    [SerializeField] private Button _redoButton;

    private Stack<ActionSaveState> _pastSaveStates = new Stack<ActionSaveState>();
    private Stack<ActionSaveState> _futureSaveStates = new Stack<ActionSaveState>();

    private ObjectSaveState _selectedObjectState;
    private ActionSaveState _currentSaveState;
    private ObjectAction _currentAction;
    private int number;

    private void Start()
    {
        ObjectManager.Instance.OnObjectAdded += HandleObjectAdded;
        ObjectManager.Instance.OnObjectSelected += HandleObjectSelected;
        ObjectManager.Instance.OnObjectModified += HandleObjectModified;

        _undoButton.onClick.AddListener(Undo);
        _redoButton.onClick.AddListener(Redo);
    }

    private void OnDestroy()
    {
        if(ObjectManager.Instance != null)
        {
            ObjectManager.Instance.OnObjectAdded -= HandleObjectAdded;
            ObjectManager.Instance.OnObjectSelected -= HandleObjectSelected;
            ObjectManager.Instance.OnObjectModified -= HandleObjectModified;
        }

        _undoButton.onClick.RemoveListener(Undo);
        _redoButton.onClick.RemoveListener(Redo);
    }

    private void Update()
    {
        _undoButton.interactable = _currentSaveState != null;
        _redoButton.interactable = _futureSaveStates.Count > 0;
    }

    private void HandleObjectAdded()
    {
        _currentAction = ObjectAction.Added;
    }

    public void SaveDeletionState()
    {
        _currentAction = ObjectAction.Removed;
        HandleObjectModified();
    }

    private void HandleObjectSelected()
    {
        _currentAction = ObjectAction.Modified;
        _selectedObjectState = new ObjectSaveState(ObjectManager.Instance.SelectedObject.transform as RectTransform);
        _selectedObjectState.Number = number;
        number++;
    }

    private void HandleObjectModified()
    {
        PaintObject selectedObject = ObjectManager.Instance.SelectedObject;
        ObjectSaveState currentObjectState = new ObjectSaveState(selectedObject.transform as RectTransform);
        currentObjectState.Number = number;
        number++;

        ActionSaveState actionSaveState = new ActionSaveState(
            _currentAction,
            selectedObject,
            selectedObject.GetImageSprite(),
            selectedObject.GetInstanceID(),
            selectedObject.transform.GetSiblingIndex(),
            selectedObject.GetIsSetPlaced(),
            (selectedObject.GetIsSetPlaced() ? selectedObject.GetSetGroup() : 0)
        );

        actionSaveState.PreviousObjectState = _selectedObjectState;
        actionSaveState.CurrentObjectState = currentObjectState;

        SetSaveState(actionSaveState);

        _selectedObjectState = currentObjectState;
        _currentAction = ObjectAction.Modified;

        Debug.Log("Saved Action: " + actionSaveState.PreviousObjectState.Number + " | " + actionSaveState.CurrentObjectState.Number + " -- " + _selectedObjectState.Number);
    }

    private void SetSaveState(ActionSaveState actionSaveState)
    {
        if(_currentSaveState != null)
        {
            _pastSaveStates.Push(_currentSaveState);
            Debug.Log("Pushing: " + _currentSaveState.PreviousObjectState.Number + " | " + _currentSaveState.CurrentObjectState.Number);
        }

        _currentSaveState = actionSaveState;
        _futureSaveStates.Clear();
    }

    private void ReorderLayer(Transform transform, int index)
    {
        LayeredObject obj = ObjectManager.Instance.GetCachedLayeredObject();
        if (obj == null)
        {
            Debug.LogWarning("Cached Layered Object is null.");
            return;
        }

        obj.ReorderLayer(index);
    }

    public void Undo()
    {
        ActionSaveState saveState = _currentSaveState;
        bool isSetPlaced = false;
        int setGroup = 0;
        if (saveState == null)
        {
            Debug.Log("No current save.");
            return;
        }
        else
        {
            isSetPlaced = saveState.IsSetPlaced;
            setGroup = saveState.SetGroup;
        }

        if(saveState.Action == ObjectAction.Modified)
        {
            RectTransform transform = saveState.PaintObject.transform as RectTransform;
            SetTransformValues(transform, saveState.PreviousObjectState);

            Debug.Log("Undo: " + saveState.PreviousObjectState.Number);
        }

        else if(saveState.Action == ObjectAction.Added)
        {
            ObjectManager.Instance.DeleteObject(saveState.PaintObject);
        }

        else if(saveState.Action == ObjectAction.Removed)
        {
            saveState.PaintObject = ObjectManager.Instance.RecreateObject(saveState.Sprite);

            RectTransform transform = saveState.PaintObject.transform as RectTransform;
            SetTransformValues(transform, saveState.PreviousObjectState);
            _currentAction = ObjectAction.Modified;

            ReorderLayer(transform, saveState.SiblingIndex);

            OverrideInstanceObjects(saveState);
        }

        _futureSaveStates.Push(_currentSaveState);
        _selectedObjectState = _currentSaveState.PreviousObjectState;

        if(_pastSaveStates.Count > 0)
        {
            _currentSaveState = _pastSaveStates.Pop();
            Debug.Log("Poping: " + _currentSaveState.PreviousObjectState.Number + " | " + _currentSaveState.CurrentObjectState.Number + " -- " + _selectedObjectState.Number);
        }

        else
        {
            _currentSaveState = null;
            Debug.Log("No current save: " + _selectedObjectState.Number);
        }

        _undoButton.interactable = _currentSaveState != null;
        ObjectManager.Instance.RefreshControls();

        // IF THE OBJ UNDONE & THE NEXT OBJ IS SET, UNDO THAT TOO
        if (_currentSaveState == null)
        {
            return;
        }
        else if (isSetPlaced && _currentSaveState.IsSetPlaced && setGroup == _currentSaveState.SetGroup)
        {
            Undo();
        }
    }

    public void Redo()
    {
        ActionSaveState saveState = _futureSaveStates.Pop();
        bool isSetPlaced = false;
        int setGroup = 0;
        if (saveState == null)
        {
            Debug.Log("No current save.");
            return;
        }
        else
        {
            isSetPlaced = saveState.IsSetPlaced;
            setGroup = saveState.SetGroup;
        }

        if (saveState.Action == ObjectAction.Modified)
        {
            if(saveState.PaintObject == null)
            {
                //Fail safe, just in case.
                PaintObject[] paintObjects = FindObjectsOfType<PaintObject>(true);

                for(int i = 0; i < paintObjects.Length; i++)
                {
                    if(saveState.InstanceId == paintObjects[i].GetInstanceID())
                    {
                        saveState.PaintObject = paintObjects[i];
                        break;
                    }    
                }
                Debug.Log("Error here");
            }

            RectTransform transform = saveState.PaintObject.transform as RectTransform;
            SetTransformValues(transform, saveState.CurrentObjectState);
            Debug.Log("Redo: " + saveState.CurrentObjectState.Number);
        }

        else if(saveState.Action == ObjectAction.Removed)
        {
            ObjectManager.Instance.DeleteObject(saveState.PaintObject);
        }

        else if(saveState.Action == ObjectAction.Added)
        {
            saveState.PaintObject = ObjectManager.Instance.RecreateObject(saveState.Sprite);

            RectTransform transform = saveState.PaintObject.transform as RectTransform;
            SetTransformValues(transform, saveState.CurrentObjectState);
            _currentAction = ObjectAction.Modified;

            ReorderLayer(transform, saveState.SiblingIndex);

            OverrideInstanceObjects(saveState);
        }

        if(_currentSaveState != null)
        {
            _pastSaveStates.Push(_currentSaveState);
            Debug.Log("Pushing: " + _currentSaveState.PreviousObjectState.Number + " | " + _currentSaveState.CurrentObjectState.Number);
        }

        _currentSaveState = saveState;
        _selectedObjectState = _currentSaveState.CurrentObjectState;
        Debug.Log("Now: " + _currentSaveState.PreviousObjectState.Number + " | " + _currentSaveState.CurrentObjectState.Number + " -- " + _selectedObjectState.Number);

        _redoButton.interactable = _futureSaveStates.Count > 0;
        ObjectManager.Instance.RefreshControls();

        // IF THE OBJ REDO & THE NEXT OBJ IS SET, REDO THAT TOO
        if (_futureSaveStates.Count > 0)
        {
            ActionSaveState futureSave = _futureSaveStates.Peek();
            if (futureSave == null)
            {
                return;
            }
            else if (isSetPlaced && futureSave.IsSetPlaced && setGroup == futureSave.SetGroup)
            {
                Redo();
            }
        }
    }

    public void Clear()
    {
        _currentSaveState = null;
        _pastSaveStates.Clear();
        _futureSaveStates.Clear();
    }

    private void SetTransformValues(RectTransform transform, ObjectSaveState saveState)
    {
        transform.anchorMin = saveState.AnchorMin;
        transform.anchorMax = saveState.AnchorMax;
        transform.anchoredPosition = saveState.AnchoredPosition;
        transform.sizeDelta = saveState.SizeDelta;
        transform.localScale = saveState.LocalScale;
        transform.rotation = saveState.Rotation;
    }

    private void OverrideInstanceObjects(ActionSaveState currentState)
    {
        int newInstanceId = currentState.PaintObject.GetInstanceID();
        int instanceId = currentState.InstanceId;

        foreach(ActionSaveState saveState in _pastSaveStates)
        {
            if(saveState.InstanceId == instanceId)
            {
                saveState.PaintObject = currentState.PaintObject;
                saveState.InstanceId = newInstanceId;
            }
        }

        foreach(ActionSaveState saveState in _futureSaveStates)
        {
            if(saveState.InstanceId == instanceId)
            {
                saveState.PaintObject = currentState.PaintObject;
                saveState.InstanceId = newInstanceId;
            }
        }

        if(_currentSaveState != null && _currentSaveState.InstanceId == instanceId)
        {
            _currentSaveState.PaintObject = currentState.PaintObject;
            _currentSaveState.InstanceId = newInstanceId;
        }

        currentState.InstanceId = newInstanceId;
    }
}
