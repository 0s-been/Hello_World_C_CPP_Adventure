using System;
using System.Collections.Generic;
 using InventorySDK.Domain;

namespace OnionArc.Model
{

    /// <summary>
    /// 로드된 아이템들을 보관 및 제공하는 model의 interface
    /// 인벤토리에 놓을 수 있는 아이템풀 
    /// </summary>
    public interface IItemCatalogModel
    {
        /// <summary>아이템 데이터 풀</summary>
        IReadOnlyList<ItemInfo> Items { get; }

        /// <summary>로드 완료 시 전체 목록을 통지</summary>
        IObservable<IReadOnlyList<ItemInfo>> OnLoaded { get; }

        /// <summary>데이터 소스에서 아이템을 불러옴</summary>
        void Load();

        /// <summary>id로 아이템 하나 찾기 없으면 null</summary>
        ItemInfo FindById(string id);
    }
}
