﻿using UnityEngine;

[RequireComponent(typeof(UIAnchor))]
public class InventoryDisplay : Inventory {

    [SerializeField] private Vector2 hiddenOffset; //relative position of display when hidden
    [SerializeField] private float moveSpeed; //how fast to hide and show

    private Transform display; //child object with the inventory display 
    private bool hidden = true; //true when inventory is empty and display not showing
    private Vector2 moveVector; //temporary vector when moving 

	void Awake () {
        inventoryUI = transform; //set the inventory transform to this object
        display = transform.GetChild(0);
        //may need to be removed later, makes sure no items stored when new level is loaded
        Items.Clear();
    }

    private void Start()
    {
        display.localPosition = hiddenOffset; //move the display to a hidden position
        hidden = true;
    }

    private void Update()
    {
        //display is hidden but there are items
        if (hidden && Items.Count > 0) 
        {
            MoveToLocalPosition(Vector2.zero);
        }
        //display is showing but there are no items
        else if(!hidden && Items.Count == 0)
        {
            MoveToLocalPosition(hiddenOffset);
        }
    }

    /// <summary>
    /// Moves the inventoryDisplay to a target location local to inventory
    /// </summary>
    /// <param name="target">position to move to</param>
    private void MoveToLocalPosition(Vector2 target)
    {
        moveVector = (((Vector3)target) - display.localPosition).normalized * moveSpeed * Time.deltaTime;
        //check if close enought to snap into position
        if ((((Vector3)target) - display.localPosition).magnitude < moveVector.magnitude)
        {
            display.localPosition = target; //snap into position
            hidden = !hidden; //toggle hidden
            return; //stop moving
        }
        display.localPosition += (Vector3)moveVector;
    }
}
