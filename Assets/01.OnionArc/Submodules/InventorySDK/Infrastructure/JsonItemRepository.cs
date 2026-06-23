using System;
using System.Collections.Generic;
using System.IO;
using UniRx;
using UnityEngine;
using InventorySDK.Domain;

namespace InventorySDK.Infrastructure
{

    /// <summary>
    /// IItemRepositoryDomain의 JSON 구현체 (구현체 2).
    /// JSON 형식:
    ///   { "items": [ { "id":"sword","name":"검","width":1,"height":3,"equipPart":"Weapon" }, }
    ///
    /// JsonUtility의 두 가지 제약을 우회한다:
    ///   1) 최상위 배열을 직접 못 읽음 → { "items": [...] } 래퍼로 감싼다.
    ///   2) enum을 직접 매핑하지만 안전하게 string으로 받아 Enum.TryParse로 변환한다.
    /// </summary>
    public class JsonItemRepository : IItemRepositoryDomain
    {
        private readonly string _fileName;

        public JsonItemRepository(string fileName = "items.json")
        {
            _fileName = fileName;
        }

        public IObservable<IReadOnlyList<ItemInfo>> LoadItems()
        {
            var items = ParseJson(ReadRaw());
            return Observable.Return((IReadOnlyList<ItemInfo>)items);
        }

        private string ReadRaw()
        {
            string path = Path.Combine(UnityEngine.Application.streamingAssetsPath, _fileName);
            if (!File.Exists(path))
            {
                //Debug.LogWarning($"[JsonItemRepository] 파일 없음: {path} → 빈 목록 반환");
                return string.Empty;
            }
            return File.ReadAllText(path);
        }

        private List<ItemInfo> ParseJson(string raw)
        {
            var result = new List<ItemInfo>();
            if (string.IsNullOrWhiteSpace(raw)) return result;

            ItemDtoList dtoList;
            try
            {
                dtoList = JsonUtility.FromJson<ItemDtoList>(raw);
            }
            catch (Exception e)
            {
                //Debug.LogWarning($"[JsonItemRepository] 파싱 실패: {e.Message} → 빈 목록 반환");
                return result;
            }

            if (dtoList?.items == null) return result;

            foreach (var dto in dtoList.items)
            {
                int width  = dto.width  > 0 ? dto.width  : 1;
                int height = dto.height > 0 ? dto.height : 1;
                EquipPart part = Enum.TryParse(dto.equipPart, out EquipPart p) ? p : EquipPart.None;
                result.Add(new ItemInfo(dto.id, dto.name, width, height, part));
            }
            return result;
        }

        /// <summary>
        /// JsonUtility 매핑용 DTO 
        /// ItemInfo를 직접 못 쓰는 이유
        /// ItemInfo는 생성자 전용 불변 클래스라
        /// JsonUtility(필드 직접 대입 방식)와 안 맞기에  중간 운반용 구조를 추가함
        /// 이 DTO가 infrastructure에만 있고 domain으로 새어나가지 않는 게 포인트
        /// </summary>
        [Serializable]
        private class ItemDto
        {
            public string id;
            public string name;
            public int width;
            public int height;
            public string equipPart;
        }

        [Serializable]
        private class ItemDtoList
        {
            public List<ItemDto> items;
        }
    }
}
