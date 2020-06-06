using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class RunGame : MonoBehaviour
{

    public GameObject Player;
    public GameObject Enemy;
    protected WavesScript WavesScript;
    public int WavesChangeCounter = 50;
    private float currentTimer = 0.0f;
    private float enemyTimer = 0.0f;
    public int AddEnemiesCounter = 50;
    public int addedEnemies = 0;
    public int maxEnemies = 10;
    public int startEnemies = 5;

    private int runhealth;
    private int runscore;

    //UI properties
    [Header("Boat Settings")]
    public GameObject GameOverText;
    public GameObject HealthText;
    public GameObject ScoreText;
    public GameObject HighScoreText;

    public LevelManager levelmanager;

    private IEnumerator coroutine;
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<AudioSource>().Play();
        WavesScript = FindObjectOfType<WavesScript>();
        currentTimer = Time.deltaTime;
        enemyTimer = Time.deltaTime;
        for (int x = 0; x < startEnemies; x++)
            AddEnemies();
        runhealth = Player.GetComponent<WaterBoatScript>().health;
        runscore = Player.GetComponent<WaterBoatScript>().privateScore;
        if (PlayerPrefs.HasKey("currentNavalHighScore"))
            HighScoreText.GetComponent<Text>().text = "Highscore = " + PlayerPrefs.GetInt("currentNavalHighScore").ToString();
        }

    // Update is called once per frame
    void Update()
    {
        enemyTimer += Time.deltaTime;
        currentTimer += Time.deltaTime;

        if (currentTimer >= WavesChangeCounter)
        {
            currentTimer = Time.deltaTime;
            //1st Wave Alternate 
            WavesScript.GetComponent<WavesScript>().Octaves[0].speed = new Vector2(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f));
            WavesScript.GetComponent<WavesScript>().Octaves[0].scale = new Vector2(Random.Range(0.0f, 0.5f), Random.Range(0.0f, 0.5f));
            WavesScript.GetComponent<WavesScript>().Octaves[0].height = Random.Range(0.0f,0.5f);

            //2nd Wave Not Alternate
            WavesScript.GetComponent<WavesScript>().Octaves[1].speed = new Vector2(Random.Range(0.0f, 15.0f), Random.Range(0.0f, 15.0f));
            WavesScript.GetComponent<WavesScript>().Octaves[1].scale = new Vector2(Random.Range(0.0f, 15.0f), Random.Range(0.0f, 15.0f));
            WavesScript.GetComponent<WavesScript>().Octaves[1].height = Random.Range(0.0f, 1.0f);


            //Adding enemies on same time as waveChange
            if (addedEnemies < maxEnemies && Player!= null)
                AddEnemies();
        }
        if (enemyTimer >= AddEnemiesCounter)
        {
            enemyTimer = Time.deltaTime;
            if (addedEnemies < maxEnemies && Player != null)
                AddEnemies();
        }


            if (Player.GetComponent<WaterBoatScript>().health != runhealth)
        {
            //update health
            runhealth = Player.GetComponent<WaterBoatScript>().health;
            HealthText.GetComponent<Text>().text = "Health = " + runhealth.ToString();
            if (runhealth <= 0)
            {
                coroutine = WaitForExit(5.0f);
                StartCoroutine(coroutine);
                //game over
            }
        }
        if (Player.GetComponent<WaterBoatScript>().privateScore != runscore)
        {
            //update score
            runscore = Player.GetComponent<WaterBoatScript>().privateScore;
            ScoreText.GetComponent<Text>().text = "Score = " + runscore.ToString();
            newHighScore(runscore);
        }
    }

    private void AddEnemies()
    {
        addedEnemies++;
        GameObject newShip;
        //Vector3 newShipLocation = Player.transform.position;
        Vector3 newShipLocation = new Vector3(Random.Range(50f, 950f), 10.0f, Random.Range(50f, 950f));
        //if spawn is too close to player
        if (newShipLocation.x <= Player.transform.position.x + 100f && newShipLocation.x >= Player.transform.position.x - 100f)
            newShipLocation.x += 100f;

        Quaternion newShipRotation = Enemy.transform.localRotation;
        Quaternion temp = new Quaternion(Random.Range(-500f, 500f), Random.Range(0.0f, 360f), Random.Range(-500f, 500f), 0.0f);
        newShipRotation = temp * newShipRotation;
        newShip = Instantiate(Enemy, newShipLocation, newShipRotation) as GameObject;
        newShip.SetActive(true);
        //newShip.GetComponent<EnemyAIScript>().SteerPower = 0.0f;
        //newShip.GetComponent<EnemyAIScript>().Power = 0.0f;
        //newShip.GetComponent<EnemyAIScript>().MaxSpeed = 0.0f;
    }

    private void newHighScore(int score)
    {


        if (PlayerPrefs.HasKey("currentNavalHighScore"))
        {
            if (score > PlayerPrefs.GetInt("currentNavalHighScore"))
            {
                HighScoreText.GetComponent<Text>().text = "Highscore = " + PlayerPrefs.GetInt("currentNavalHighScore").ToString();
                PlayerPrefs.SetInt("currentNavalHighScore", score);
                PlayerPrefs.Save();
            }
        }
        else
        {
            PlayerPrefs.SetInt("currentNavalHighScore", score);
            PlayerPrefs.Save();
        }
    }

        IEnumerator WaitForExit(float time)
    {
        if (Player.tag == "Player")
        {
            GameOverText.SetActive(true);// = true;
            yield return new WaitForSeconds(time);
            levelmanager.LoadNavalMenu();
        }
        else
        {
            //endEpisodeAI
        }
    }
}
