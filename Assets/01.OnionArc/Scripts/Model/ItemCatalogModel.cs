using System;
using System.Collections.Generic;
using UniRx;
using InventorySDK.Domain;
using InventorySDK.Application;

namespace OnionArc.Model
{

    /// <summary>
    /// 아이템 카탈로그 model. 
    /// application을 통해 데이터를 로드해 메모리에 보관.
    /// 로드 완료를 IObservable로 통지.
    /// </summary>
    public class ItemCatalogModel : IItemCatalogModel
    {
        private readonly IItemRepositoryApplication _application;
        private readonly List<ItemInfo> _items = new List<ItemInfo>();

        private readonly Subject<IReadOnlyList<ItemInfo>> _onLoaded
            = new Subject<IReadOnlyList<ItemInfo>>();

        public ItemCatalogModel(IItemRepositoryApplication application)
        {
            _application = application;
        }

        public IReadOnlyList<ItemInfo> Items => _items;
        public IObservable<IReadOnlyList<ItemInfo>> OnLoaded => _onLoaded;

        /// <summary>
        /// Application이 돌려주는 스트림을 구독하고 도착하면 보관 후 통지
        /// </summary>
        public void Load()
        {
            _application.LoadItems()
                .Subscribe(loaded =>
                {
                    _items.Clear();
                    _items.AddRange(loaded);
                    _onLoaded.OnNext(_items);
                });
        }

        public ItemInfo FindById(string id)
        {
            for(int i = 0; i < _items.Count; i++ )
            {
                var item = _items[i] as ItemInfo;
                if (item.Id == id) return item;
            }
            return null;
        }
    }
}
