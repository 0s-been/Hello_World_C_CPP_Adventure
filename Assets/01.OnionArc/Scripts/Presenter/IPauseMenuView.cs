using System;
using UniRx;

namespace OnionArc.View
{
    /// <summary>
    /// 일시정지 메뉴 view 
    /// </summary>
    public interface IPauseMenuView
    {
        /// <summary>재개 버튼 클릭됨</summary>
        IObservable<Unit> OnResumeClicked { get; }
        /// <summary>종료 버튼 클릭됨</summary>
        IObservable<Unit> OnExitClicked { get; }
        /// <summary>일시정지 메뉴를 표시</summary>
        void Show();
        /// <summary>일시정지 메뉴를 숨김</summary>
        void Hide();
    }
}
