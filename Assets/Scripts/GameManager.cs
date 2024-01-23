using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.IO;

public class GameManager : MonoBehaviour
{
    public int stage;
    public int maxStage;

    public Animator stageAnim;
    public Animator clearAnim;
    public Animator fadeAnim;
    public Transform playerPosition;

    public string[] enemyObjs;
    public Transform[] spawnPoints;

    public float nextSpawnDelay;
    public float curSpawnDelay;

    public GameObject player;
    public TextMeshProUGUI scoreText;
    public Image[] lifeImage;
    public Image[] boomImage;
    public GameObject gameOverSet;
    public GameObject gameClearSet;
    public ObjectManager objectManager;

    public AudioClip audioDie;
    public AudioClip audioItem;
    public AudioClip audioClear;
    public AudioClip audioBoss;

    public List<Spawn> spawnList;
    public int spawnIndex;
    public bool spawnEnd;

    AudioSource audioSource;
        
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        enemyObjs = new string[] { "EnemyS", "EnemyM", "EnemyL", "EnemyB"};
        spawnList = new List<Spawn>();
        StageStart();
    }

    public void PlaySound(string type)
    {
        audioSource.volume = 0.3f;
        switch (type)
        {
            case "DIE":
                audioSource.clip = audioDie;
                audioSource.volume = 0.1f;
                break;
            case "ITEM":
                audioSource.clip = audioItem;
                break;
            case "CLEAR":
                audioSource.clip = audioClear;
                break;
            case "BOSS":
                audioSource.clip = audioBoss;
                audioSource.volume = 1f;
                break;
        }
        audioSource.Play();
    }

        public void StageStart()
    {
        // Player Reposition
        player.transform.position = playerPosition.position;
        player.SetActive(true);

        // Stage UI Load
        stageAnim.SetTrigger("On");
        stageAnim.GetComponent<TextMeshProUGUI>().text = "STAGE " + stage + "\nSTART";
        clearAnim.GetComponent<TextMeshProUGUI>().text = "STAGE " + stage + "\nCLEAR";

        // Enemy Spawn File Read
        ReadSpawnFile();

        // Fade In
        fadeAnim.SetTrigger("In");
    } 

    public void StageEnd()
    {
        PlaySound("CLEAR");
        player.SetActive(false);

        // Clear UI Load
        clearAnim.SetTrigger("On");
        clearAnim.speed = 0.7f;

        // Fade Out
        fadeAnim.SetTrigger("Out");

        // Stage Increament
        stage++;
        if (stage > maxStage)
            Invoke("GameClear", 5);
        else
            Invoke("StageStart", 5);
    }

    void ReadSpawnFile()
    {
        // 초기화
        spawnList.Clear();
        spawnIndex = 0;
        spawnEnd = false;

        // 리스폰 파일 읽기
        // TextAsset : 텍스트 파일 에셋 클래스
        TextAsset textFile = Resources.Load("Stage " + stage.ToString()) as TextAsset; // as를 쓰면 뒤에 있는 클래스가 아닐 경우 null을 반환함
        // StringReader : 파일 내의 문자열 데이터 읽기 클래스
        StringReader stringReader = new StringReader(textFile.text);

        while(stringReader != null)
        {
            string line = stringReader.ReadLine(); // ReadLine : 텍스트 데이터를 한 줄씩 반환 (자동 줄 바꿈)
            Debug.Log(line);

            if (line == null)
                break;

            // 리스폰 데이터 생성
            Spawn spawnData = new Spawn();
            spawnData.delay = float.Parse(line.Split(',')[0]);
            spawnData.type = line.Split(',')[1];
            spawnData.point = int.Parse(line.Split(',')[2]);
            spawnList.Add(spawnData);
        }

        // 텍스트 파일 닫기
        stringReader.Close();

        nextSpawnDelay = spawnList[0].delay;
    }

    void Update()
    {
        curSpawnDelay += Time.deltaTime;

        if(curSpawnDelay >= nextSpawnDelay && !spawnEnd)
        {
            SpawnEnemy();
            curSpawnDelay = 0;
        }

        Player playerLogic = player.GetComponent<Player>();
        scoreText.text = string.Format("{0:n0}", playerLogic.score); // {0:n0}은 세자리마다 쉼표로 나눠주는 숫자 양식
    }

    void SpawnEnemy()
    {
        int enemyIndex = 0;

        switch(spawnList[spawnIndex].type)
        {
            case "S":
                enemyIndex = 0;
                break;
            case "M":
                enemyIndex = 1;
                break;
            case "L":
                enemyIndex = 2;
                break;
            case "B":
                enemyIndex = 3;
                break;
        }

        int enemyPoint = spawnList[spawnIndex].point;
        GameObject enemy = objectManager.MakeObj(enemyObjs[enemyIndex]);
        enemy.transform.position = spawnPoints[enemyPoint].position;

        Rigidbody2D rigid = enemy.GetComponent<Rigidbody2D>();
        Enemy enemyLogic = enemy.GetComponent<Enemy>();
        enemyLogic.player = player;
        enemyLogic.objectManager = objectManager;
        enemyLogic.gameManager = this;

        if (enemyPoint == 5 || enemyPoint == 7) // 왼쪽에서 생성될 경우
        {
            enemy.transform.Rotate(Vector3.forward * 90);
            rigid.velocity = new Vector2(enemyLogic.speed, -1);
        }
        else if (enemyPoint == 6 || enemyPoint == 8) // 오른쪽에서 생성될 경우
        {
            enemy.transform.Rotate(Vector3.back * 90);
            rigid.velocity = new Vector2(enemyLogic.speed*(-1), -1);
        }
        else
            rigid.velocity = new Vector2(0, enemyLogic.speed *(-1));

        // 리스폰 인덱스 증가
        spawnIndex++;
        if (spawnIndex == spawnList.Count) // 다 소환했을 경우
        {
            spawnEnd = true;
            return;
        }
        // 다음 리스폰 딜레이 갱신
        nextSpawnDelay = spawnList[spawnIndex].delay;
    }

    public void RespawnPlayer()
    {
        Invoke("RespawnPlayerEx", 2f);
    }

    void RespawnPlayerEx()
    {
        player.transform.position = Vector3.down * 3.5f;
        player.SetActive(true);

        Player playerLogic = player.GetComponent<Player>();
        playerLogic.isHit = false;
    }

    public void UpdateLifeImage(int life)
    {
        for(int i = 0; i < 3; i++)
        {
            lifeImage[i].color = new Color(1, 1, 1, 0);
        }

        for (int i = 0; i < life; i++)
        {
            lifeImage[i].color = new Color(1, 1, 1, 1);
        }
    }

    public void UpdateBoomImage(int boom)
    {
        for (int i = 0; i < 3; i++)
        {
            boomImage[i].color = new Color(1, 1, 1, 0);
        }

        for (int i = 0; i < boom; i++)
        {
            boomImage[i].color = new Color(1, 1, 1, 1);
        }
    }

    public void CallExplosion(Vector3 pos, string type)
    {
        GameObject explosion = objectManager.MakeObj("Explosion");
        Explosion explosionLogic = explosion.GetComponent<Explosion>();

        explosion.transform.position = pos;
        explosionLogic.StartExplosion(type);
    }

    public void GameOver()
    {
        gameOverSet.SetActive(true);
    }

    public void GameRetry()
    {
        SceneManager.LoadScene(0);
    }

    public void GameClear()
    {
        gameClearSet.SetActive(true);
    }
}
