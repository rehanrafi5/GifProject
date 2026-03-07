
using System;
using UnityEngine;

/// <summary>
/// Serializable class for test.
/// </summary>
[Serializable]
public class EasyIOTestClass
{
    public string Name;
    public string ID;

    public int Value1;
    public int Value2;

    public string[] Values;

    public string Dump()
    {
        string text = "Name: " + Name + "\n";
        text += "ID: " + ID + "\n";
        text += "Value1: " + Value1 + "\n";
        text += "Value2: " + Value2 + "\n";

        text += "Values: ";
        if (Values != null && Values.Length > 0)
        {
            for (int i = 0; i < Values.Length; i++)
            {
                text += Values[i] + (i >= Values.Length - 1 ? "" : ", ");
            }
        }

        Debug.Log(text);
        return text;
    }
}