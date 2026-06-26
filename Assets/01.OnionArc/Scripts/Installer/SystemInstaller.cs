using UnityEngine;
using Zenject;
using OnionArc.View;
using OnionArc.Presenter;

namespace OnionArc.Installer
{
    /// <summary>
    /// 시스템 수준 기능을 담는 Installer
    /// 현재는 일시정지 메뉴
    /// 인벤토리/장착과 독립적이라 별도 Installer로 분리
    /// SceneContext의 Mono Installers 목록에 InventoryInstaller와 함께 등록하면
    /// Zenject가 두 Installer의 바인딩을 하나의 Container로 합쳐줌
    /// </summary>
    public class SystemInstaller : MonoInstaller
    {
        [Header("Pause")]
        [SerializeField] private PauseMenuView _pauseMenuView;
        [SerializeField] private PausePresenter _pausePresenter;

        public override void InstallBindings()
        {
            // 일시정지 메뉴
            Container.Bind<IPauseMenuView>()
                .FromInstance(_pauseMenuView)
                .AsSingle();

            Container.Bind<PausePresenter>()
                .FromInstance(_pausePresenter)
                .AsSingle()
                .NonLazy();
        }
    }
}
