using System;
using System.Collections.Generic;

namespace InventorySDK.Application
{
    using InventorySDK.Domain;

    /// <summary>
    /// 아이템 데이터 로드 application구현. 지금은 단일 domain 위임이지만,
    /// 나중에 로드 후 검증, 중복 id 제거 같은 규칙을 넣게 될 경우 여기에 추가하면 됨
    /// </summary>
    public class ItemRepositoryApplication : IItemRepositoryApplication
    {
        private readonly IItemRepositoryDomain _repository;

        public ItemRepositoryApplication(IItemRepositoryDomain repository)
        {
            _repository = repository;
        }

        public IObservable<IReadOnlyList<ItemInfo>> LoadItems()
            => _repository.LoadItems();
    }
}
