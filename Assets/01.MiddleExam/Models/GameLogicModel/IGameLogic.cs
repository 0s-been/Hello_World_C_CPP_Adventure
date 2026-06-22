using UnityEngine;
using System.Collections.Generic;
using System;


//스킬창 ui의 미니게임 로직 인터페이스
//Stat과 분리한 이유는 SRP원칙을 준수하기 위함
public interface IGameLogic
{
    void InitGame(int arrowCount);
    void ProcessHit(HitResult result);
    bool HasArrows();
    bool IsGameClear();
    int GetRemainingArrows();
    List<StatObjectViewModel> BuildViewModel();

    event Action<HitResult> OnObjectHit;
    event Action OnGameClear;
    event Action OnOutOfArrows;
}
