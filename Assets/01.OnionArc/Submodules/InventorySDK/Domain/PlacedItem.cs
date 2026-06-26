using System.Collections.Generic;

namespace InventorySDK.Domain
{
    /// <summary>
    /// 아이템이 이 인벤토리에 어디에 배치되었는 지에 대한 정보
    /// </summary>
    // ItemInfo와 분리한 이유
    // 위치는 인벤토리에 있을 때, 그리고 상황에 따라 달라지는 정보임
    // 아이템 원본의 고유 데이터는 거의 변하지 않으므로 
    // 수정이 잦은 데이터와 그렇지 않은 데이터를 분리하고 싶었음
    public class PlacedItem
    {
        public ItemInfo Item { get; }   
        public int Row { get; }         
        public int Col { get; }         

        public PlacedItem(ItemInfo item, int row, int col)
        {
            Item = item;
            Row = row;
            Col = col;
        }

        /// <summary>
        /// 이 배치가 차지하는 모든 칸의 좌표를 돌려줌
        /// 점유 칸 좌표를 지연 생산하는 이터레이터
        /// </summary>
        // 이 계산을 PlacedItem에 둔 이유
        // 점유 칸 계산을 PlacedItem 한 곳에 캡슐화해서 필요한 곳에서 호출만 하면 됨
        // 리스트 객체 없이 값만 흘려보냄 -> gc부담 완화 및 메모리 효율
        // 칸을 차지하는가는 배치 상태에 대한 정보와 아이템 크기에 대한 정보만으로 결정 가능
        // 온전히 PlacedItem 자신의 정보로 정할 수 있기때문
        // InventoryDomain은 이걸 받아서 그 칸들이 비었나만 판단 -> srp준수
        // 아닌가 domain에 둔 이유가 순수 데이터 클래스로써 둔 건데
        // 그럼 이런 로직 기능이 없도록 하고 이걸 inventorydomain에서 
        // 하도록 해야 하나
        public IEnumerable<(int row, int col)> GetOccupiedCells()
        {
            for (int r = 0; r < Item.Height; r++)
            {
                for (int c = 0; c < Item.Width; c++)
                {
                    yield return (Row + r, Col + c);
                }
            }
        }
    }
}
