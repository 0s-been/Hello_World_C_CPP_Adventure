using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

public class Arrow : MonoBehaviour
{
    private Vector2 m_dir;
    private Image m_image;
    private float m_speed;
    private RectTransform m_rect;
    private Action<Arrow> m_OnOutOfBounds;

    //UIManager가 충돌 감지용으로 참조
    public RectTransform RectTr => m_rect;

    private void Awake()
    {
        m_rect = GetComponent<RectTransform>();
        m_image = GetComponent<Image>();
    }
    public void Init(Vector2 dir, float speed, Sprite sprite, Action<Arrow> OnOutOfBounds)
    {
        m_dir = dir.normalized;
        m_speed = speed;
        m_OnOutOfBounds = OnOutOfBounds;

        if (sprite != null)
            m_image.sprite = sprite;

        //발사 방향으로 화살 회전
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        //스프라이트 기본 방향 보정
        transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
    }

    // Update is called once per frame
    void Update()
    {
        m_rect.anchoredPosition += m_dir * m_speed * Time.deltaTime;

        if (IsOutOfBounds()) m_OnOutOfBounds?.Invoke(this);
    }

    //월드 좌표 -> 뷰포트 좌표로 변환 후 범위 체크
    //뷰포트 내부는 (0~1, 0~1) 그 이외는 범위 밖
    private bool IsOutOfBounds()
    {
        Vector2 pos = m_rect.anchoredPosition;
        Vector2 screenHalf = new Vector2(Screen.width, Screen.height);

        return pos.x < -screenHalf.x || pos.x > screenHalf.x ||
               pos.y < -screenHalf.y || pos.y > screenHalf.y;
    }
}
