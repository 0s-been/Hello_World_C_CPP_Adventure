using System;
using System.Collections.Generic;
using System.IO;
using UniRx;
using UnityEngine;
using InventorySDK.Domain;

namespace InventorySDK.Infrastructure
{

    /// <summary>
    /// IItemRepositoryDomain의 csv 구현체 
    /// </summary>
    public class CsvItemRepository : IItemRepositoryDomain
    {
        private readonly string _fileName;

        public CsvItemRepository(string fileName = "items.csv")
        {
            _fileName = fileName;
        }

        public IObservable<IReadOnlyList<ItemInfo>> LoadItems()
        {
            var items = ParseCsv(ReadRaw());
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

        private List<ItemInfo> ParseCsv(string raw)
        {
            var result = new List<ItemInfo>();
            if (string.IsNullOrWhiteSpace(raw)) return result;

            // \r\n / \n 모두 대응
            var lines = raw.Replace("\r\n", "\n").Split('\n');

            // 0번 줄은 헤더라 건너뜀
            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (line.Length == 0) continue;

                var cols = line.Split(',');
                if (cols.Length < 5) continue; // 최소 5열 필요 desc는 비워도 됨

                string id   = cols[0].Trim();
                string name = cols[1].Trim();

                int width  = ParseIntOr(cols[2], 1);
                int height = ParseIntOr(cols[3], 1);
                EquipPart part = ParseEquipPart(cols[4]);

                // description은 없으면 빈 문자열
                string description = cols.Length >= 6 ? cols[5].Trim() : "";

                result.Add(new ItemInfo(id, name, width, height, part, description));
            }
            return result;
        }

        private static int ParseIntOr(string s, int fallback)
            => int.TryParse(s.Trim(), out var v) && v > 0 ? v : fallback;

        private static EquipPart ParseEquipPart(string s)
            => Enum.TryParse(s.Trim(), out EquipPart p) ? p : EquipPart.None;
    }
}
