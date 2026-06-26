using System;
using System.Collections.Generic;

namespace InventorySDK.Domain
{
    /// <summary>
    /// 미리 정해진 아이템 데이터 풀에 대한 interface
    /// csv로든 읽든 json으로 읽든 구현체에서 구현만 변경하면 됨
    /// 이 인터페이스를 쓰는 쪽은 파일 형식을 전혀 몰라도 됨 -> dip 준수
    /// DI를 통해 installer에서 구현제만 갈아 끼우면 됨
    /// 비동기가 될 수 있어서 스트림이 자연스러움 및 게임분석 시간에 예제 소스 방식 재활용
    /// </summary>
    public interface IItemRepositoryDomain
    {
        IObservable<IReadOnlyList<ItemInfo>> LoadItems();
    }
}
