using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public string enemyName;
    public float speed;
    public int health;
    public int bossHealth;
    public int enemyScore;

    public float maxShotDelay;
    public float curShotDelay;

    public bool isDead;

    public Sprite[] sprites;

    public GameObject bulletObjA;
    public GameObject bulletObjB;
    public GameObject itemCoin;
    public GameObject itemPower;
    public GameObject itemBoom;
    public GameObject player;
    public GameManager gameManager;
    public ObjectManager objectManager;

    SpriteRenderer spriteRenderer;
    Animator bossAnim;

    public int patternIndex;
    public int curPatternCount;
    public int[] maxPatternCount;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (enemyName == "B")
            bossAnim = GetComponent<Animator>();
    }

    void OnEnable()
    {
        isDead = false;
        switch(enemyName)
        {
            case "B":
                bossHealth += 500;
                health = bossHealth;
                Invoke("Stop", 1.5f);
                break;
            case "L":
                health = 40;
                break;
            case "M":
                health = 15;
                break;
            case "S":
                health = 3;
                break;
        } 
    }

    void Stop()
    {
        if (!gameObject.activeSelf)
            return;
        Rigidbody2D rigid = GetComponent<Rigidbody2D>();
        rigid.velocity = Vector2.zero;
        gameManager.PlaySound("BOSS");

        Invoke("Think", 2);
    }

    void Think()
    {
        patternIndex = patternIndex == 3 ? 0 : patternIndex + 1; // index를 넘으면 0으로 초기화
        curPatternCount = 0;
        
        switch(patternIndex)
        {
            case 0:
                FireForward();
                break;
            case 1:
                FireShot();
                break;
            case 2:
                FireArc();
                break;
            case 3:
                FireAround();
                break;
        }
    }

    void FireForward() // 앞으로 6발 발사
    {
        if (health <= 0)
            return;
        // 공격
        GameObject bulletCL = objectManager.MakeObj("BulletBossA");
        bulletCL.transform.position = transform.position + Vector3.left * 0.2f;
        GameObject bulletCR = objectManager.MakeObj("BulletBossA");
        bulletCR.transform.position = transform.position + Vector3.right * 0.2f;

        GameObject bulletL = objectManager.MakeObj("BulletBossA");
        bulletL.transform.position = transform.position + Vector3.left * 0.65f;
        GameObject bulletLL = objectManager.MakeObj("BulletBossA");
        bulletLL.transform.position = transform.position + Vector3.left * 0.8f;

        GameObject bulletR = objectManager.MakeObj("BulletBossA");
        bulletR.transform.position = transform.position + Vector3.right * 0.65f;
        GameObject bulletRR = objectManager.MakeObj("BulletBossA");
        bulletRR.transform.position = transform.position + Vector3.right * 0.8f;

        Rigidbody2D rigidCL = bulletCL.GetComponent<Rigidbody2D>();
        Rigidbody2D rigidCR = bulletCR.GetComponent<Rigidbody2D>();
        Rigidbody2D rigidL = bulletL.GetComponent<Rigidbody2D>();
        Rigidbody2D rigidR = bulletR.GetComponent<Rigidbody2D>();
        Rigidbody2D rigidLL = bulletLL.GetComponent<Rigidbody2D>();
        Rigidbody2D rigidRR = bulletRR.GetComponent<Rigidbody2D>();

        rigidCL.AddForce(Vector2.down * 8, ForceMode2D.Impulse);
        rigidCR.AddForce(Vector2.down * 8, ForceMode2D.Impulse);
        rigidL.AddForce(Vector2.down * 8, ForceMode2D.Impulse);
        rigidR.AddForce(Vector2.down * 8, ForceMode2D.Impulse);
        rigidLL.AddForce(Vector2.down * 8, ForceMode2D.Impulse);
        rigidRR.AddForce(Vector2.down * 8, ForceMode2D.Impulse);

        // 각 패턴 별로 횟수를 실행하고 다음 패턴으로 넘어가도록 구현
        curPatternCount++;
        if (curPatternCount < maxPatternCount[patternIndex])
            Invoke("FireForward", 2);
        else
            Invoke("Think", 3);
    }

    void FireShot() // 플레이어 방향으로 샷건
    {
        if (health <= 0)
            return;

        for (int i = 0; i < 5; i++)
        {
            GameObject bullet = objectManager.MakeObj("BulletEnemyB");
            bullet.transform.position = transform.position;

            Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();

            Vector2 dirVec = player.transform.position - transform.position;
            Vector2 ranVec = new Vector2(Random.Range(-1f, 1f), 0);
            dirVec += ranVec;
            rigid.AddForce(dirVec.normalized * 5, ForceMode2D.Impulse); 
        }

        curPatternCount++;
        if (curPatternCount < maxPatternCount[patternIndex])
            Invoke("FireShot", 3.5f);
        else
            Invoke("Think", 3);
    }

    void FireArc() // 부채모양으로 발사
    {
        if (health <= 0)
            return;

        GameObject bullet = objectManager.MakeObj("BulletEnemyA");
        bullet.transform.position = transform.position;
        bullet.transform.rotation = Quaternion.identity;

        Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();
        // 원의 파형을 그리며 발사하게 됨 
        Vector2 dirVec = new Vector2(Mathf.Cos(Mathf.PI * 10 * curPatternCount / maxPatternCount[patternIndex]), -1);
        rigid.AddForce(dirVec.normalized * 5, ForceMode2D.Impulse);

        curPatternCount++;
        if (curPatternCount < maxPatternCount[patternIndex])
            Invoke("FireArc", 0.15f);
        else
            Invoke("Think", 3);
    }

    void FireAround() // 원 형태로 전체 공격
    {
        if (health <= 0)
            return;

        int roundNumA = 50;
        int roundNumB = 40;
        int roundNum = curPatternCount % 2 == 0 ? roundNumA : roundNumB;

        for(int i = 0; i < roundNum; i++)
        {
            GameObject bullet = objectManager.MakeObj("BulletBossB");
            bullet.transform.position = transform.position;
            bullet.transform.rotation = Quaternion.identity;

            Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();
            // 원의 둘레를 그리며 발사하게 됨 
            Vector2 dirVec = new Vector2(Mathf.Cos(Mathf.PI * 2 * i / roundNum),
                                         Mathf.Sin(Mathf.PI * 2 * i / roundNum));
            rigid.AddForce(dirVec.normalized * 2, ForceMode2D.Impulse);

            // 가는 방향으로 회전되어 돌아가도록 함
            Vector3 rotVec = Vector3.forward * 360 * i / roundNum + Vector3.forward * 90;
            bullet.transform.Rotate(rotVec);
        }

        curPatternCount++;
        if (curPatternCount < maxPatternCount[patternIndex])
            Invoke("FireAround", 0.7f);
        else
            Invoke("Think", 5);
    }

    void Update()
    {
        if (enemyName == "B")
            return;
        Fire(); // 공격
        Reload();
    }

    void Fire()
    {
        if (curShotDelay >= maxShotDelay)
        {
            switch(enemyName)
            {
                case "S":
                    GameObject bullet = objectManager.MakeObj("BulletEnemyA");
                    bullet.transform.position = transform.position;

                    Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();

                    Vector3 dirVec = player.transform.position - transform.position;
                    rigid.AddForce(dirVec.normalized * 3, ForceMode2D.Impulse); // normalized : 방향은 유지되고 크기는 1인 단위벡터를 지님
                    break;
                case "L":
                    GameObject bulletL = objectManager.MakeObj("BulletEnemyB");
                    bulletL.transform.position = transform.position + Vector3.left * 0.3f;
                    
                    GameObject bulletR = objectManager.MakeObj("BulletEnemyB");
                    bulletR.transform.position = transform.position + Vector3.right * 0.3f;

                    Rigidbody2D rigidL = bulletL.GetComponent<Rigidbody2D>();
                    Rigidbody2D rigidR = bulletR.GetComponent<Rigidbody2D>();

                    Vector3 dirVecL = player.transform.position - transform.position;
                    Vector3 dirVecR = player.transform.position - transform.position;
                    rigidL.AddForce(dirVecL.normalized * 4, ForceMode2D.Impulse);
                    rigidR.AddForce(dirVecR.normalized * 4, ForceMode2D.Impulse);
                    break;
            }

            curShotDelay = 0;
        }
    }

    void Reload()
    {
        curShotDelay += Time.deltaTime;
    }

    public void OnHit(int dmg)
    {
        health -= dmg;

        if (enemyName == "B")
            bossAnim.SetTrigger("OnHit");
        else
        { 
            spriteRenderer.sprite = sprites[1]; // 평소 스프라이트는 0, 피격 시 1로 변하고 다시 돌아옴
            Invoke("ReturnSprite", 0.1f);
        }   

        if (health <= 0 && !isDead)
        {
            gameManager.PlaySound("DIE");
            isDead = true;
            Player playerLogic = player.GetComponent<Player>();
            playerLogic.score += enemyScore;

            // 랜덤으로 아이템 드랍
            int ran = enemyName == "B" ? 10 : Random.Range(0, 10);
            if (ran < 3) // 30% 확률로 Coin 드랍
            {
                GameObject itemCoin = objectManager.MakeObj("ItemCoin");
                itemCoin.transform.position = transform.position;
            }
            else if (ran < 5) // 20% 확률로 Power 드랍
            {
                GameObject itemPower = objectManager.MakeObj("ItemPower");
                itemPower.transform.position = transform.position;
            }
            else if (ran < 6) // 10% 확률로 Boom 드랍
            {
                GameObject itemBoom = objectManager.MakeObj("ItemBoom");
                itemBoom.transform.position = transform.position;
            }
            // 40% 확률로 아이템 드랍 X

            gameObject.SetActive(false);
            CancelInvoke("Think");
            transform.rotation = Quaternion.identity; // 기본 회전값을 0으로 함
            gameManager.CallExplosion(transform.position, enemyName);

            // 보스를 잡았을 때 (스테이지 클리어)
            if(enemyName == "B")
                gameManager.StageEnd();
        }
    }

    void ReturnSprite()
    {
        spriteRenderer.sprite = sprites[0];
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "EnemyBorder" && enemyName != "B")
        {
            gameObject.SetActive(false);
            transform.rotation = Quaternion.identity;
        }
        else if (collision.gameObject.tag == "PlayerBullet")
        {
            Bullet bullet = collision.gameObject.GetComponent<Bullet>();
            OnHit(bullet.dmg);
            collision.gameObject.SetActive(false);
        }
    }
}
