using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class ArrowController : MonoBehaviour
{
    [SerializeField] private GameObject m_arrowPrefab;
    [SerializeField] private GameObject m_trajectoryDotPrefab; //점 프리팹
    [SerializeField] private Transform m_trajectoryParent;    //점들의 부모
    [SerializeField] private float m_arrowSpeed = 10f;
    [SerializeField] private int m_trajectoryPointCount = 20;
    [SerializeField] private RectTransform m_canvasRoot;

    private RectTransform m_rect;
    private List<GameObject> m_trajectoryDots = new();

    //활성화된 화살 목록 (UIManager가 충돌 감지용으로 참조)
    public List<Arrow> ActiveArrows { get; private set; } = new();
    [SerializeField] private Sprite m_arrowSprite;

    private void Awake()
    {
        m_rect = GetComponent<RectTransform>();
    }

    public void Fire(Vector2 direction)
    {
        var go = Instantiate(m_arrowPrefab, m_canvasRoot);
        var arrow = go.GetComponent<Arrow>();
        var rectTr = go.GetComponent<RectTransform>();

        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, m_rect.position);
        Debug.Log($"ArrowController 월드 위치: {m_rect.position}");
        Debug.Log($"스크린 좌표: {screenPos}");

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            m_canvasRoot,
            screenPos,
            null,
            out Vector2 localPos
        );
        Debug.Log($"Canvas 로컬 좌표: {localPos}");

        rectTr.anchoredPosition = localPos;
        Debug.Log($"Arrow anchoredPosition 설정 후: {rectTr.anchoredPosition}");

        arrow.Init(direction, m_arrowSpeed, m_arrowSprite, (a) =>
        {
            ActiveArrows.Remove(a);
            a.gameObject.SetActive(false);
        });

        ActiveArrows.Add(arrow);
    }

    //궤적 업데이트
    public void UpdateTrajectory(Vector2 dir)
    {
        Vector2 start = m_rect.anchoredPosition;

        // 점 개수 맞추기
        while (m_trajectoryDots.Count < m_trajectoryPointCount)
        {
            var dot = Instantiate(m_trajectoryDotPrefab, m_trajectoryParent);
            m_trajectoryDots.Add(dot);
        }

        for (int i = 0; i < m_trajectoryPointCount; i++)
        {
            float t = i * 30f; // 간격
            Vector2 pos = start + dir.normalized * m_arrowSpeed * t * 0.001f;

            var rectTr = m_trajectoryDots[i].GetComponent<RectTransform>();
            rectTr.anchoredPosition = pos;
            m_trajectoryDots[i].SetActive(true);
        }
    }

    //궤적 제거
    public void ClearTrajectory()
    {
        foreach (var dot in m_trajectoryDots)
            dot.SetActive(false);
    }

}
