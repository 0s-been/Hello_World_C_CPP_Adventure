using UniRx;
using UnityEngine;
using Zenject;
using OnionArc.View;

namespace OnionArc.Presenter
{

    /// <summary>
    /// 일시정지 흐름 제어 presenter
    ///
    /// ESC 감지 -> 일시정지/재개 결정 -> View 표시와 게임 정지를 지시
    /// Time.timeScale = 0  ->  시간 기반 동작(적·애니메이션) 정지
    /// InputBlocker.Push() -> 게임 입력 차단 (기존 메커니즘 재사용, 커서도 자동 해제)
    /// View.Show()         -> 검은 레이어 + 버튼 표시
    /// 재개는 셋을 되돌림
    ///
    /// timeScale=0이어도 ESC 감지는 Update에서 계속 동작함
    /// (Input은 timeScale 영향 안 받음)그래서 일시정지 중에도 ESC로 재개 가능
    /// </summary>
    public class PausePresenter : MonoBehaviour
    {
        private IPauseMenuView _view;
        private bool _isPaused;

        [Inject]
        public void Construct(IPauseMenuView view)
        {
            _view = view;
        }

        private void Start()
        {
            _view.OnResumeClicked
                .Subscribe(_ => Resume())
                .AddTo(this);

            _view.OnExitClicked
                .Subscribe(_ => Exit())
                .AddTo(this);
        }

        private void Update()
        {
            // 일시정지 토글 (timeScale 영향 안 받는 입력이라 정지 중에도 동작)
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (_isPaused) Resume();
                else Pause();
            }
        }

        private void Pause()
        {
            _isPaused = true;
            Time.timeScale = 0f;        // 게임 시간 정지
            InputBlocker.Push();        // 게임 입력 차단 (커서 자동 해제)
            _view.Show();               // 메뉴 표시
        }

        private void Resume()
        {
            _isPaused = false;
            Time.timeScale = 1f;        // 시간 복구
            InputBlocker.Pop();         // 입력 차단 해제
            _view.Hide();               // 메뉴 숨김
        }

        private void Exit()
        {
            // 종료 전 timeScale
            // 복구 안 하면 다음 실행/씬에 0이 남을 수 있음
            Time.timeScale = 1f;

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;  // 에디터 -> 플레이 중지
#else
            Application.Quit();                               // 빌드   -> 게임 종료
#endif
        }

        private void OnDestroy()
        {
            // 일시정지 중 파괴되면 timeScale이 0으로 남는 사고 방지
            if (_isPaused)
            {
                Time.timeScale = 1f;
                InputBlocker.Pop();
            }
        }
    }
}
