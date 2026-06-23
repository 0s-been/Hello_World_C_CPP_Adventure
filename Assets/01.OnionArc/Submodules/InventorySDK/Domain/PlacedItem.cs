using System.Collections.Generic;

namespace InventorySDK.Domain
{
    /// <summary>
    /// 아이템이 이 인벤토리의 어느 위치(row, col)에 있는지에 대한 배치 정보
    /// ItemInfo(고유 속성)와 분리한 이유
    /// 위치는 인벤토리에 있을 때, 그리고 상황에 따라 달라지는 정보임
    /// 아이템 원본의 고유 데이터는 거의 변하지 않으므로 
    /// 수정이 잦은 데이터와 그렇지 않은 데이터를 분리하고 싶었음
    /// </summary>
    public class PlacedItem
    {
        public ItemInfo Item { get; }   // 아이템 정보
        public int Row { get; }         // 좌상단 기준 행
        public int Col { get; }         // 좌상단 기준 열

        public PlacedItem(ItemInfo item, int row, int col)
        {
            Item = item;
            Row = row;
            Col = col;
        }

        /// <summary>
        /// 이 배치가 점유하는 모든 칸의 좌표를 돌려준다.
        /// 이 계산을 PlacedItem에 둔 이유: "내가 어느 칸을 차지하는가"는
        /// 배치 정보(Row,Col)와 아이템 크기(Width,Height)만으로 결정되는,
        /// 온전히 PlacedItem 자신의 정보로 정할 수 있기때문
        /// InventoryDomain은 이걸 받아서 그 칸들이 비었나만 판단 -> srp준수
        /// </summary>
        public IEnumerable<(int row, int col)> GetOccupiedCells()
        {
            for (int r = 0; r < Item.Height; r++)
                for (int c = 0; c < Item.Width; c++)
                    yield return (Row + r, Col + c);
        }
    }
}
