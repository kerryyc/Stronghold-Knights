﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public float health = 10f;
    public float speed = 5f;

    [HideInInspector] public bool attack = false;

    public bool isPlayer1 = true;
    public bool isPlayer2 = false;

    //direction variables
    private int DOWN = 1;
    private int UP = 2;
    private int LEFT = 3;
    private int RIGHT = 4;
    private int direction = 1;

    //damage over time variables
    private float lastDamageTime = 0f;
    public float damagePeriod = 2f;

    //private component variables
    private Rigidbody2D rb2d;
    private Animator anim;
    private SpriteRenderer spriteRend;

    //variables to enable blinking of sprite
    private float spriteBlinkingTimer = 0.0f;
    private float spriteBlinkingMiniDuration = 0.1f;
    [HideInInspector] public float spriteBlinkingTotalTimer = 0.0f;
    private float spriteBlinkingTotalDuration = 1.0f;
    [HideInInspector] public bool startBlinking = false;

    // Melee sound effect
    private AudioSource soundSource;
    public AudioClip deathSoundEffect;
    public AudioClip meleeSoundEffect;
    public GameObject onDeathObject;
    private bool isDeathSoundPlayed = false;

    void Awake() {
        rb2d = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRend = GetComponent<SpriteRenderer>();
        soundSource = GetComponent<AudioSource>();

        
    }

    void Update() {
        //when health reaches 0
        if (health <= 0) {
            // play sound effect on death one time
            if (!isDeathSoundPlayed)
            {
                Debug.Log("Play death sound");
                soundSource.PlayOneShot(deathSoundEffect);
                //onDeathObject.SetActive(true);
                isDeathSoundPlayed = true;

            }
            //subtract alive player count
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (GameObject enemy in enemies)
                enemy.GetComponent<Enemy>().alivePlayers -= 1;


            //deactive player
            // moved this line of code into the disable player 
            // to invoke
            // this.gameObject.SetActive(false);

            Invoke("disablePlayer", .8f);
            
                
            return;
        }

        if (Time.timeScale != 0) {
            Move();
            Attack();

            if (startBlinking)
                SpriteBlinkingEffect();

        }
    }

    void OnCollisionEnter2D(Collision2D coll) {
        if (!attack && coll.gameObject.tag == "Enemy") {
            startBlinking = true; //start blinking effect
            health -= 1; //decrement health
            spriteBlinkingTotalTimer = 0f; //reset blinking timer
            lastDamageTime = Time.time;
        }
    }

    void OnCollisionStay2D(Collision2D coll) {
        if(Time.time - lastDamageTime >= damagePeriod) {
            OnCollisionEnter2D(coll);
        }
    }

    private void Attack() {
        if ((isPlayer1 && Input.GetButtonDown("Fire1")) || (isPlayer2 && Input.GetButtonDown("Fire2"))) {
            // play sound effect
            soundSource.PlayOneShot(meleeSoundEffect);
            attack = true;
            anim.SetTrigger("attack");
            Invoke("DisableAttack", 0.2f); //allow player invulnerability while attacking, disable in 0.2f
        }
    }

    private void Move() {
        //get directional input
        float horizontal = 0, vertical = 0;
        if (isPlayer1) {
            horizontal = Input.GetAxisRaw("Horizontal1") * speed;
            vertical = Input.GetAxisRaw("Vertical1") * speed;
        }
        else if (isPlayer2) {
            horizontal = Input.GetAxisRaw("Horizontal2") * speed;
            vertical = Input.GetAxisRaw("Vertical2") * speed;
        }

        if (horizontal != 0 && Mathf.Abs(horizontal) < 1) {
            horizontal = 0;
        }
        if (vertical != 0 && Mathf.Abs(vertical) < 1) {
            vertical = 0;
        }
        //set layer depending on direction
        SetLayer(direction, 0f); //set current layer weight to zero
        if (horizontal > 0)
            direction = RIGHT;
        else if (horizontal < 0)
            direction = LEFT;
        else if (vertical > 0)
            direction = UP;
        else if (vertical < 0)
            direction = DOWN;
        SetLayer(direction, 1f); //set new layer weight to one

        //set move animation
        if (horizontal != 0 || vertical != 0)
            anim.SetBool("move", true);
        else
            anim.SetBool("move", false);

        //make player move
    
        rb2d.velocity = new Vector2(horizontal,vertical);
    }

    private void SetLayer(int dir, float weight) {
        //gets layer name and then sets the layer to specified weight
        string layerName = SetLayerName(dir);
        anim.SetLayerWeight(anim.GetLayerIndex(layerName), weight);
    }

    private string SetLayerName(int dir) {
        //get layer name depending on direction
        string layerName = "Base Layer";
        if (direction == UP)
            layerName = "Up Layer";
        else if (direction == DOWN)
            layerName = "Base Layer";
        else if (direction == RIGHT)
            layerName = "Right Layer";
        else if (direction == LEFT)
            layerName = "Left Layer";
        return layerName;
    }

    private void SpriteBlinkingEffect() {
        //turns on and off sprite renderer to create blinking effect
        spriteBlinkingTotalTimer += Time.deltaTime;
        if (spriteBlinkingTotalTimer >= spriteBlinkingTotalDuration) {
            startBlinking = false;
            spriteBlinkingTotalTimer = 0.0f;
            spriteRend.enabled = true;
            return;
        }

        spriteBlinkingTimer += Time.deltaTime;
        if (spriteBlinkingTimer >= spriteBlinkingMiniDuration) {
            spriteBlinkingTimer = 0.0f;
            if (spriteRend.enabled)
                spriteRend.enabled = false;
            else
                spriteRend.enabled = true;
        }
    }

    private void DisableAttack() {
        attack = false;
    }
    private void disablePlayer()
    {
        //deactive player
        this.gameObject.SetActive(false);
    }

}
