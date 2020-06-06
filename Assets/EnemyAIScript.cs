using Ditzelgames;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class EnemyAIScript : Agent
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
    public int maxStep = 500;
    private int stepCounter = 0;
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
        Vector3 newShipLocation = new Vector3(UnityEngine.Random.Range(50f, 950f), 10.0f, UnityEngine.Random.Range(50f, 950f));
        this.transform.position = newShipLocation;
    }
    
    public override void OnEpisodeBegin()
    {
        //this.gameObject.GetComponent<AudioSource>().Play();
        stepCounter = 0;
        dead = false;
        health = maxHealth;
        privateScore = 0;
        Rigidbody = GetComponent<Rigidbody>();
        StartRotation = Motor.localRotation;
        Vector3 newShipLocation = new Vector3(UnityEngine.Random.Range(50f, 950f), 10.0f, UnityEngine.Random.Range(50f, 950f));
        this.transform.position = newShipLocation;
        //start location

    }

    /*
    void FixedUpdate()
    {
        if (!dead)
        {
            if (health <= 0)
            {
                coroutine = WaitForSending(1.0f);
                StartCoroutine(coroutine);
                dead = true;
                coroutine = WaitForExit(10.0f);
                StartCoroutine(coroutine);
            }

            if (Input.GetKeyDown(KeyCode.X)) //change shoot direciton with X atm
            {
                leftSide = !leftSide;
            }

            //rotating cannons + making sure rotation is between 5-70%
            if (Input.GetKey(KeyCode.DownArrow))
                MoveCannon(-1.0f);

            if (Input.GetKey(KeyCode.UpArrow))
                MoveCannon(1.0f);



            //rotational forcec
            //Rigidbody.AddForceAtPosition(steer * transform.right * SteerPower / 100f, Motor.position);

            //compute vector
            //using this instead of .forward because the x and z will be the same
            var forward = Vector3.Scale(new Vector3(1, 0, 1), transform.forward);
            //Controls
            if (Input.GetKey(KeyCode.A))
                Rigidbody.AddForceAtPosition(1 * transform.right * SteerPower / 100f, Motor.position);
            if (Input.GetKey(KeyCode.D))
                Rigidbody.AddForceAtPosition(-1 * transform.right * SteerPower / 100f, Motor.position);
            if (Input.GetKey(KeyCode.W))
                PhysicsHelper.ApplyForceToReachVelocity(Rigidbody, forward * MaxSpeed, Power);
            if (Input.GetKey(KeyCode.S))
                PhysicsHelper.ApplyForceToReachVelocity(Rigidbody, forward * -MaxSpeed, Power);

            //Motor animation // particle system
            //Motor.SetPositionAndRotation(Motor.position, transform.rotation * StartRotation * Quaternion.Euler(0, 30f * steer, 0));

            //doing this to only call this method when reload is active
            if (!CanShoot)
            {
                ReloadTimer += Time.deltaTime; // can print reload here
                if (ReloadTimer >= reloadTime)
                {
                    CanShoot = true;
                }
            }

            if (Input.GetKeyDown(KeyCode.Space) && CanShoot)
            {
                shoot();
            }
        }
    }
    */

    //normalizedValue = (currentValue - minValue)/(maxValue - minValue)

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(gameObject.transform.position.x / 1000);//1 //MaxXValue
        sensor.AddObservation(gameObject.transform.position.y / 15);//2 // MaxValue
        sensor.AddObservation(gameObject.transform.position.z / 1000);//3 //MaxZValue
        sensor.AddObservation(gameObject.transform.rotation.x / 10);//4
        sensor.AddObservation(gameObject.transform.rotation.y / 180);//5
        sensor.AddObservation(gameObject.transform.rotation.z / 10);//6
        sensor.AddObservation(CanShoot);//7
        sensor.AddObservation(leftSide);//8
        for (int x = 0; x< LeftCannons.Length; x++) //8 & 3 = 24 :: 8-34
        {
            sensor.AddObservation((LeftCannons[x].transform.rotation.x - 5) / (85 - 5));
            sensor.AddObservation(LeftCannons[x].transform.rotation.y / 70);
            //sensor.AddObservation(LeftCannons[x].gameObject.transform.position);
            //sensor.AddObservation(LeftCannons[x].transform.localRotation);
            //GameObject bullet = LeftCannons[x].transform.GetChild(0).gameObject;
            //sensor.AddObservation(bullet.transform.position.x / 1000);
            //sensor.AddObservation(bullet.transform.position.y);
            //sensor.AddObservation(bullet.transform.position.z / 1000);
        }
        for (int x = 0; x < RightCannons.Length; x++) //8 & 3 = 24 :: 34-56
        {
            sensor.AddObservation((RightCannons[x].transform.rotation.x - 5) / (85-5) );
            sensor.AddObservation(RightCannons[x].transform.rotation.y / 70);
            //GameObject bullet = RightCannons[x].transform.GetChild(0).gameObject;
            //sensor.AddObservation(bullet.transform.position.x / 1000);
            //sensor.AddObservation(bullet.transform.position.y);
            //sensor.AddObservation(bullet.transform.position.z / 1000);
        }
        //sensor.AddObservation(bulletForce); //57
    }

    public override void OnActionReceived(float[] vectorArray)
    {
        stepCounter++;
        //actions
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
                SetReward(1.0f);
                EndEpisode();
                //Win
            }

            if (stepCounter > maxStep)
            {
                EndEpisode();
            }

            SetReward(-0.001f); // to force AI to gain points
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


            Vector3 controlSignal = Vector3.zero;

            if (Mathf.FloorToInt(vectorArray[0]) == 1)
                PhysicsHelper.ApplyForceToReachVelocity(Rigidbody, forward * MaxSpeed, Power);
            if (Mathf.FloorToInt(vectorArray[0]) == 2)
                PhysicsHelper.ApplyForceToReachVelocity(Rigidbody, forward * -MaxSpeed, Power / 2);
            if (Mathf.FloorToInt(vectorArray[0]) == 3)
            {
                //adding a small penality for rotating
                SetReward(-0.0001f);
                Rigidbody.AddForceAtPosition(1 * transform.right * SteerPower / 100f, Motor.position);
            }
            if (Mathf.FloorToInt(vectorArray[0]) == 4)
            {
                //adding a small penality for rotating
                SetReward(-0.0001f);
                Rigidbody.AddForceAtPosition(-1 * transform.right * SteerPower / 100f, Motor.position);
            }
            if (Mathf.FloorToInt(vectorArray[1]) == 1) //switch
            {
                //adding a small reward for switching
                SetReward(0.00005f);
                leftSide = !leftSide;
            }
            if (Mathf.FloorToInt(vectorArray[1]) == 2) //arrange Downarrow
            {
                //adding a small reward for moving cannon
                SetReward(0.00001f);
                MoveCannon(-1.0f);
            }
            if (Mathf.FloorToInt(vectorArray[1]) == 3) //arrange uparrow
            {
                //adding a small reward for moving cannon
                SetReward(0.00001f);
                MoveCannon(1.0f);
            }
            if (Mathf.FloorToInt(vectorArray[1]) == 4 && CanShoot) //Shoot
            {
                //adding a small reward for shooting cannon
                SetReward(0.001f);
                shoot();
            }
        }
    }




    public override void Heuristic(float[] actionsOut)
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
            SetReward(-0.05f);
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
        SetReward(-1.0f);
        //this.gameObject.SetActive(false);
        //yield return new WaitForSeconds(time);
        //Destroy(this.gameObject); // not working right now
        //reset episode
        EndEpisode();
        //rungame.GetComponent<RunGame>().addedEnemies -= 1;
        //Destroy(this.gameObject);
        
    }

    IEnumerator WaitForSending(float time)
    {
        yield return new WaitForSeconds(time);
        this.gameObject.SetActive(false);
    }


    public void AddToScore(int points)
    {
        SetReward(0.2f);
        this.privateScore += points;
    }
}

