using UnityEngine;
using Zenject;
using InventorySDK.Domain;
using InventorySDK.Application;
using InventorySDK.Infrastructure;
using EquipmentSDK.Domain;
using EquipmentSDK.Application;
using OnionArc.Model;
using OnionArc.View;
using OnionArc.Presenter;


// 고민해볼 점
// 인벤토리와 장착에 관한 걸 하나의 인스톨러로 해도 되나?
// 이 인스톨러도 특정 기능에 대한 인스톨러로 분리해야 하나?
// 아니면 어차피 프로그램 실행 전 설정에 대한 부분이니까
// 인스톨러 한 곳에서 필요한 모든 작업을 한 곳에 뭉쳐서 하는게 낫나? 
// 바인딩이 여러 군데로 퍼지면 나중에 힘들어질텐데
namespace OnionArc.Installer
{
    /// <summary>
    /// 인벤토리 및 장착 관련 모든 연결이 모이는 곳
    /// zenject를 통해 DI를 받음
    /// </summary>
    public class InventoryInstaller : MonoInstaller
    {
        // DI 교체 선택지, 사실 이 부분도 별도로 빼야할듯
        public enum ItemSource { Csv, Json }

        [Header("Grid Size")]
        [SerializeField] private int _rows = 6;
        [SerializeField] private int _cols = 10;

        [Header("ItemSource")]
        [SerializeField] private ItemSource _itemSource = ItemSource.Csv;

        [Header("Scene References")]
        [SerializeField] private InventoryView _inventoryView;
        [SerializeField] private InventoryPresenter _inventoryPresenter;
        [SerializeField] private EquipmentView _equipmentView;
        [SerializeField] private EquipmentPresenter _equipmentPresenter;
        [SerializeField] private ConfirmDialogView _confirmDialogView;




        public override void InstallBindings()
        {
            // DI 부분
            // 같은 IItemRepositoryDomain에 csv,json 구현체 중 하나를 꽂음
            // 인스펙터의 _itemSource 한 값만 바꾸면 데이터 소스가 통째로 교체됨
            // 다른 계층은 어느 쪽인지 전혀 모름
            if (_itemSource == ItemSource.Csv)
            {
                Container.Bind<IItemRepositoryDomain>()
                    .To<CsvItemRepository>()
                    .AsSingle();
            }
            else
            {
                Container.Bind<IItemRepositoryDomain>()
                    .To<JsonItemRepository>()
                    .AsSingle();
            }

            Container.Bind<IItemRepositoryApplication>()
                .To<ItemRepositoryApplication>()
                .AsSingle();

            Container.Bind<IItemCatalogModel>()
                .To<ItemCatalogModel>()
                .AsSingle();

            // 인벤토리 바인딩
            Container.Bind<IInventoryDomain>()
                .FromMethod(_ => new InventoryDomain(_rows, _cols))
                .AsSingle();

            Container.Bind<IInventoryApplication>()
                .To<InventoryApplication>()
                .AsSingle();

            Container.Bind<IInventoryModel>()
                .To<InventoryModel>()
                .AsSingle();

            // 장착 바인딩
            Container.Bind<IEquipmentDomain>()
                .To<EquipmentDomain>()
                .AsSingle();

            Container.Bind<IEquipmentApplication>()
                .To<EquipmentApplication>()
                .AsSingle();

            Container.Bind<IEquipmentModel>()
                .To<EquipmentModel>()
                .AsSingle();

            // view
            Container.Bind<IInventoryView>()
                .FromInstance(_inventoryView)
                .AsSingle();

            Container.Bind<IEquipmentView>()
                .FromInstance(_equipmentView)
                .AsSingle();

            Container.Bind<IConfirmDialogView>()
                .FromInstance(_confirmDialogView)
                .AsSingle();

            // presenter
            Container.Bind<InventoryPresenter>()
                .FromInstance(_inventoryPresenter)
                .AsSingle()
                .NonLazy();

            Container.Bind<EquipmentPresenter>()
                .FromInstance(_equipmentPresenter)
                .AsSingle()
                .NonLazy();
        }
    }
}
