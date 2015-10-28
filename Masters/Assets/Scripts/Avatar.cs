﻿using System;
using System.Collections;
using Assets.Scripts;
using Assets.Scripts.Attack;
using Assets.Shared.Scripts;
using UnityEngine;

[RequireComponent(typeof (Animator))]
public class Avatar : MonoBehaviour
{
    public float jumpDuration = 1;
    public float jumpHeight = 10;
    public float walkingSpead = 12.5f;
    public float runningSpeed = 25;
    public float speed = 15;
    public PlayerProfile PlayerProfile;
    private Tween tween;

    public SpecialAttack specialAttackPrefab;
    private Command CurrentDirection = Command.MoveNone;
    private float CurrentDirectionTimeStamp = -1;
    public GameObject Opponent { get; private set; }
    private float facingDirection;

    public float Direction { get; private set; }
    public bool AllowMovement { get; set; }

    private void Start()
    {
        var player = GameObject.FindGameObjectWithTag("asdfasdfads");
        var enemy = GameObject.FindGameObjectWithTag("asdfasdfasdfasaf");

        Opponent = gameObject == player ? enemy : player;
    }

    public enum State
    {
        Idle, Walking, Running, Jumping, Attacking, Laying, Blocking
    }

    public enum Command
    {
        MoveLeft, MoveRight, MoveNone, Jump, Block, Punch, Kick, Special,
        NoBlock
    }

    public State CurrentState { get; private set; }
    
    public void process(Command command)
    {
        if (CurrentState == State.Blocking)
        {
            if (command != Command.NoBlock)
            {
                return;
            }
        }

        if (command == Command.MoveLeft)
        {
            if (CurrentDirection != Command.MoveLeft)
            {
                CurrentDirection = Command.MoveLeft;
                CurrentDirectionTimeStamp = Time.time;
            }
        }
        
        if (command == Command.MoveRight)
        {
            if (CurrentDirection != Command.MoveRight)
            {
                CurrentDirection = Command.MoveRight;
                CurrentDirectionTimeStamp = Time.time;
            }
        }
        
        if (command == Command.MoveNone)
        {
            CurrentDirection = Command.MoveNone;
            CurrentDirectionTimeStamp = -1;
        }

        switch (CurrentState)
        {
            case State.Idle:
            case State.Walking:
            case State.Running:
            case State.Blocking:
                if (command == Command.Punch)
                {
                    GetComponent<Animator>().SetTrigger("Punch");
                    CurrentState = State.Attacking;
                }
                if (command == Command.Kick)
                {
                           GetComponent<Animator>().SetTrigger("Kick");
                    CurrentState = State.Attacking;
                }
                if (command == Command.Special)
                {
                    if (PlayerProfile.PowerBar.Value == 100)
                    {
                        special();
                        CurrentState = State.Attacking;
                    }
                }
                if (command == Command.Jump)
                {
                    CurrentState = State.Jumping;
                }
                if (command == Command.Block)
                {
                    CurrentState = State.Blocking;
                    GetComponent<Animator>().SetBool("Block", true);
                }
                if (command == Command.NoBlock)
                {
                    GetComponent<Animator>().SetBool("Block", false);
                    CurrentState = State.Idle;
                }
                break;
            case State.Attacking:
                // Attack animation or whatever is playing out.
                break;
        }
    }

    private void Update()
    {
        facingDirection = Math.Sign(Opponent.transform.position.x - gameObject.transform.position.x);
        var movingDirection = CurrentDirection == Command.MoveLeft ? -1 : 1;

        if (CurrentState == State.Blocking || CurrentState == State.Laying) return;
        if (CurrentState != State.Attacking)
        {
            if (CurrentDirectionTimeStamp == -1)
            {
                CurrentState = State.Idle;
            }
            else if (Time.time - CurrentDirectionTimeStamp > 0.5f)
            {
                CurrentState = State.Running;
            }
            else
            {
                CurrentState = State.Walking;
            }
        }
        switch (CurrentState)
        {
            case State.Idle:
                turnPlayer(facingDirection);
                GetComponent<Animator>().SetFloat("Speed", 0);
                break;
            case State.Walking:
                turnPlayer(facingDirection);
                move(movingDirection*walkingSpead);
                GetComponent<Animator>().SetFloat("Speed", 1);
                break;
            case State.Running:
                turnPlayer(movingDirection);
                move(movingDirection*runningSpeed);
                GetComponent<Animator>().SetFloat("Speed", 2);
                break;
            case State.Attacking:
                turnPlayer(facingDirection);
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void turnPlayer(float direction)
    {
        transform.localScale = new Vector3(Math.Sign(direction), 1, 1);
    }

    private void resetState()
    {
        if (CurrentDirectionTimeStamp != -1) CurrentDirectionTimeStamp = Time.time;
        if(CurrentState!=State.Blocking)CurrentState = State.Idle;
    }

    private void move(float speed)
    {
        Direction = Mathf.Sign(speed);
        if (AllowMovement) transform.AddX(speed*Time.deltaTime);
    }

    private void special()
    {
        GetComponent<Animator>().SetTrigger("Special");
        PlayerProfile.PowerBar.Value = 0;

        var newAttack = Instantiate(specialAttackPrefab);
        newAttack.transform.position = transform.position;
        newAttack.startAttack(this);
    }

    public void damage(float damage, Avatar opponent)
    {

        if (CurrentState == State.Blocking)
        {
            PlayerProfile.HealthBar.Value -= damage/5;
            iTween.MoveTo(gameObject,
                new Hashtable
                            {
                                {"x", gameObject.transform.position.x + -1*facingDirection},
                                {"time", 0.25f},
                                {"EaseType", "easeOutQuad"}
                            });
        }
        else
        {
            opponent.PlayerProfile.PowerBar.Value += 20;
            PlayerProfile.HealthBar.Value -= damage;
            if (damage <= 10)
            {
                iTween.MoveTo(gameObject,
                    new Hashtable
                            {
                                {"x", gameObject.transform.position.x + -5*facingDirection},
                                {"time", 0.25f},
                                {"EaseType", "easeOutQuad"}
                            });
                GetComponent<Animator>().SetTrigger("Hurt");
            }
            else
            {
                CurrentState = State.Laying;
                GetComponent<Animator>().SetTrigger("Fall");
            }
        }

        if (PlayerProfile.HealthBar.Value == 0)
        {
            GetComponent<Animator>().SetTrigger("Die");
            GetComponent<BoxCollider2D>().enabled = false;
        }
    }
}