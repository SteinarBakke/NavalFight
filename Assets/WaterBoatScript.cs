using Ditzelgames;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WaterBoatScript : MonoBehaviour
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
    public  int health;
    public  int privateScore = 0;
    public int reloadTime = 5;
    private bool dead = false;


    //Camera
    [Header("Camera Settings")]
    public Camera RightCamera;
    public Camera LeftCamera;
    public Camera ThirdPersonCamera;
    public Slider rightSlider;
    public Slider leftSlider;

    //used Components
    protected Rigidbody Rigidbody;
    protected Quaternion StartRotation;
    protected ParticleSystem ParticleSystem;
    private IEnumerator coroutine;


    public void Awake()
    {
        health = maxHealth;
        Rigidbody = GetComponent<Rigidbody>();
        StartRotation = Motor.localRotation;
        RightCamera.gameObject.SetActive(false);
        LeftCamera.gameObject.SetActive(false);
        ThirdPersonCamera.gameObject.SetActive(true);

        //UI
        rightSlider.value = reloadTime;
        leftSlider.value = reloadTime;
        leftSlider.gameObject.transform.Find("Fill Area").Find("Fill").GetComponent<Image>().color = Color.green;
        rightSlider.gameObject.transform.Find("Fill Area").Find("Fill").GetComponent<Image>().color = Color.grey;
    }
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
                if (leftSide == true)
                {
                    leftSlider.gameObject.transform.Find("Fill Area").Find("Fill").GetComponent<Image>().color = Color.green;
                    rightSlider.gameObject.transform.Find("Fill Area").Find("Fill").GetComponent<Image>().color = Color.grey;
                }
                else
                {
                    leftSlider.gameObject.transform.Find("Fill Area").Find("Fill").GetComponent<Image>().color = Color.grey;
                    rightSlider.gameObject.transform.Find("Fill Area").Find("Fill").GetComponent<Image>().color = Color.green;
                }
            }

            if (Input.GetKeyDown(KeyCode.F3))
            {
                ThirdPersonCamera.GetComponent<AudioListener>().enabled = false;
                ThirdPersonCamera.gameObject.SetActive(false);
                RightCamera.GetComponent<AudioListener>().enabled = false;
                RightCamera.gameObject.SetActive(false);
                LeftCamera.GetComponent<AudioListener>().enabled = true;
                LeftCamera.gameObject.SetActive(true);
            }
            if (Input.GetKeyDown(KeyCode.F2))
            {
                ThirdPersonCamera.GetComponent<AudioListener>().enabled = false;
                ThirdPersonCamera.gameObject.SetActive(false);
                LeftCamera.GetComponent<AudioListener>().enabled = false;
                LeftCamera.gameObject.SetActive(false);
                RightCamera.GetComponent<AudioListener>().enabled = true;
                RightCamera.gameObject.SetActive(true);
            }
            if (Input.GetKeyDown(KeyCode.F1))
            {
                LeftCamera.GetComponent<AudioListener>().enabled = false;
                LeftCamera.gameObject.SetActive(false);
                RightCamera.GetComponent<AudioListener>().enabled = false;
                RightCamera.gameObject.SetActive(false);
                ThirdPersonCamera.GetComponent<AudioListener>().enabled = true;
                ThirdPersonCamera.gameObject.SetActive(true);
            }

            //rotating cannons + making sure rotation is between 5-70%
            if (Input.GetKey(KeyCode.DownArrow))
                MoveCannon(-1.0f);

            if (Input.GetKey(KeyCode.UpArrow))
                MoveCannon(1.0f);

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
            if (ParticleSystem != null)
                {
                    if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S))
                        ParticleSystem.Play();
                    else
                        ParticleSystem.Pause();
                }

                //doing this to only call this method when reload is active
                if (!CanShoot)
                {
                    ReloadTimer += Time.deltaTime; // can print reload here
                    rightSlider.value = ReloadTimer;
                    leftSlider.value = ReloadTimer;
                    if (ReloadTimer >= reloadTime)
                    {
                        CanShoot = true;
                        rightSlider.value = reloadTime;
                        leftSlider.value = reloadTime;
                    }
                }

                if (Input.GetKeyDown(KeyCode.Space) && CanShoot)
                {
                    CanShoot = false;
                    ReloadTimer = Time.deltaTime;
                    Shoot();
                }
            }
        }

    //need to divide slider by 6 for some reason? 
    // also adding failsafe if it get's stuck at -5 or -70

    private void MoveCannon(float rotate)
    {
        GameObject[] cannons = leftSide ? LeftCannons : RightCannons;
        Slider tempSlider = leftSide ? leftSlider : rightSlider;
        for (int count = 0; count < cannons.Length; count++)
        {
            if ((cannons[count].transform.localEulerAngles.x - 360) < -5 && (cannons[count].transform.localEulerAngles.x - 360) > -70)
            {
                cannons[count].transform.Rotate(rotate, 0.0f, 0.0f, Space.Self);
                tempSlider.transform.Rotate(0.0f, 0.0f, rotate / cannons.Length, Space.Self);
            }
            else if ((cannons[count].transform.localEulerAngles.x - 360) >= -5) //failsafe 
            {
                cannons[count].transform.Rotate(-1.0f, 0.0f, 0.0f, Space.Self);
                tempSlider.transform.Rotate(0.0f, 0.0f, -1.0f / cannons.Length, Space.Self);
            }
            else if ((cannons[count].transform.localEulerAngles.x - 360) <= 70) //failsafe 
            {
                cannons[count].transform.Rotate(1.0f, 0.0f, 0.0f, Space.Self);
                tempSlider.transform.Rotate(0.0f, 0.0f, 1.0f / cannons.Length, Space.Self);
            }

        }
    }

    private void Shoot()
    {
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
        if (other.gameObject.tag == "AI")
        {
            health -= 50;
        }
    }



    IEnumerator WaitForExit(float time)
    {
        //this.gameObject.SetActive(false);
        yield return new WaitForSeconds(time);
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

