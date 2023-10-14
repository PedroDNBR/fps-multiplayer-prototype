using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Item : ScriptableObject
{
    [Header("Basic Item Info")]
    public string name;
    public Image uiImage;
    public GameObject prefab;

    [Header("Item Dimensions (mm/g/ml)")]
    public int lenght;
    public int width;
    public int height;
    public int weight;
    public int volume;
}
