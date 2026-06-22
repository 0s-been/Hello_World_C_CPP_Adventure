using UnityEngine;

//model -> view로 데이터를 전달하기 위한 DTO클래스
//v단에서 m의 데이터 원본을 수정하지 못하도록 하며
//v가 m의 SO구조에 종속되지 않도록 함. DIP준수
//v는 이 StatObjectViewModel 구조만 알면 됨
//데이터 방식이 바뀌어도 View 변경하지 않아도 됨. DIP준수
//m가 만들어준 이 데이터에서 v가 자신이 필요한 데이터만 추출해서 사용하면 됨
public class StatObjectViewModel
{
    public StatType type;
    public int row, col;
    public float amount;
    public bool isDestroyed;
    public string displayname;
    public Sprite icon;
}
