using UnityEngine;
using System.Collections.Generic;
using System;
using Unity.VisualScripting;
public interface IGameView
{
    //V -> P 입력 이벤트
    event Action OnMiniGameOpened;
    event Action OnMiniGameClosed;
    event Action OnDragStarted;
    event Action<HitResult> OnStatObjectHit;

    // P -> V 렌더링 요청
    void RenderStatObjects(List<StatObjectViewModel> vm);
    void DestroyStatObjectView(int row, int col);
    void UpdateArrowCount(int remaining);
    void UpdateStatDisplay(StatType type, float amount);
    void ShowStatGainEffect(StatType type, float amount);
    void ShowGameClearUI();
    void ShowOutOfArrowsUI();
    void ShowOutOfArrowsWarning();
    void AllowDrag();
}
