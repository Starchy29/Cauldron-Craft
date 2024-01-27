using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void MonsterTrigger(Monster monster);
public delegate void Trigger();
public delegate int MonsterValue(Monster monster);

public enum StatusEffect {
    Regeneration,
    Strength,
    Haste,
    Energy,

    Poison,
    Fear,
    Slowness,
    Drowsiness,
    Frozen,
    Cursed,
    Haunted
}

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
}
