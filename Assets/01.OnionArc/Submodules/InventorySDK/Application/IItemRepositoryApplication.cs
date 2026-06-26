using System;
using System.Collections.Generic;
using InventorySDK.Domain;

namespace InventorySDK.Application
{

    /// <summary>
    /// 아이템 데이터 로드 유스케이스 interface
    /// </summary>
    public interface IItemRepositoryApplication
    {
        IObservable<IReadOnlyList<ItemInfo>> LoadItems();
    }
}
