using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Global
{
    public static Vector2Int[] Cardinals = new Vector2Int[4] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

    public delegate bool CheckCondition<T>(T data);
    public delegate int GetValue<T>(T data);

    public static List<T> Filter<T>(this List<T> list, CheckCondition<T> conditionFunc) {
        List<T> result = new List<T>();
        foreach(T item in list) {
            if(conditionFunc(item)) {
                result.Add(item);
            }
        }
        return result;
    }

    public static T Max<T>(this List<T> list, GetValue<T> valueCalculator) {
        if(list.Count == 0) {
            return default(T);
        }
        
        T result = list[0];
        int max = valueCalculator(list[0]);
        for(int i = 1; i < list.Count; i++) {
            int value = valueCalculator(list[i]);
            if(value > max) {
                max = value;
                result = list[i];
            }
        }
        return result;
    }

    public static T Min<T>(this List<T> list, GetValue<T> valueCalculator) {
        if(list.Count == 0) {
            return default(T);
        }
        
        T result = list[0];
        int min = valueCalculator(list[0]);
        for(int i = 1; i < list.Count; i++) {
            int value = valueCalculator(list[i]);
            if(value < min) {
                min = value;
                result = list[i];
            }
        }
        return result;
    }

    public static int CalcTileDistance(Vector2Int start, Vector2Int end) {
        return Mathf.Abs(start.x - end.x) + Mathf.Abs(start.y - end.y);
    }
}
