using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace OnionArc.View
{
    /// <summary>
    /// 일시정지 메뉴 View 구현체.
    /// 검은 반투명 레이어 + 재개/종료 버튼을 표시하고, 버튼 클릭을 이벤트로 흘린다.
    /// 순수 View — 게임을 멈추는 것(timeScale, InputBlocker)은 Presenter가 한다.
    /// </summary>
    public class PauseMenuView : MonoBehaviour, IPauseMenuView
    {
        [SerializeField] private GameObject _panel;     // 검은 레이어 + 버튼 묶음 (켜고 끔)
        [SerializeField] private Button _resumeButton;  // 재개
        [SerializeField] private Button _exitButton;    // 종료

        private readonly Subject<Unit> _onResume = new Subject<Unit>();
        private readonly Subject<Unit> _onExit   = new Subject<Unit>();

        public IObservable<Unit> OnResumeClicked => _onResume;
        public IObservable<Unit> OnExitClicked   => _onExit;

        private void Awake()
        {
            if (_panel != null) _panel.SetActive(false);

            if (_resumeButton != null)
                _resumeButton.onClick.AddListener(() => _onResume.OnNext(Unit.Default));

            if (_exitButton != null)
                _exitButton.onClick.AddListener(() => _onExit.OnNext(Unit.Default));
        }

        public void Show()
        {
            if (_panel != null) _panel.SetActive(true);
        }

        public void Hide()
        {
            if (_panel != null) _panel.SetActive(false);
        }
    }
}
