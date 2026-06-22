using UnityEngine;
using System;
using System.Collections.Generic;

// ──────────────────────────────────────────────
// Presenter 레이어의 핵심 클래스
// View와 Model 사이를 중계
//
// [최종 설계 결정]
// _arrowController 제거
// → 화살 발사는 UIManager 내부에서 직접 처리
// → Presenter가 발사 시점을 알 필요 없음
//
// OnArrowFired 제거
// → 같은 이유
//
// [Presenter가 아는 것]
// IGameView      → View 계약 (구현체 모름)
// IPlayerStat    → 스탯 계약 (구현체 모름)
// ISkillTreeGame → 게임 상태 계약 (구현체 모름)
// → 전부 인터페이스 → DIP 준수
// ──────────────────────────────────────────────
public class Presenter : MonoBehaviour
{
    [SerializeField] private StatObjectGridData m_gridData;

    // ── 인터페이스 참조 (DIP) ─────────────────
    private IGameView m_gameView;
    private IStatSystem m_playerStat;
    private IGameLogic m_skillTreeGame;

    // ── 초기화 ────────────────────────────────
    private void Awake()
    {
        m_gameView = GetComponent<IGameView>();

        // 순수 C# 클래스 → new로 생성
        // SetActive, 씬 전환에 영향 없음
        // → 미니게임 재진입 시 상태 유지
        m_playerStat = new StatComponent(GetInitialStats(), 10);
        m_skillTreeGame = new GameLogic(m_gridData);
    }

    private void Start()
    {
        // ── View → Presenter 바인딩 ───────────
        // [왜 함수명으로 바인딩하는가?]
        // 람다식: 매번 새 객체 생성 → -= 해제 불가
        // 함수명: 동일 참조 → -= 정상 해제 가능
        m_gameView.OnMiniGameOpened += HandleGameOpened;
        m_gameView.OnDragStarted += HandleDragStarted;
        m_gameView.OnStatObjectHit += HandleStatObjectHit;

        // ── Model → Presenter 바인딩 ──────────
        m_skillTreeGame.OnObjectHit += HandleObjectHit;
        m_skillTreeGame.OnGameClear += HandleGameClear;
        m_skillTreeGame.OnOutOfArrows += HandleOutOfArrows;
        m_playerStat.OnStatChanged += HandleStatChanged;
    }

    // ── 메모리 누수 방지 ──────────────────────
    // 이벤트 해제 없이 오브젝트 파괴 시
    // → GC가 Presenter 수거 못함 (메모리 누수)
    // → 파괴된 오브젝트 함수 호출 버그 가능
    private void OnDestroy()
    {
        m_gameView.OnMiniGameOpened -= HandleGameOpened;
        m_gameView.OnDragStarted -= HandleDragStarted;
        m_gameView.OnStatObjectHit -= HandleStatObjectHit;

        m_skillTreeGame.OnObjectHit -= HandleObjectHit;
        m_skillTreeGame.OnGameClear -= HandleGameClear;
        m_skillTreeGame.OnOutOfArrows -= HandleOutOfArrows;
        m_playerStat.OnStatChanged -= HandleStatChanged;
    }

    // ── View → Presenter 핸들러 ───────────────

    // 미니게임 진입
    // Model이 배치 정보를 DTO로 조립
    // View는 DTO대로 렌더링만 담당
    private void HandleGameOpened()
    {
        int arrows = m_playerStat.GetSkillPoint();
        m_skillTreeGame.InitGame(arrows);

        var viewModels = m_skillTreeGame.BuildViewModel();
        Debug.Log($"ViewModel 개수: {viewModels.Count}"); // 몇 개인지 확인
        foreach (var vm in viewModels)
            Debug.Log($"vm - row:{vm.row} col:{vm.col} statType:{vm.type} isDestroyed:{vm.isDestroyed}");
        m_gameView.RenderStatObjects(viewModels);
        m_gameView.UpdateArrowCount(arrows);
    }

    // 마우스 다운 선검증
    // 화살 없는데 드래그 허용하면
    // → 불필요한 드래그 연산 + UX 혼란
    // 다운 시점에 검증 → 드래그 자체 차단
    private void HandleDragStarted()
    {
        if (m_skillTreeGame.HasArrows())
            m_gameView.AllowDrag();
        else
            m_gameView.ShowOutOfArrowsWarning();
    }

    // StatObject 충돌 수신
    // [흐름]
    // StatObject.OnHit
    //   → UIManager 람다식 수신
    //   → OnStatObjectHit 재발행
    //   → 여기서 수신
    //   → SkillTreeGame.ProcessHit()
    //   → OnObjectHit 발행
    //   → HandleObjectHit에서 수신
    private void HandleStatObjectHit(HitResult result)
    {
        m_skillTreeGame.ProcessHit(result);
    }

    // ── Model → Presenter 핸들러 ──────────────

    // 충돌 결과 처리
    private void HandleObjectHit(HitResult result)
    {
        // 스킬포인트 소모
        m_playerStat.SpendSkillPoint(1);

        // 스탯 반영
        // → OnStatChanged 발행
        // → HandleStatChanged에서 View HUD 갱신
        m_playerStat.ApplyStat(result.type, result.amount);

        // View 갱신 명령
        m_gameView.DestroyStatObjectView(result.row, result.col);
        m_gameView.ShowStatGainEffect(result.type, result.amount);
        m_gameView.UpdateArrowCount(m_skillTreeGame.GetRemainingArrows());
    }

    private void HandleGameClear() => m_gameView.ShowGameClearUI();
    private void HandleOutOfArrows() => m_gameView.ShowOutOfArrowsUI();

    // 스탯 변경 → View HUD 갱신
    private void HandleStatChanged(StatType type, float newValue)
    {
        m_gameView.UpdateStatDisplay(type, newValue);
    }

    // ── 유틸리티 ──────────────────────────────
    private Dictionary<StatType, float> GetInitialStats()
    {
        return new Dictionary<StatType, float>
        {
            { StatType.Hp,           200f  },
            { StatType.MP,           100f  },
            { StatType.Power,         10f  },
            { StatType.Defense,        5f  },
            { StatType.MoveSpeed,      5f  },
            { StatType.CriticalRate, 0.1f  }
        };
    }
}
