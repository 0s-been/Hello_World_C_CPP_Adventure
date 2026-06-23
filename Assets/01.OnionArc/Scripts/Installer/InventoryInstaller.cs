using UnityEngine;
using Zenject;
using InventorySDK.Domain;
using InventorySDK.Application;
using InventorySDK.Infrastructure;
using OnionArc.Model;
using OnionArc.View;
using OnionArc.Presenter;


namespace OnionArc.Installer
{
    /// <summary>
    /// 인벤토리 관련 모든 연결이 모이는 곳
    /// zenject를 통해 DI를 받음
    /// </summary>
    public class InventoryInstaller : MonoInstaller
    {
        public enum ItemSource { Csv, Json }   //DI 교체 선택지, 사실 이 부분도 별도로 빼야할듯

        [Header("Grid Size")]
        [SerializeField] private int _rows = 6;
        [SerializeField] private int _cols = 10;

        [Header("ItemSource")]
        [SerializeField] private ItemSource _itemSource = ItemSource.Csv;

        [Header("Scene References")]
        [SerializeField] private InventoryView _inventoryView;
        [SerializeField] private InventoryPresenter _inventoryPresenter;

        public override void InstallBindings()
        {
            // DI 부분
            // 같은 IItemRepositoryDomain에 csv,json 구현체 중 하나를 꽂음
            // 인스펙터의 _itemSource 한 값만 바꾸면 데이터 소스가 통째로 교체됨.
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

            // view
            Container.Bind<IInventoryView>()
                .FromInstance(_inventoryView)
                .AsSingle();

            // presenter
            Container.Bind<InventoryPresenter>()
                .FromInstance(_inventoryPresenter)
                .AsSingle()
                .NonLazy();
        }
    }
}
