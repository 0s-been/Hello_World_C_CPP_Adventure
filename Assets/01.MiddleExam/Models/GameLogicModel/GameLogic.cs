using UnityEngine;
using System.Collections.Generic;
using System;


public class GameLogic : IGameLogic
{
    public event Action<HitResult> OnObjectHit;
    public event Action OnGameClear;
    public event Action OnOutOfArrows;

    //원본 배치 정보
    private StatObjectGridData _gridData;

    //스킬창 재진입 시에 이전 과정이 남아있어야 함
    //오브젝트들의 파괴 여부
    private bool[,] m_destroyedGrid;
    private int m_remainingArrows;
    private int m_remainingObjects;
    //최초 진입 시에만 초기화할 거라서 그에 대한 플래그
    private bool _isInit;

    //배치 정보는 외부에서 주입 받는 식으로 -> DI
    //gridData에 따라서 배치를 달리할 수 있음
    public GameLogic(StatObjectGridData gridData)
    {
        _gridData = gridData;
    }

    public int GetRemainingArrows() => m_remainingArrows;
    public bool IsGameClear() => m_remainingObjects <= 0;
    public bool HasArrows() => m_remainingArrows > 0;

    public void InitGame(int arrowCount)
    {
        //화살 수는 진입 시마다 갱신
        m_remainingArrows = arrowCount;

        if (_isInit) return;

        int rows = _gridData.rows;
        int cols = _gridData.cols;

        m_destroyedGrid = new bool[rows, cols];
        m_remainingObjects = 0;

        //오브젝트 수 계산 
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
                if (_gridData.GetData(r, c) != null)
                    m_remainingObjects++;

        _isInit = true;
    }

    //statobj가 자체적으로 충돌 감지 -> 자신의 정보를 토대로 HitResult 생성
    //V단에서 statobj의 이벤트들을 중계해서 P로 보냄
    //P가 수신 받고 M으로 보냄
    //M이 로직 처리 후 P에게 결과 전달
    //P가 이벤트 받고 V에게 갱신 요청
    public void ProcessHit(HitResult result)
    {
        int row = result.row;
        int col = result.col;

        //이미 파괴된 오브젝트 중복 처리 방지
        //동일 프레임에 충돌 여러 번 발생할 수도 있음
        if (m_destroyedGrid[row, col]) return;

        m_destroyedGrid[row, col] = true;
        m_remainingObjects--;
        m_remainingArrows--;

        //충돌에 대한 로직 수행 후 p에게 알림
        OnObjectHit?.Invoke(result);

        //종료 조건 체크
        if (IsGameClear()) OnGameClear?.Invoke();
        else if (!HasArrows()) OnOutOfArrows?.Invoke();
    }

    public List<StatObjectViewModel> BuildViewModel()
    {
        var list = new List<StatObjectViewModel>();

        for (int r = 0; r < _gridData.rows; r++)
        {
            for (int c = 0; c < _gridData.cols; c++)
            {
                var data = _gridData.GetData(r, c);
                if (data == null) continue; //빈 칸은 스킵

                list.Add(new StatObjectViewModel
                {
                    row         = r,
                    col         = c,
                    type        = data.statType,
                    amount      = data.amount,
                    isDestroyed = m_destroyedGrid[r, c],
                    displayname = data.displayname,
                    icon        = data.icon
                });//list new
            }//inner
        }//outer
        return list;
    }//BuildViewModel


}
