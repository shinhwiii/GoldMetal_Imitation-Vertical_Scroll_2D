using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

public class Follower : MonoBehaviour
{
    public float maxShotDelay;
    public float curShotDelay;

    public Vector3 followPos;
    public int followDelay;
    public Transform parent;
    public Queue<Vector3> parentPos;

    public ObjectManager objectManager;

    void Awake()
    {
        parentPos = new Queue<Vector3>();
    }

    void Update()
    {
        Watch();
        Follow();
        Fire();
        Reload();
    }

    void Watch()
    {
        // Queue = 선입선출 방식임
        if(!parentPos.Contains(parent.position)) // 같은 위치일 때 들어가지 않도록 함 
            parentPos.Enqueue(parent.position);

        if (parentPos.Count > followDelay) // 딜레이를 줌
            followPos = parentPos.Dequeue();
        else if (parentPos.Count < followDelay)
            followPos = parent.position;
    }

    void Follow()
    {
        transform.position = followPos;
    }

    void Fire()
    {
        if (Input.GetButton("Fire1")) // 좌클릭
        {
            if (curShotDelay >= maxShotDelay)
            {
                GameObject bullet = objectManager.MakeObj("BulletFollower");
                bullet.transform.position = transform.position;

                Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();
                rigid.AddForce(Vector2.up * 10, ForceMode2D.Impulse);

                curShotDelay = 0;
            }
        }
    }

    void Reload()
    {
        curShotDelay += Time.deltaTime;
    }
    
}
