using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public delegate void MonsterTrigger(Monster monster);
public delegate void Trigger();
public delegate int MonsterValue(Monster monster);

public static class Global
{
    public static Vector2Int[] Cardinals = new Vector2Int[4] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

    #region Extension Functions
    public delegate bool CheckCondition<T>(T data);
    public delegate float GetValue<T>(T data);
    public delegate T ValueCombiner<T>(T current, T next);

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

    public static List<T> AllTiedMax<T>(this List<T> list, GetValue<T> valueCalculator) {
        List<T> result = new List<T>();
        float max = float.MinValue;

        foreach(T element in list) {
            float value = valueCalculator(element);
            if(value > max) {
                max = value;
                result.Clear();
                result.Add(element);
            }
            else if(value == max) {
                result.Add(element);
            }
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

    public static int? IndexOf<T>(this T[] array, T value) {
        for(int i = 0; i < array.Length; i++) {
            if(array[i].Equals(value)) {
                return i;
            }
        }

        return null;
    }

    public static bool Contains<T>(this T[] array, T value) {
        for(int i = 0; i < array.Length; i++) {
            if(array[i].Equals(value)) {
                return true;
            }
        }

        return false;
    }

    public static bool AreContentsEqual<T>(this List<T> list, List<T> other) {
        if(list.Count != other.Count) {
            return false;
        }

        for(int i = 0; i < list.Count; i++) {
            if(!list[i].Equals(other[i])) {
                return false;
            }
        }

        return true;
    }

    public static List<T> CollapseList<T>(this List<List<T>> listOfLists) {
        if(listOfLists.Count == 0) {
            return new List<T>();
        }

        List<T> result = listOfLists[0];
        for(int i = 1; i < listOfLists.Count; i++) {
            result.AddRange(listOfLists[i]);
        }
        return result;
    }
    #endregion

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

    public static Vector2 DetermineCenter(List<Vector2Int> tileGroup) {
        LevelGrid level = LevelGrid.Instance;
        Vector2 firstCenter = level.Tiles.GetCellCenterWorld((Vector3Int)tileGroup[0]);
        Rect coveredArea = new Rect(firstCenter.x, firstCenter.y, 0, 0);
        for(int i = 1; i < tileGroup.Count; i++) {
            Vector2 tileCenter = level.Tiles.GetCellCenterWorld((Vector3Int)tileGroup[i]);
            if(tileCenter.x > coveredArea.xMax) {
                coveredArea.xMax = tileCenter.x;
            }
            else if(tileCenter.x < coveredArea.xMin) {
                coveredArea.xMin = tileCenter.x;
            }

            if(tileCenter.y > coveredArea.yMax) {
                coveredArea.yMax = tileCenter.y;
            }
            else if(tileCenter.y < coveredArea.yMin) {
                coveredArea.yMin = tileCenter.y;
            }
        }
        
        return coveredArea.center;
    }
}
