using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour
{

    [Header("Bullet Collide Element")]
    [SerializeField] private GameObject Targetexplosion = null;
    [SerializeField] private GameObject Waterexplosion = null;

    [Header("Damage of bullet")]
    [SerializeField] public int damage;

    [Header("Player who sent the bullet")]
    [SerializeField] public Transform shooter;
    // Start is called before the first frame update

    protected WavesScript WavesScript;


    void Start()
    {
        WavesScript = FindObjectOfType<WavesScript>();
    }

    // Update is called once per frame
    void Update()
    {

        if (transform.position.y <= WavesScript.GetHeight(transform.position) && Waterexplosion != null)
        {
            WaterExplode();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag != "Cannonball" && Targetexplosion!= null)
        {
            CollideExplode();
        }
    }

    private void WaterExplode()
    {
        Waterexplosion.GetComponent<ParticleSystem>().Play();
        //Waterexplosion.GetComponent<AudioSource>().Play();
        GetComponent<Rigidbody>().isKinematic = true;
        GetComponent<Rigidbody>().useGravity = false;
    }
    private void CollideExplode()
    {
        Targetexplosion.GetComponent<ParticleSystem>().Play();
        Targetexplosion.GetComponent<AudioSource>().Play();
        GetComponent<Rigidbody>().isKinematic = true;
        GetComponent<Rigidbody>().useGravity = false;
    }
}
