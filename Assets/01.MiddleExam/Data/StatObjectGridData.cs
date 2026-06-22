using UnityEngine;
using System.Collections.Generic;   

[CreateAssetMenu(fileName = "StatObjectGridData", menuName = "Scriptable Objects/StatObjectGridData")]
public class StatObjectGridData : ScriptableObject
{
    [Header("Grid Info")]
    public int rows;
    public int cols;
    public float offset;

    [Header("Replacement Info")]
    public List<StatObjectData> statobjList;

    public StatObjectData GetData(int row, int col)
    {
        int idx = row * cols + col;

        if (idx < 0 || 
           idx >= statobjList.Count) return null;

        return statobjList[idx];
    }
}
