using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackGround : MonoBehaviour
{
    public float speed;
    public int startIndex;
    public int endIndex;
    public Transform[] sprites;

    float viewHeight;

    void Awake()
    {   // 카메라 사이즈의 실제 높이 구하기
        viewHeight = Camera.main.orthographicSize * 2; 
    }

    void Update()
    {
        Move();
        Scrolling();
    }

    void Move() // 밑으로 조금씩 움직임
    {
        Vector3 curPos = transform.position;
        Vector3 nextPos = Vector3.down * speed * Time.deltaTime;
        transform.position = curPos + nextPos;
    }

    void Scrolling() // 아래 것을 위로 올리기
    {
        if (sprites[endIndex].position.y < viewHeight * (-1))
        {
            // 스프라이트 재사용 (맨 밑에 있는 걸 카메라에서 벗어나면 맨 위로 올림)
            Vector3 backSpritePos = sprites[startIndex].localPosition; // localPosition은 부모의 position을 기준으로 잡은 position
            sprites[endIndex].transform.localPosition = backSpritePos + Vector3.up * viewHeight;

            // 인덱스 초기화
            int startIndexSave = startIndex; // 중간에 있는 스프라이트의 인덱스
            startIndex = endIndex; // 맨 위로 간 스프라이트를 start로 
            endIndex = (startIndexSave - 1 == -1) ? sprites.Length - 1 : startIndexSave - 1; // 끝에 도달하면 다시 처음으로 인덱스 바꾸기
        }
    }
}
