using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public delegate void MonsterTrigger(Monster monster);
public delegate void Trigger();
public delegate int MonsterValue(Monster monster);

public static class Global
{
    public static Vector2Int[] Cardinals = new Vector2Int[4] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

    #region Functional Programming List Functions
    public delegate bool CheckCondition<T>(T data);
    public delegate float GetValue<T>(T data);
    public delegate EndType MapValue<StartValue, EndType>(StartValue value);
    public delegate T ValueCombiner<T>(T current, T next);

    public static List<T> Filter<T>(this List<T> list, CheckCondition<T> conditionFunc) {
        List<T> result = new List<T>();
        foreach(T item in list) {
            if(conditionFunc(item)) {
                result.Add(item);
            }
        }
        return result;
    }

    public static T Max<T>(this T[] array, GetValue<T> valueCalculator) {
        if(array.Length == 0) {
            return default(T);
        }
        
        T result = array[0];
        float max = valueCalculator(array[0]);
        for(int i = 1; i < array.Length; i++) {
            float value = valueCalculator(array[i]);
            if(value > max) {
                max = value;
                result = array[i];
            }
        }
        return result;
    }

    public static T Min<T>(this T[] array, GetValue<T> valueCalculator) {
        if(array.Length == 0) {
            return default(T);
        }
        
        T result = array[0];
        float min = valueCalculator(array[0]);
        for(int i = 1; i < array.Length; i++) {
            float value = valueCalculator(array[i]);
            if(value < min) {
                min = value;
                result = array[i];
            }
        }
        return result;
    }

    public static T Min<T>(this List<T> list, GetValue<T> valueCalculator) {
        return list.ToArray().Min(valueCalculator);
    }

    public static T Max<T>(this List<T> list, GetValue<T> valueCalculator) {
        return list.ToArray().Max(valueCalculator);
    }

    public static List<EndType> Map<StartType, EndType>(this List<StartType> list, MapValue<StartType, EndType> mapFunction) {
        List<EndType> result = new List<EndType>();
        foreach(StartType element in list) {
            result.Add(mapFunction(element));
        }
        return result;
    }

    public static T Collapse<T>(this T[] array, ValueCombiner<T> valueCombiner) {
        if(array.Length == 0) {
            return default(T);
        }

        T result = array[0];
        for(int i = 1; i < array.Length; i++) {
            result = valueCombiner(result, array[i]);
        }
        return result;
    }

    public static T Collapse<T>(this List<T> list, ValueCombiner<T> valueCombiner) {
        return list.ToArray().Collapse(valueCombiner);
    }
    #endregion

    public static int? IndexOf<T>(this T[] array, T value) {
        for(int i = 0; i < array.Length; i++) {
            if(array[i].Equals(value)) {
                return i;
            }
        }

        return null;
    }

    public static int CalcTileDistance(Vector2Int start, Vector2Int end) {
        return Mathf.Abs(start.x - end.x) + Mathf.Abs(start.y - end.y);
    }

    public static Rect GetObjectArea(GameObject gObj) {
        Vector2 middle = gObj.transform.position;
        Vector2 scale = gObj.transform.lossyScale;
        return new Rect(middle - scale / 2, scale);
    }

    public static bool IsAdjacent(Vector2Int tile1, Vector2Int tile2) {
        return Mathf.Abs(tile1.x - tile2.x) <= 1 && Mathf.Abs(tile1.y - tile2.y) <= 1;
    }

    public static Color ChangeSaturation(Color color, float amount) {
        float h, s, v;
        Color.RGBToHSV(color, out h, out s, out v);
        if(s == 0f) {
            v += amount; // keep grayscale colors grayscale
        } else {
            s += amount;
        }
        return Color.HSVToRGB(h, s, v);
    }

    public static Color ChangeValue(Color color, float amount) {
        float h, s, v;
        Color.RGBToHSV(color, out h, out s, out v);
        v += amount;
        return Color.HSVToRGB(h, s, v);
    }
}
