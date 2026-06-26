using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace OnionArc.View
{
    /// <summary>
    /// 확인메뉴 ui
    /// "메시지 + 확인/취소" 범용 view
    /// 삭제, 종료, 초기화 등 어디서나 재사용 가능
    /// 데이터를 직접 바꾸지 않고 사용자의 결정을 presenter로 전달만 함
    /// </summary>
    public class ConfirmDialogView : MonoBehaviour, IConfirmDialogView
    {
        /// <summary> 확인메뉴 ui 다이얼로그 전체 </summary>
        [SerializeField] private GameObject _panel;
        /// <summary> 메뉴에 띄울 메인 메시지 텍스트 </summary>
        [SerializeField] private TextMeshProUGUI _messageText;
        /// <summary> 확인버튼에 띄울 메시지 텍스트 </summary>
        [SerializeField] private Button _confirmButton;
        /// <summary> 취소버튼에 띄울 메시지 텍스트 </summary>
        [SerializeField] private Button _cancelButton;

        private readonly Subject<Unit> _onConfirmed = new Subject<Unit>();
        private readonly Subject<Unit> _onCancelled = new Subject<Unit>();

        public IObservable<Unit> OnConfirmed => _onConfirmed;
        public IObservable<Unit> OnCancelled => _onCancelled;

        private void Awake()
        {
            if (_panel != null) _panel.SetActive(false);

            // 버튼 클릭 -> 결과 발행 및 창 닫기
            if (_confirmButton != null)
                _confirmButton.onClick.AddListener(() =>
                {
                    Hide();
                    _onConfirmed.OnNext(Unit.Default);
                });

            if (_cancelButton != null)
                _cancelButton.onClick.AddListener(() =>
                {
                    Hide();
                    _onCancelled.OnNext(Unit.Default);
                });
        }

        /// <summary> 지정한 메시지로 확인창을 띄움 </summary>
        public void Show(string message)
        {
            if (_messageText != null) _messageText.text = message;
            if (_panel != null) _panel.SetActive(true);
        }

        /// <summary>창을 닫음 </summary>
        public void Hide()
        {
            if (_panel != null) _panel.SetActive(false);
        }
    }
}
