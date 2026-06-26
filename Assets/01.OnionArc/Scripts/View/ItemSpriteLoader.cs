using System.Collections.Generic;
using UnityEngine;
using InventorySDK.Domain;

namespace OnionArc.View
{
    /// <summary>
    /// 아이템  스프라이트 로더 및 캐싱
    /// 네이밍 규칙: {id}_{EquipPart}_{인덱스}
    /// Resources/Items/ 아래에서 로드
    /// Resources.Load는 매번 디스크/번들을 뒤져 느리므로, 한 번 로드한 스프라이트는
    /// 딕셔너리에 캐싱해 재사용해서 같은 조각을 매 Render마다 다시 안 읽도록 함
    /// static으로 둔 이유 -> 인벤토리/장착 view가 같은 캐시를 공유하면
    /// 중복 로드가 줄고, 스프라이트는 읽기 전용이라 공유해도 안전하기 때문이라고 생각
    /// </summary>
    // 얘는 어떻게 보면 assetbundle이나 itemrespository처럼 데이터를 파싱하는데 왜 view 단에 있느냐
    // 다루는 게 스프라이트이기 때문 -> unity 종속적이며 단순 데이터를 받아 그 스프라이트를 렌더링함
    // 특정 데이터의 상태를 변경하지는 않음
    // 혹시라도 나중에 로딩 방식을 바꾸고 싶으면 그때는 IItemSpriteProvider 같은 인터페이스로 추상화하고 DI를 받도록 할 수 있겠지만
    // 이 또한 지금 당장엔 필요 없고 zenject를 통한 DI 부분은 이미 itemrespository(csv, json)으로 이미 했기 때문에
    // yagni 원칙에 따라 이것까진 지금 당장은 포기
    // interface가 없는 이유는 tooltipview와 같은 이유
    public static class ItemSpriteLoader
    {
        private const string ResourceFolder = "Items/";

        // 파일명 -> 스프라이트 캐시 (없는 것도 null로 기록해 재시도 방지)
        private static readonly Dictionary<string, Sprite> _cache
            = new Dictionary<string, Sprite>();

        /// <summary>
        /// 아이템의 특정 로컬 칸(localRow, localCol)에 해당하는 조각 스프라이트를 반환
        /// 없으면 null (호출부에서 색 등으로 대체 가능)
        /// </summary>
        public static Sprite Get(ItemInfo item, int localRow, int localCol)
        {
            if (item == null) return null;

            int index = localRow * item.Width + localCol;
            // 시트(.png) 이름 = id_부위
            string sheet = $"{item.Id}_{item.EquipPart}";
            // 캐시에 사용할 키
            string key = $"{sheet}_{index}";               

            // 캐시에 있으면 바로 반환 (null이 기록돼 있어도 그대로 반환 -> 재로드 안 함)
            if (_cache.TryGetValue(key, out var cached))
                return cached;
           
            // 처음 보는 키 -> Resources에서 로드 후 캐시에 기록
            Sprite[] all = Resources.LoadAll<Sprite>(ResourceFolder + sheet);
            Sprite found = (index >= 0 && index < all.Length) ? all[index] : null;   

            _cache[key] = found;
            return found;
            
        }

        /// <summary>캐시 비우기 </summary>
        public static void ClearCache()
        {
            _cache.Clear();
        }
    }
}
