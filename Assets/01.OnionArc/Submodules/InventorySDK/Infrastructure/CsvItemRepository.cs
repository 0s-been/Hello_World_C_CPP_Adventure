using System;
using System.Collections.Generic;
using System.IO;
using UniRx;
using UnityEngine;
using InventorySDK.Domain;

namespace InventorySDK.Infrastructure
{

    /// <summary>
    /// IItemRepositoryDomain의 CSV 구현체 
    /// CSV 형식 (1행은 헤더)
    ///   id,name,width,height,equipPart
    ///   sword,검,1,3,Weapon
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
            // Observable.Start로 감싸지 않고, 로컬 파일이라 즉시 읽어 Return으로 통지.
            // (원격이었다면 비동기 스트림으로 바꾸면 됨 — 인터페이스는 그대로)
            var items = ParseCsv(ReadRaw());
            return Observable.Return((IReadOnlyList<ItemInfo>)items);
        }

        private string ReadRaw()
        {
            string path = Path.Combine(UnityEngine.Application.streamingAssetsPath, _fileName);
            if (!File.Exists(path))
            {
                //Debug.LogWarning($"[CsvItemRepository] 파일 없음: {path} → 빈 목록 반환");
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
                if (cols.Length < 5) continue; // 형식 불량 행은 무시

                string id   = cols[0].Trim();
                string name = cols[1].Trim();

                // 숫자 파싱 실패 시 안전한 기본값(1)로 런타임 에러 방지
                int width  = ParseIntOr(cols[2], 1);
                int height = ParseIntOr(cols[3], 1);
                EquipPart part = ParseEquipPart(cols[4]);

                result.Add(new ItemInfo(id, name, width, height, part));
            }
            return result;
        }

        private static int ParseIntOr(string s, int fallback)
            => int.TryParse(s.Trim(), out var v) && v > 0 ? v : fallback;

        private static EquipPart ParseEquipPart(string s)
            => Enum.TryParse(s.Trim(), out EquipPart p) ? p : EquipPart.None;
    }
}
