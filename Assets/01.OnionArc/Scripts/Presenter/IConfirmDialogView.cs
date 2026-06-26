using System;
using UniRx;

namespace OnionArc.View
{
    /// <summary>
    /// 공용 확인 다이얼로그 ui view interface 
    /// 지금은 확인/ 취소지만 다양한 다이얼로그 ui로 커스텀 가능
    /// </summary>
    public interface IConfirmDialogView
    {
        /// <summary>확인 버튼 클릭됨</summary>
        IObservable<Unit> OnConfirmed { get; }

        /// <summary>취소 버튼 클릭됨</summary>
        IObservable<Unit> OnCancelled { get; }

        /// <summary>지정한 메시지로 확인창을 띄움</summary>
        void Show(string message);

        /// <summary>확인창을 숨김</summary>
        void Hide();
    }
}
