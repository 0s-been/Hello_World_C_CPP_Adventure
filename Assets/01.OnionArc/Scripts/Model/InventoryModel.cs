using System;
using System.Collections.Generic;
using InventorySDK.Domain;
using InventorySDK.Application;

namespace OnionArc.Model
{

    public class InventoryModel : IInventoryModel
    {
        private readonly IInventoryApplication _application;

        public InventoryModel(IInventoryApplication application)
        {
            _application = application;
        }


        // application으로 위임
        public int Rows => _application.Rows;
        public int Cols => _application.Cols;
        public IReadOnlyList<PlacedItem> PlacedItems => _application.PlacedItems;


        // domain에서 출발한 변경 통지 스트림을 app으로 위임해서 위로 노출
        // presenter가 이걸 구독해서 view를 갱신.
        public IObservable<IReadOnlyList<PlacedItem>> OnInventoryChanged
            => _application.OnInventoryChanged;

        public bool TryPlace(ItemInfo item, int row, int col)
            => _application.TryPlace(item, row, col);

        public bool RemoveAt(int row, int col)
            => _application.RemoveAt(row, col);

        public PlacedItem GetItemAt(int row, int col)
            => _application.GetItemAt(row, col);
    }
}
