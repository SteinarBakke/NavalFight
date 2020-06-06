using Ditzelgames;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class EnemyAIDummy : MonoBehaviour
{


    //visible properties
    [Header("Boat Settings")]
    public Transform Motor;
    public float SteerPower = 500f;
    public float Power = 5f;
    public float MaxSpeed = 10f;
    public float Drag = 0.1f;

    //guns and fighting
    [Header("Weapons Settings")]
    public GameObject[] LeftCannons;
    public GameObject[] RightCannons;
    public bool leftSide = true;
    public float bulletForce;
    public bool CanShoot = true;
    private float ReloadTimer = 0.0f;

    //Player
    [Header("Player Settings")]
    public int maxHealth;
    public int health;
    public int privateScore = 0;
    public int reloadTime = 5;
    private bool dead = false;
    public int winScore = 150;


    //used Components
    protected Rigidbody Rigidbody;
    protected Quaternion StartRotation;
    protected ParticleSystem ParticleSystem;
    private IEnumerator coroutine;

    public RunGame rungame;


    public void Awake()
    {
        health = maxHealth;
        Rigidbody = GetComponent<Rigidbody>();
        StartRotation = Motor.localRotation;
    }
    

    

    void FixedUpdate()
    {
        if (!dead)
        {
            if (health <= 0)
            {
                //coroutine = WaitForSending(1.0f);
                //StartCoroutine(coroutine);
                dead = true;
                WaitForExit();
                //coroutine = WaitForExit();
                //StartCoroutine(coroutine);
            }

            if (privateScore >= winScore)
            {
                Debug.Log("Never gonna happen");
                //Win

            }

            //doing this to only call this method when reload is active
            if (!CanShoot)
            {
                ReloadTimer += Time.deltaTime; // can print reload here
                if (ReloadTimer >= reloadTime)
                {
                    CanShoot = true;
                }
            }

            //compute vector
            //using this instead of .forward because the x and z will be the same
            var forward = Vector3.Scale(new Vector3(1, 0, 1), transform.forward);


        }
    }

    



    public void Heuristic(float[] actionsOut)
    {
        actionsOut[0] = 0;
        actionsOut[1] = 0; // seperating shooting and cannons to seperate
        if (Input.GetKey("w")) //forward
            actionsOut[0] = 1;
        if (Input.GetKey("s")) //backward
            actionsOut[0] = 2;
        if (Input.GetKey("a")) //left
            actionsOut[0] = 3;
        if (Input.GetKey("d")) // right
            actionsOut[0] = 4;
        if (Input.GetKeyDown("x")) //switch cannon
            actionsOut[1] = 1;
        if (Input.GetKey(KeyCode.DownArrow)) //arrange cannon
            actionsOut[1] = 2;
        if (Input.GetKey(KeyCode.UpArrow)) //arrange cannon
            actionsOut[1] = 3;
        if (Input.GetKey("space") && CanShoot) //shopt
            actionsOut[1] = 4;
    }
    

    private void shoot()
    {
        //adding a small reward for shooting
        CanShoot = false;
        ReloadTimer = Time.deltaTime;
        GameObject[] Cannons;
        if (leftSide)
            Cannons = LeftCannons;
        else
            Cannons = RightCannons;
        for (int x = 0; x < Cannons.Length; x++)
        {
            Cannons[x].GetComponent<ParticleSystem>().Play();
            Cannons[x].GetComponent<AudioSource>().Play();
            GameObject bullet = Cannons[x].transform.GetChild(0).gameObject;
            GameObject tempBullet;
            tempBullet = Instantiate(bullet, bullet.transform.position, Cannons[x].transform.localRotation) as GameObject;
            tempBullet.GetComponent<Rigidbody>().isKinematic = false;
            tempBullet.SetActive(true);
            tempBullet.GetComponent<Rigidbody>().AddForce(bullet.transform.forward * bulletForce);
            Destroy(tempBullet, 8.0f);
        }

    }

    private void MoveCannon(float rotate)
    {
        GameObject[] cannons = leftSide ? LeftCannons : RightCannons;
        for (int count = 0; count < cannons.Length; count++)
        {
            if ((cannons[count].transform.localEulerAngles.x - 360) < -5 && (cannons[count].transform.localEulerAngles.x - 360) >= -71)
                cannons[count].transform.Rotate(rotate, 0.0f, 0.0f, Space.Self);
            else if ((cannons[count].transform.localEulerAngles.x - 360) >= -5)
                cannons[count].transform.Rotate(-1.0f, 0.0f, 0.0f, Space.Self);
            else if ((cannons[count].transform.localEulerAngles.x - 360) <= 70)
                cannons[count].transform.Rotate(1.0f, 0.0f, 0.0f, Space.Self);
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Cannonball")
        {
            other.gameObject.GetComponent<BulletScript>().shooter.SendMessage("AddToScore", other.gameObject.GetComponent<BulletScript>().damage);
            Debug.Log(this.gameObject.name + " Got Hit By " + other.gameObject.GetComponent<BulletScript>().shooter.name);
            health -= other.gameObject.GetComponent<BulletScript>().damage;
        }
        if (other.gameObject.tag == "wall")
        {
            health = 0;
        }
        if (other.gameObject.tag == "AI" || other.gameObject.tag == "Player")
        {
            health -= 50;
        }
    }



    private void WaitForExit()
    {

      
        //reset episode
        rungame.GetComponent<RunGame>().addedEnemies -= 1;
        Destroy(this.gameObject);
        
    }

    IEnumerator WaitForSending(float time)
    {
        yield return new WaitForSeconds(time);
        this.gameObject.SetActive(false);
    }


    public void AddToScore(int points)
    {
        this.privateScore += points;
    }
}

