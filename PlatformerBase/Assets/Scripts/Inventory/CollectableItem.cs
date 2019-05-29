﻿using UnityEngine;

public enum ItemType { Key, Gem, Lily, Skull, Letter }
public enum ItemStyle { Default, Blood, Heal, Water, Virus}

[RequireComponent(typeof(SpriteRenderer))]
public class CollectableItem : Inventory
{
    private const float speed = 4.0f;
    private const float minMoveDistance = 0.05f;

    [SerializeField] private bool persists;
    private bool collected = false;
    private bool used = false;
    private bool moving = false;

    private Vector3 targetPosition = Vector3.zero;
    private Vector2 pickupPosition;
    private Vector2 inventoryPosition;

    private SpriteRenderer render;

    private AudioPlayer audioPlayer;

    [SerializeField] private ItemType itemtype;
    [SerializeField] private ItemStyle itemStyle;

    public ItemType ItemType { get { return itemtype; } }
    public ItemStyle ItemStyle { get { return itemStyle; } }
    public bool Persists { get { return persists; } }

    public void Awake()
    {
        render = GetComponent<SpriteRenderer>();
        pickupPosition = transform.position;

        audioPlayer = GetComponentInParent<AudioPlayer>();
    }

    private void Start()
    {
        if (persists && used) { return; } //exit out when creating for 
        if (allItemsStates[gameObject.name] != 0 && !collected) { gameObject.SetActive(false); }
    }

    private void OnTriggerEnter2D(Collider2D coll)
    {
        //check if collision with player
        if (!collected && coll.CompareTag("Player"))
        {
            PlayCollectionEffectAt(transform.position);

            AddItem(gameObject.name, gameObject); //add item to inventory and move to final location in UI

            Collected();

            targetPosition = transform.localPosition;
            transform.position = pickupPosition; //return to origional position for animation
             //start the animation
            moving = true;

            audioPlayer.PlaySound("itemCollect");
        }
    }

    void Update()
    {
        if (!Pause.menuPaused)
        {
            //Lerp into position
            if ((collected || used) && moving)
            {
                Vector2 move = new Vector2(Mathf.Lerp(transform.localPosition.x, targetPosition.x, speed * Time.deltaTime),
                                                             Mathf.Lerp(transform.localPosition.y, targetPosition.y, speed * Time.deltaTime));
                if ((targetPosition - transform.localPosition).magnitude <=  minMoveDistance)
                {
                    transform.localPosition = targetPosition; //snap into position
                    //if (used) { render.enabled = false; } 

                    moving = false;
                    return; //stop moving
                }
                //move at speed greater than min move speed
                transform.localPosition = move.magnitude > minMoveDistance ? move : move.normalized * minMoveDistance;
            }
        }
    }

    public void Collected()
    {
        SpriteRenderer rend = GetComponent<SpriteRenderer>(); //make sure sorting layer and order is just above inventoryUI
        SpriteRenderer invRend = inventoryUI.GetChild(0).GetComponent<SpriteRenderer>();
        rend.sortingLayerID = invRend.sortingLayerID;
        rend.sortingOrder = invRend.sortingOrder + 1;
        collected = true;
    }

    public void Eaten(Transform target)
    {
        transform.parent = target;
        targetPosition = Vector2.zero;

        Debug.Log("EAT");

        SpriteRenderer targetRender = target.GetComponent<SpriteRenderer>();
        if (targetRender != null)
        {
            render.sortingLayerID = targetRender.sortingLayerID;
            render.sortingOrder = targetRender.sortingOrder + (persists ? 1: - 1);
        }
        collected = true; 
        used = true;
        moving = true;

        //audioPlayer.PlaySound("keyUnlock");
    }
}
