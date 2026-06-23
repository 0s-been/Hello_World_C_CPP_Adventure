using System;
using System.Collections.Generic;

namespace InventorySDK.Domain
{
    /// <summary>
    /// "아이템 정의 데이터를 불러온다"는 계약. 출력은 항상 List&lt;ItemInfo&gt;.
    /// CSV로 읽든 JSON으로 읽든 이 인터페이스 뒤에 숨으므로,
    /// 이 인터페이스를 쓰는 쪽은 파일 형식을 전혀 모른다 (DIP).
    ///
    /// 과제의 "DI 교체 대상"이 바로 이 인터페이스다:
    /// 같은 IItemRepositoryDomain을 상속한 CSV/JSON 구현체를 Installer에서 스왑한다.
    ///
    /// 완료를 IObservable로 통지하는 이유: 과제 요구사항 + 파일 읽기는
    /// 비동기가 될 수 있어(로컬은 동기지만 원격이면 비동기) 스트림이 자연스럽다.
    /// </summary>
    public interface IItemRepositoryDomain
    {
        IObservable<IReadOnlyList<ItemInfo>> LoadItems();
    }
}
