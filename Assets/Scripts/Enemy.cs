﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Enemy : MonoBehaviour {

    public float health = 1f;
    public float speed = 2f;
    public float knockback = 250f;
    public int alivePlayers = 2;

    private float damageCoolDown = 1f;
    [HideInInspector] public bool doKnockback = false;
    private bool curDamagePlayer = false;

    private float lastDamageTime = 0f;
    public float damagePeriod = 0.5f;

    private float lastKnockTime = 0f;
    private float knockPeriod = 0.5f;

    private GameObject[] allPlayers;
    private GameObject player;
    private Rigidbody2D rb2d;
    private Animator anim;

    void Awake() {
        rb2d = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Update() {
        //if not being knocked back, erase all forces
        if (!doKnockback)
            rb2d.velocity = new Vector2(0, 0);

        //when health is 0
        if (health <= 0)
            Destroy(this.gameObject);

        //checks for player that is closest to enemy (might need to be optimized)
        allPlayers = GameObject.FindGameObjectsWithTag("Player");
        if (alivePlayers > 0) {
            int minIndex = 0;
            float minDistance = float.MaxValue;
            for (int i = 0; i < alivePlayers; ++i) {
                float distance = Vector2.Distance(allPlayers[i].transform.position, transform.position);
                if (distance < minDistance) {
                    minIndex = i;
                    minDistance = distance;
                }
            }
            player = allPlayers[minIndex];

            //moves toward player until it reaches certain distance
            transform.position = Vector2.MoveTowards(transform.position, player.transform.position, speed * Time.deltaTime);
            updateAnimations();
        }
    }

    void OnCollisionEnter2D(Collision2D other) {
        //only take damage if player is attacking, otherwise player is damaged
        if (other.gameObject.tag == "Player") {
            //TODO: optimize script to allow for ease of changes if adding more players
            if ((other.gameObject.GetComponent<PlayerController>() != null && other.gameObject.GetComponent<PlayerController>().attack) ||
                (other.gameObject.GetComponent<Player2Controller>() != null && other.gameObject.GetComponent<Player2Controller>().attack)) {
                --health;
                lastDamageTime = Time.time; //update when enemy was last damaged
                doKnockback = true;
                //allow for enemies to get knocked back upon getting hit
                var force = transform.position - other.transform.position;
                force.Normalize();
                rb2d.AddForce(force * knockback);
                Invoke("StopForce", 0.2f);
            }
        }

        if(other.gameObject.tag == "Enemy" && other.gameObject.GetComponent<Enemy>().doKnockback) {
            doKnockback = true;
            //allow for enemies to get knocked back upon getting hit
            var force = transform.position - other.transform.position;
            force.Normalize();
            rb2d.AddForce(force * knockback);
            Invoke("StopForce", 0.2f);
        }

        if (other.gameObject.tag == "PlayerBoundary")
        {
            // if enemy collides with player boundary, ignore collision
            Physics2D.IgnoreCollision(other.collider, this.GetComponent<Collider2D>());
        }
    }

    void OnCollisionStay2D(Collision2D other) {
        //only take damage if player is attacking, otherwise player is damaged
        if (other.gameObject.tag == "Player") {
            //TODO: optimize script to allow for ease of changes if adding more players
            if ((Time.time - lastDamageTime >= damagePeriod) && (
                (other.gameObject.GetComponent<PlayerController>() != null && other.gameObject.GetComponent<PlayerController>().attack) ||
                (other.gameObject.GetComponent<Player2Controller>() != null && other.gameObject.GetComponent<Player2Controller>().attack))) {
                --health;
                doKnockback = true;
                lastDamageTime = Time.time; //update when enemy was last damaged
                //allow for enemies to get knocked back upon getting hit
                var force = transform.position - other.transform.position;
                force.Normalize();
                rb2d.AddForce(force * knockback);
                Invoke("StopForce", 0.2f);
            }
        }

        if (Time.time - lastKnockTime >= knockPeriod && other.gameObject.tag == "Enemy" && other.gameObject.GetComponent<Enemy>().doKnockback) {
            doKnockback = true;
            //allow for enemies to get knocked back upon getting hit
            var force = transform.position - other.transform.position;
            force.Normalize();
            rb2d.AddForce(force * 50);
            lastKnockTime = Time.time;
            Invoke("StopForce", 0.2f);
        }
    }

    private void StopForce() {
        //stop knockback force from being applied forever
        var force = transform.position - player.transform.position;
        force.Normalize();
        rb2d.AddForce(-force * knockback);
        doKnockback = false;
    }

    private void updateAnimations() {
        Vector2 detectDistance = transform.position - player.transform.position;
        if (detectDistance.y > detectDistance.x && detectDistance.y > 0)
            anim.Play("skel_run_down");
        else if (detectDistance.y < detectDistance.x && detectDistance.y < 0)
            anim.Play("skel_run_up");
        else if (detectDistance.x > 0)
            anim.Play("skel_run_left");
        else if (detectDistance.x < 0)
            anim.Play("skel_run_right");
           
    }
}
