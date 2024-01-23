using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public bool isTouchTop;
    public bool isTouchBottom;
    public bool isTouchLeft;
    public bool isTouchRight;

    public int life;
    public int score;
    public float speed;
    public int power;
    public int maxPower;
    public int boom;
    public int maxBoom;
    public float maxShotDelay;
    public float curShotDelay;

    public bool isHit;
    public bool isBoomTime;

    public GameObject bulletObjA;
    public GameObject bulletObjB;
    public GameObject boomEffect;
    public GameObject[] followers;

    public GameManager gameManager;
    public ObjectManager objectManager;

    public AudioClip audioFire;
    public AudioClip audioBoom;

    public bool isRespawnTime;

    public bool[] joyControl;
    public bool isControl;
    public bool isButtonA;
    public bool isButtonB;

    Animator anim;
    SpriteRenderer spriteRenderer;
    AudioSource audioSource;

    void Awake()
    {
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
    }

    void OnEnable()
    {
        Unbeatable();
        Invoke("Unbeatable", 3);
    }

    void PlaySound(string type)
    {
        audioSource.volume = 0.3f;
        switch (type)
        {
            case "FIRE":
                audioSource.clip = audioFire;
                break;
            case "BOOM":
                audioSource.clip = audioBoom;
                audioSource.volume = 0.5f;
                break;
        }
        audioSource.Play();
    }

    void Unbeatable()
    {
        isRespawnTime = !isRespawnTime;

        if(isRespawnTime) // 무적 시간 동안 투명 상태로 만들기
        {
            spriteRenderer.color = new Color(1, 1, 1, 0.5f);
            for(int i = 0; i < followers.Length; i++)
            {
                followers[i].GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.5f);
            }
        }
        else // 무적 시간 종료
        {
            spriteRenderer.color = new Color(1, 1, 1, 1);
            for (int i = 0; i < followers.Length; i++)
            {
                followers[i].GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
            }
        }
    }

    void Update()
    {
        Move(); // 이동 
        Fire(); // 공격
        Boom(); // 필살기
        Reload();
    }

    public void JoyPad(int type)
    {
        for(int i = 0; i < 9; i++)
        {
            joyControl[i] = i == type;
        }
    }

    public void JoyDown()
    {
        isControl = true;
    }

    public void JoyUp()
    {
        isControl = false;
    }



    void Move()
    {
        // 키보드 조종
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // 조이패드 조종
        if (joyControl[0]) { h = -1; v = 1; }
        if (joyControl[1]) { h = 0; v = 1; }
        if (joyControl[2]) { h = 1; v = 1; }
        if (joyControl[3]) { h = -1; v = 0; }
        if (joyControl[4]) { h = 0; v = 0; }
        if (joyControl[5]) { h = 1; v = 0; }
        if (joyControl[6]) { h = -1; v = -1; }
        if (joyControl[7]) { h = 0; v = -1; }
        if (joyControl[8]) { h = 1; v = -1; }

        if ((h == 1 && isTouchRight) || (h == -1 && isTouchLeft) || !isControl)
            h = 0;
        if ((v == 1 && isTouchTop) || (v == -1 && isTouchBottom) || !isControl)
            v = 0;

        Vector3 curPos = transform.position;
        Vector3 nextPos = new Vector3(h, v, 0) * speed * Time.deltaTime;

        transform.position = curPos + nextPos;

        anim.SetInteger("Input", (int)h);
        
    }

    public void ButtonADown()
    {
        isButtonA = true;
    }

    public void ButtonAUp()
    {
        isButtonA = false;
    }

    public void ButtonBDown()
    {
        isButtonB = true;
    }

    public void ButtonBUp()
    {
        isButtonB = false;
    }

    void Fire()
    {
        if (isButtonA) // 공격 버튼을 누를 때
        {
            if (curShotDelay >= maxShotDelay)
            {
                PlaySound("FIRE");
                switch(power)
                {
                    case 1: // power가 1일 때
                        GameObject bullet = objectManager.MakeObj("BulletPlayerA");
                        bullet.transform.position = transform.position;

                        Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();
                        rigid.AddForce(Vector2.up * 10, ForceMode2D.Impulse);
                        break;
                    case 2: // power가 2일 때
                        GameObject bulletR = objectManager.MakeObj("BulletPlayerA");
                        bulletR.transform.position = transform.position + Vector3.right * 0.1f;

                        GameObject bulletL = objectManager.MakeObj("BulletPlayerA");
                        bulletL.transform.position = transform.position + Vector3.left * 0.1f;

                        Rigidbody2D rigidR = bulletR.GetComponent<Rigidbody2D>();
                        Rigidbody2D rigidL = bulletL.GetComponent<Rigidbody2D>();
                        rigidR.AddForce(Vector2.up * 10, ForceMode2D.Impulse);
                        rigidL.AddForce(Vector2.up * 10, ForceMode2D.Impulse);
                        break;
                    default: // power가 3 이상일 때 새로운 총알을 발사함
                        GameObject bulletRR = objectManager.MakeObj("BulletPlayerA");
                        bulletRR.transform.position = transform.position + Vector3.right * 0.25f;

                        GameObject bulletCC = objectManager.MakeObj("BulletPlayerB");
                        bulletCC.transform.position = transform.position;

                        GameObject bulletLL = objectManager.MakeObj("BulletPlayerA");
                        bulletLL.transform.position = transform.position + Vector3.left * 0.25f;

                        Rigidbody2D rigidRR = bulletRR.GetComponent<Rigidbody2D>();
                        Rigidbody2D rigidCC = bulletCC.GetComponent<Rigidbody2D>();
                        Rigidbody2D rigidLL = bulletLL.GetComponent<Rigidbody2D>();
                        rigidRR.AddForce(Vector2.up * 10, ForceMode2D.Impulse);
                        rigidCC.AddForce(Vector2.up * 10, ForceMode2D.Impulse);
                        rigidLL.AddForce(Vector2.up * 10, ForceMode2D.Impulse);
                        break;
                }

                curShotDelay = 0;
            }
        }
    }

    void Reload()
    {
        curShotDelay += Time.deltaTime;
    }

    void Boom()
    {
        if(isButtonB) // B 버튼을 누를 때
        {
            if (boom == 0 || isBoomTime)
                return;

            PlaySound("BOOM");
            boom--;
            gameManager.UpdateBoomImage(boom);
            isBoomTime = true;
            
            // 이펙트 보이게 하기
            boomEffect.SetActive(true);

            Invoke("OffBoomEffect", 1f);

            // 적 모두 없애기
            GameObject[] enemiesL = objectManager.GetPool("EnemyL");
            GameObject[] enemiesM = objectManager.GetPool("EnemyM");
            GameObject[] enemiesS = objectManager.GetPool("EnemyS");
            GameObject[] enemiesB = objectManager.GetPool("EnemyB");

            for (int i = 0; i < enemiesL.Length; i++)
            {
                if (enemiesL[i].activeSelf)
                {
                    Enemy enemyLogic = enemiesL[i].GetComponent<Enemy>();
                    enemyLogic.OnHit(1000);
                }
            }
            for (int i = 0; i < enemiesM.Length; i++)
            {
                if (enemiesM[i].activeSelf)
                {
                    Enemy enemyLogic = enemiesM[i].GetComponent<Enemy>();
                    enemyLogic.OnHit(1000);
                }
            }
            for (int i = 0; i < enemiesS.Length; i++)
            {
                if (enemiesS[i].activeSelf)
                {
                    Enemy enemyLogic = enemiesS[i].GetComponent<Enemy>();
                    enemyLogic.OnHit(1000);
                }
            }
            for (int i = 0; i < enemiesB.Length; i++)
            {
                if (enemiesB[i].activeSelf)
                {
                    Enemy enemyLogic = enemiesB[i].GetComponent<Enemy>();
                    enemyLogic.OnHit(500);
                }
            }

            // 적의 총알 모두 없애기
            GameObject[] BulletEnemyA = objectManager.GetPool("BulletEnemyA");
            GameObject[] BulletEnemyB = objectManager.GetPool("BulletEnemyB");
            GameObject[] BulletBossA = objectManager.GetPool("BulletBossA");
            GameObject[] BulletBossB = objectManager.GetPool("BulletBossB");

            for (int i = 0; i < BulletEnemyA.Length; i++)
            {
                if (BulletEnemyA[i].activeSelf)
                    BulletEnemyA[i].SetActive(false);
            }
            for (int i = 0; i < BulletEnemyB.Length; i++)
            {
                if (BulletEnemyB[i].activeSelf)
                    BulletEnemyB[i].SetActive(false);
            }
            for (int i = 0; i < BulletBossA.Length; i++)
            {
                if (BulletBossA[i].activeSelf)
                    BulletBossA[i].SetActive(false);
            }
            for (int i = 0; i < BulletBossB.Length; i++)
            {
                if (BulletBossB[i].activeSelf)
                    BulletBossB[i].SetActive(false);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Border")
        {
            switch(collision.gameObject.name)
            {
                case "Top":
                    isTouchTop = true;
                    break;
                case "Bottom":
                    isTouchBottom = true;
                    break;
                case "Left":
                    isTouchLeft = true;
                    break;
                case "Right":
                    isTouchRight = true;
                    break;
            }
        }
        else if(collision.gameObject.tag == "Enemy" || collision.gameObject.tag == "EnemyBullet")
        {
            if (isRespawnTime) // 무적 시간
                return;
            if (isHit) // 중복 피격을 막기 위한 코드
                return;
            isHit = true;

            gameManager.PlaySound("DIE");
            life--;
            gameManager.UpdateLifeImage(life);
            gameManager.CallExplosion(transform.position, "P");

            if (life == 0)
                gameManager.GameOver();
            else
                gameManager.RespawnPlayer();
            
            gameObject.SetActive(false);

            if (collision.gameObject.tag == "Enemy" && collision.GetComponent<Enemy>().enemyName != "B") 
                collision.gameObject.SetActive(false);
        }
        else if(collision.gameObject.tag == "Item")
        {
            gameManager.PlaySound("ITEM");
            Item item = collision.gameObject.GetComponent<Item>();
            switch(item.type)
            {
                case "Coin":
                    score += 1000;
                    break;
                case "Power":
                    if (power == maxPower)
                        score += 500;
                    else
                    {
                        power++;
                        AddFollower();
                    }
                    break;
                case "Boom":
                    if (boom == maxBoom)
                        score += 500;
                    else
                    {                        
                        boom++;
                        gameManager.UpdateBoomImage(boom);
                    }
                    break;
            }
            collision.gameObject.SetActive(false);
        }
    }

    void AddFollower()
    {
        if (power == 4)
            followers[0].SetActive(true);
        else if (power == 5)
            followers[1].SetActive(true);
        else if (power == 6)
            followers[2].SetActive(true);
    }

    void OffBoomEffect()
    {
        boomEffect.SetActive(false);
        isBoomTime = false;
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Border")
        {
            switch (collision.gameObject.name)
            {
                case "Top":
                    isTouchTop = false;
                    break;
                case "Bottom":
                    isTouchBottom = false;
                    break;
                case "Left":
                    isTouchLeft = false;
                    break;
                case "Right":
                    isTouchRight = false;
                    break;
            }
        }
    }
}
