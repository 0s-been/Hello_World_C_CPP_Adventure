namespace InventorySDK.Domain
{
    /// <summary>
    /// 아이템 정보를 담는 순수 데이터 클래스
    /// </summary>
    public class ItemInfo
    {
        public string Id { get; }
        public string Name { get; }
        public int Width { get; }
        public int Height { get; }
        /// <summary> 장착 부위 </summary>
        public EquipPart EquipPart { get; }

        /// <summary> 아이템 설명 툴팁 </summary>
        public string Description { get; }

        public ItemInfo(string id, string name, int width, int height,
                        EquipPart equipPart = EquipPart.None, string description = "")
        {
            Id = id;
            Name = name;
            Width = width;
            Height = height;
            EquipPart = equipPart;
            Description = description;
        }
    }
}
