using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemDetail
{
    [SerializeField]
    string itemName;
    [SerializeField]
    ObjectItem item;
    [SerializeField]
    List<Category> categories;

    public ObjectItem Item => item;
    public List<Category> Categories => categories;
}