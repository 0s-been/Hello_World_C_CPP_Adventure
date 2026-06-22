using UnityEngine;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

// ──────────────────────────────────────────────
// View 레이어의 핵심 클래스
// IGameView를 구현하며 MVP의 View 역할 담당
//
// [최종 설계 결정]
// StatObject 통합 방식 (충돌 + 렌더링)
// UIManager가 StatObject 생성/관리
// ArrowController.Fire() 직접 호출 (Presenter 경유 없음)
//
// [이벤트 중계 원리]
// StatObject.OnHit 발생
//   → UIManager 내부 람다식 수신 (result 받음)
//   → OnStatObjectHit 재발행 (result 그대로 전달)
//   → Presenter가 구독 후 수신
//
// [View 내부 처리 기준]
// 데이터 상태 변화 없음 → View 내부
// (활 회전, sprite 변화, 궤적, 화살 발사)
// 데이터 상태 변화 있음 → Presenter 경유
// (충돌 결과 처리, 스탯 반영)
// ──────────────────────────────────────────────
public class UIManager : MonoBehaviour, IGameView
{
    // ── 참조 ──────────────────────────────────
    [Header("Comp")]
    [SerializeField] private ArrowController m_arrowController;
    [SerializeField] private RectTransform m_miniGamePlayerPos;
    [SerializeField] private RectTransform m_gridRoot;
    [SerializeField] private InputReader m_inputReader;
    [SerializeField] private GameObject m_miniGameRoot;

    private bool m_isOpen = false;

    [Header("Prefab")]
    [SerializeField] private GameObject m_statObjectPrefab;

    [Header("Bow")]
    [SerializeField] private Image m_bowRenderer;
    [SerializeField] private Sprite[] m_bowDrawSprites;
    [SerializeField] private float m_maxDragDistance = 3f;

    [Header("Grid Settings")]
    [SerializeField] private float m_cellSize = 1.5f;
    [SerializeField] private float m_gridHeightOffset = 2f;

    [Header("HUD")]
    [SerializeField] private TextMeshProUGUI m_arrowCountText;
    [SerializeField] private TextMeshProUGUI m_statGainText;
    [SerializeField] private TextMeshProUGUI m_statDisplayText;

    [Header("Pannel")]
    [SerializeField] private GameObject m_gameClearPanel;
    [SerializeField] private GameObject m_outOfArrowsPanel;

    public event Action OnMiniGameOpened;
    public event Action OnMiniGameClosed;
    public event Action OnDragStarted;
    public event Action<HitResult> OnStatObjectHit;

    private Dictionary<(int, int), StatObject> m_statObjectMap = new();
    private Dictionary<StatType, float> m_currentStats = new();
    private Coroutine m_statGainCoroutine;
    private Vector2 m_dragStartPos;
    private bool m_isDragging;
    private bool m_isDragAllowed;
    private bool m_isGameActive;

 

    private void Start()
    {
        m_inputReader.OnMiniGameInput += HandleMiniGameInput;
    }
    private void Update()
    {
        if (!m_isGameActive) return;

        // 충돌 감지
        CheckArrowCollisions();

        if (Input.GetMouseButtonDown(0))
            HandleDragStart();
        else if (Input.GetMouseButton(0) && m_isDragging && m_isDragAllowed)
            HandleDragging();
        else if (Input.GetMouseButtonUp(0) && m_isDragging && m_isDragAllowed)
            HandleDragEnd();
    }

    //화살 <-> StatObject 충돌 감지
    private void CheckArrowCollisions()
    {
        var arrows = m_arrowController.ActiveArrows;

        for (int i = arrows.Count - 1; i >= 0; i--)
        {
            var arrow = arrows[i];
            if (arrow == null || !arrow.gameObject.activeSelf) continue;

            foreach (var kvp in m_statObjectMap)
            {
                var statObject = kvp.Value;
                if (statObject == null || !statObject.gameObject.activeSelf) continue;

                //RectTransform 겹침 여부 계산
                if (IsOverlapping(arrow.RectTr, statObject.m_rect))
                {
                    //화살 비활성화
                    arrow.gameObject.SetActive(false);
                    arrows.RemoveAt(i);

                    //StatObject 피격 처리
                    statObject.OnArrowHit();
                    break;
                }
            }
        }
    }

    //RectTransform 겹침 판정
    private bool IsOverlapping(RectTransform a, RectTransform b)
    {
        return RectTransformUtility.RectangleContainsScreenPoint
            (b,
            RectTransformUtility.WorldToScreenPoint(null, a.position)
        );
    }

    //마우스 다운
    //선검증
    // 다운 ->OnDragStarted 발행
    //      =>Presenter가 HasArrows() 검증
    //      ->AllowDrag() 또는 ShowOutOfArrowsWarning()
    //m_isDragAllowed = false인 동안
    //HandleDragging, HandleDragEnd 실행 안 됨
    private void HandleDragStart()
    {
        m_isDragAllowed = false;
        m_dragStartPos = GetMousePos();
        m_isDragging = true;
        OnDragStarted?.Invoke();
    }

    //마우스 드래그
    // 활 회전, sprite 변화, 궤적
    // 데이터 상태 변화 없음-> View 내부에서 처리
    private void HandleDragging()
    {
        Vector2 currentPos = GetMousePos();

        // dragStart - currentPos
        // -> 드래그 반대 방향 = 화살 발사 방향
        // 아래로 당기면 위쪽 벡터 생성
        // -> 활시위를 당기는 느낌
        Vector2 dragDelta = m_dragStartPos - currentPos;

        // 활 회전 (View 내부)
        float angle = Mathf.Atan2(dragDelta.y, dragDelta.x) * Mathf.Rad2Deg;
        m_bowRenderer.transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);

        // 활 당김 정도에 따른 sprite 변화 (View 내부)
        float dragStrength = Mathf.Clamp01(dragDelta.magnitude / m_maxDragDistance);
        int spriteIndex = Mathf.FloorToInt(dragStrength * (m_bowDrawSprites.Length - 1));
        m_bowRenderer.sprite = m_bowDrawSprites[spriteIndex];

        m_arrowController.UpdateTrajectory(dragDelta.normalized);
        
    }

    //마우스 업
    private void HandleDragEnd()
    {
        m_isDragging = false;
        m_isDragAllowed = false;

        // 궤적 제거, 활 원상복구 (View 내부)
        m_arrowController.ClearTrajectory();
        m_bowRenderer.sprite = m_bowDrawSprites[0];
        m_bowRenderer.transform.rotation = Quaternion.identity;

        Vector2 dragDelta = m_dragStartPos - GetMousePos();
        if (dragDelta.magnitude < 0.1f) return;

        // [화살 발사 - View 내부에서 직접 처리]
        // 화살 발사 자체는 View의 물리적 동작
        // 게임 상태 변화는 StatObject 충돌 시 발생
        // → Presenter가 발사 시점을 알 필요 없음
        m_arrowController.Fire(dragDelta.normalized);
    }

    private void HandleMiniGameInput()
    {
        Debug.Log("HandleMiniGameInput 호출됨");
        if (m_isOpen) HandleClose();
        else HandleOpen();
    }

    private void HandleOpen()
    {     
        m_isOpen = true;
        m_miniGameRoot.SetActive(true);
        m_isGameActive = true;
        m_isDragAllowed = false;
        m_gameClearPanel.SetActive(false);
        m_outOfArrowsPanel.SetActive(false);

        //Presenter에게 알림
        //격자 초기화 및 렌더링
        OnMiniGameOpened?.Invoke();
    }

    private void HandleClose()
    {
        m_isOpen = false;
        m_isGameActive = false;
        m_miniGameRoot.SetActive(false);
        //닫힘은 단순 비활성화
        //Presenter에게 알릴 필요 없음
        //필요하다면 OnMiniGameClosed 발행 가능
    }

    //격자 렌더링
    public void RenderStatObjects(List<StatObjectViewModel> viewModels)
    {
        Debug.Log($"RenderStatObjects - ViewModel 수: {viewModels.Count}");
        Debug.Log($"m_gridRoot: {m_gridRoot}");

        foreach (var obj in m_statObjectMap.Values)
            if (obj != null) Destroy(obj.gameObject);
        m_statObjectMap.Clear();

        //격자 중앙 정렬 오프셋
        int maxCol = 0;
        int maxRow = 0;
        foreach (var vm in viewModels)
        {
            if (vm.col > maxCol) maxCol = vm.col;
            if (vm.row > maxRow) maxRow = vm.row;
        }
        float offsetX = maxCol * m_cellSize * 0.5f;
        float offsetY = maxRow * m_cellSize * 0.5f;

        Debug.Log($"offsetX:{offsetX} offsetY:{offsetY} cellSize:{m_cellSize}");

        foreach (var vm in viewModels)
        {
            if (vm.isDestroyed) continue;

            float posX = vm.col * m_cellSize - offsetX;
            float posY = vm.row * m_cellSize - offsetY + m_gridHeightOffset;

            Debug.Log($"StatObject 생성 - row:{vm.row} col:{vm.col} posX:{posX} posY:{posY}");

            var go = Instantiate(m_statObjectPrefab, m_gridRoot);
            var rectTr = go.GetComponent<RectTransform>();
            var statObject = go.GetComponent<StatObject>();

            rectTr.anchoredPosition = new Vector2(posX, posY);
            Debug.Log($"anchoredPosition 설정: {rectTr.anchoredPosition}");

            statObject.Init(vm.row, vm.col, vm);
            statObject.OnHit += (result) => OnStatObjectHit?.Invoke(result);

            m_statObjectMap[(vm.row, vm.col)] = statObject;
        }
    }

    private void OnDestroy()
    {
        if (m_inputReader != null)
            m_inputReader.OnMiniGameInput -= HandleMiniGameInput;
    }

    //StatObject 파괴 연출
    public void DestroyStatObjectView(int row, int col)
    {
        var key = (row, col);

        if (!m_statObjectMap.TryGetValue(key, out StatObject statObject))
        {
            Debug.LogWarning($"StatObject ({row},{col})를 찾을 수 없음");
            return;
        }

        statObject.PlayDestroyEffect();
        m_statObjectMap.Remove(key);
    }

    public void UpdateArrowCount(int remaining)
    {
        m_arrowCountText.text = $"Arrow : {remaining}";
    }

    public void UpdateStatDisplay(StatType type, float amount)
    {
        m_currentStats[type] = amount;

        // string + 연산 → 매번 새 객체 → GC 부담
        // StringBuilder → 버퍼에 누적 → GC 부담 적음
        var sb = new System.Text.StringBuilder();
        foreach (var pair in m_currentStats)
            sb.AppendLine($"{pair.Key}: {pair.Value}");
        m_statDisplayText.text = sb.ToString();
    }

    public void ShowStatGainEffect(StatType type, float amount)
    {
        m_statGainText.text = $"{type} +{amount}";

        // 연속 획득 시 타이머 초기화
        if (m_statGainCoroutine != null) StopCoroutine(m_statGainCoroutine);
        m_statGainCoroutine = StartCoroutine(HideStatGainText());
    }

    private IEnumerator HideStatGainText()
    {
        yield return new WaitForSeconds(2f);
        m_statGainText.text = "";
    }

    public void ShowGameClearUI() => m_gameClearPanel.SetActive(true);
    public void ShowOutOfArrowsUI() => m_outOfArrowsPanel.SetActive(true);

    public void AllowDrag() => m_isDragAllowed = true;

    public void ShowOutOfArrowsWarning()
    {
        m_isDragging = false;
        m_isDragAllowed = false;
        Debug.Log("화살이 없습니다!");
    }

    private Vector2 GetMousePos()
    {
        return Input.mousePosition;
    }

   
}
