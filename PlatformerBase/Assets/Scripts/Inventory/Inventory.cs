﻿using System.Collections.Generic;
using UnityEngine;

public class Inventory : Global {

    /// <summary>
    /// add on to Global for classes that interact with inventory 
    /// </summary>

    private static Dictionary<string, GameObject> items; //player inventory

    protected static Transform inventoryUI; //ref to the transform of the inventory display

    protected const int inventorySize = 6;

    //Offsets and spacings between items in inventory
    private const float offsetY = 3.75f; //offset from anchor along y axis
    private const float spacing = -1.5f; //spacing between individual items

    protected static GameObject collectEffect; //instantiated collect Effect

    protected static Dictionary<string, int> allItemsStates; //keeps track of all the items states for level transitions and data loading

    public Dictionary<string, GameObject> Items {
        get {
            if(items == null)
            {
                items = new Dictionary<string, GameObject>();
            }
            return items;
        }
        //To set items use AddItem() and RemoveItem
    } 

    public static int[] ItemStates {
        get {
            int[] states = new int[allItemsStates.Count];
            int i = 0;
            foreach(int state in allItemsStates.Values)
            {
                states[i] = state;
                i++;
            }
            return states;
        }
    }

    public static void ClearItemStates()
    {
        allItemsStates = null;
    }
    
    /// <summary>
    /// Add an item into the player's inventory
    /// </summary>
    /// <param name="name">name of the object</param>
    /// <param name="obj">gameobject</param>
    public void AddItem(string name, GameObject obj)
    {
        Items.Add(name, obj);
        allItemsStates[name] = 1;
        obj.transform.parent = inventoryUI; //make child of the inventoryUI
        DisplayItems();
    }

    /// <summary>
    /// remove an item from the player's inventory
    /// </summary>
    /// <param name="name"></param>
    public void RemoveItem(string name)
    {
        if (Items.ContainsKey(name))
        {
            Items[name].GetComponent<CollectableItem>().Eaten(transform);
            //Destroy(Items[name]); //destroy the gameObject
            Items.Remove(name);
            allItemsStates[name] = 2;
}
        DisplayItems();
    }

    /// <summary>
    /// make the items appear in the correct location on screen
    /// </summary>
    protected void DisplayItems()
    {
        int index = 0;
        foreach(string item in Items.Keys)
        {
            //position item relative to inventoryUI
            Items[item].transform.localPosition = new Vector2(0.0f, offsetY + (index * spacing));
            index++;
        }
    }
}
 