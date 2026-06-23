using System;
using System.Collections.Generic;

namespace InventorySDK.Application
{
    using InventorySDK.Domain;

    /// <summary>
    /// 아이템 데이터 로드 유스케이스의 계약.
    /// </summary>
    public interface IItemRepositoryApplication
    {
        IObservable<IReadOnlyList<ItemInfo>> LoadItems();
    }
}
