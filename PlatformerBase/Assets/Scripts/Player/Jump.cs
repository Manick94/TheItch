﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class Jump : PhysicsObject, IHealthObject
{
    # region private fields
    [SerializeField] private float moveSpeed; //how fast the object can move

    [SerializeField] private float jumpSpeed; //initial jump speed
    [Range(-gravityMag, gravityMag)] [SerializeField] private float addedSpeed; //gravity added on the way up

    private bool jumping = false; //is the player jumping

    private Animator animator; //reference to attached animator component
    private SpriteRenderer render; //attached sprite renderer

    [SerializeField] private int maxHealth; //maximum health of the player
    private int health; //health of the object
    [SerializeField] private float invulnerabilityTime; //how long the invulnerability timer lasts
    private float invulnerabilityTimer;
    private bool invulnerable; //true when player is immune to damage

    private bool touchingLadder; //true when player is touching the ladder
    protected bool climbing;

    [SerializeField] private bool canSprint; 
    [SerializeField] private float sprintSpeed;

    #endregion

    #region Properties
    public int Health { get { return health; } }
    public int MaxHealth { get { return maxHealth; } }
    public bool Invulnerable { get { return invulnerable; } set { invulnerable = value; } }
    #endregion

    //Start is already being called in Base PhysicsObject Class
    private void Awake()
    {
        animator = GetComponent<Animator>();
        render = GetComponent<SpriteRenderer>();

        if (maxHealth < 1) { maxHealth = 1; } //must have at leath one health point
        health = maxHealth;
    }

    protected override void Update()
    {
        if (!paused)
        {
            #region Movement
            Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            input = input.normalized * Mathf.Clamp(input.magnitude, 0, 1.0f);
            moveVelocity = input * (canSprint && Input.GetAxis("Fire3") > 0 ? sprintSpeed : moveSpeed) * Time.deltaTime;

            if (Input.GetButtonDown("Jump"))
            {
                jumping = true;
            } //can input a jump before you hit the ground
            else if (Input.GetButtonUp("Jump")) { jumping = false; } //releasing jump always cancels jumping

            //jumping when on ground
            if (jumping && (grounded || climbing))
            {
                gravityVelocity = (inheritGravity ? groundNormal : Vector2.up) * jumpSpeed * 0.1f;
                jumping = false; //insures that you can only jump once after pressing jump button
                climbing = false; //jump out of climbing
            }

            //add velocity while moving upwards and jump key is pressed (long jumping)
            if (Vector2.Dot(gravity, gravityVelocity) < 0 && Input.GetButton("Jump")) //check if moving upwards
            {
                CollideOneway(false);
                //add to velocity in direction of velocity proportional to velocity magnitude
                gravityVelocity += addedSpeed * gravityVelocity.normalized * Time.deltaTime;
            }
            //moving downwards
            else
            {
                CollideOneway(true);
            }

            //fall throught one way platforms when input is down
            if (Vector2.Dot(gravity, moveVelocity) > 0)
            {
                CollideOneway(false);
            }

            //start climbing if move velocity is up or down
            if (touchingLadder)
            {
                //if not moving side to side
                if (Vector2.Dot(moveVelocity, gravity) != 0)
                {
                    climbing = true;
                }
            }
            #endregion

            if (climbing && gravityVelocity.magnitude == 0)
            {
                CollideOneway(false); //don't collide with oneways while climbing 
                grounded = false;

                #region climbing collision
                float distance = moveVelocity.magnitude; //temporary distance to surface
                Vector2 moveVector = moveVelocity; //Project the moveVelocity onto the ground

                float numCollisions = rb2D.Cast(moveVector, filter, hits, distance);
                for (int i = 0; i < numCollisions; i++)
                {
                    //check not collision inside an object
                    if (hits[i].distance != 0)
                    {
                        //collide with the closest
                        if (hits[i].distance <= distance)
                        {
                            distance = hits[i].distance; //set new closest distance
                        }
                    }
                }
                if (distance > buffer) { rb2D.position = moveVector.normalized * (distance - buffer); } //move object by the shortest distance
                if (moveVector.magnitude != 0) { sprite.flipX = Vector2.Dot(transform.right, moveVector) < 0; } //face the correct direction
                #endregion
            }
            else
            {
                base.Update();
            }

            #region Animation

            //send values to animator
            animator.SetBool("grounded", grounded);
            animator.SetFloat("verticalVel", gravityVelocity.magnitude * (Vector2.Angle(gravity, gravityVelocity) > 90 ? 1 : -1));
            animator.SetFloat("horizontalMove", moveVelocity.magnitude * (Vector2.Dot(transform.right, moveVelocity) < 0 ? 1 : -1));

            render.color = Color.white;
            if (invulnerable)
            {
                invulnerabilityTimer -= Time.deltaTime;
                if (invulnerabilityTimer <= invulnerabilityTime - 0.2f)
                {
                    animator.SetBool("damage", false);
                    render.color = Color.gray; //fade Slightly
                }

                if (invulnerabilityTimer <= 0)
                {
                    invulnerable = false;
                }
            }
            #endregion
        }
    }


    /// <summary>
    /// choose whether the object should collide with oneway platforms
    /// </summary>
    /// <param name="collide">true if object collides</param>
    private void CollideOneway(bool collide)
    {
        Physics2D.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("Oneway"), !collide);
        filter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
    }

    protected override void HitSpikes()
    {
        TakeDamage(2);
    }

    //Sent when another object enters a trigger collider attached to this object (2D physics only).
    private void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.gameObject.layer == LayerMask.NameToLayer("Ladder")) {

            touchingLadder = true;
        }
    }

    private void OnTriggerExit2D(Collider2D coll)
    {
        //stop climbing when no longer touching ladder
        if (coll.gameObject.layer == LayerMask.NameToLayer("Ladder"))
        {
            touchingLadder = false;
            climbing = false;
        }
    }

    #region Health
    public void TakeDamage(int amount)
    {
        if (!invulnerable)
        {
            health -= amount;
            invulnerable = true;
            invulnerabilityTimer = invulnerabilityTime; //start the timer
            animator.SetBool("damage", true);
            //death code
            if (health <= 0)
            {
                health = 0;
            }
        }
    }

    public void Heal(int amount)
    {
        //add health up to max health
        health = (health + amount) % maxHealth;
    }

    public void FullHeal()
    {
        health = maxHealth;
    }
    #endregion
}