using System;
using System.Collections.Generic;
using System.IO;
using UniRx;
using UnityEngine;
using InventorySDK.Domain;

namespace InventorySDK.Infrastructure
{

    /// <summary>
    /// IItemRepositoryDomain의 json 구현체 
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
                return result;
            }

            if (dtoList?.items == null) return result;

            foreach (var dto in dtoList.items)
            {
                int width  = dto.width  > 0 ? dto.width  : 1;
                int height = dto.height > 0 ? dto.height : 1;
                EquipPart part = Enum.TryParse(dto.equipPart, out EquipPart p) ? p : EquipPart.None;
                // description은 없으면 null이 올 수 있어 빈 문자열로 보정
                string description = dto.description ?? "";
                result.Add(new ItemInfo(dto.id, dto.name, width, height, part, description));
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
            public string description;
        }

        [Serializable]
        private class ItemDtoList
        {
            public List<ItemDto> items;
        }
    }
}
