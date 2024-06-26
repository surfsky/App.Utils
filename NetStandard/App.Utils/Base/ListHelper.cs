﻿using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
//using System.Drawing;
//using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;

namespace App.Utils
{
    /// <summary>
    /// 列表操作（遍历、转换、过滤）
    /// </summary>
    public static partial class ListHelper
    {
        /// <summary>获取列表成员字符串并拼合</summary>
        public static string ToJoinString(this IEnumerable data)
        {
            var sb = new StringBuilder();
            foreach (object o in data)
                sb.Append(o.ToString());
            return sb.ToString();
        }

        //---------------------------------------------------
        // Dictionary -- List
        //---------------------------------------------------
        /// <summary>查找匹配的字典值（关键字可忽略大小写）</summary>
        public static T GetItem<T>(this Dictionary<string, T> dict, string key, bool ignoreCase)
        {
            foreach (var k in dict.Keys)
            {
                if (ignoreCase)
                {
                    if (k.Equals(key, StringComparison.OrdinalIgnoreCase))
                        return dict[k];
                }
                else
                {
                    if (k == key)
                        return dict[k];
                }
            }
            return default(T);
        }


        /// <summary>找到第一个匹配的位置（类似 Exists，但会返回位置信息）</summary>
        public static int IndexOf<T>(this IEnumerable<T> data, Func<T, bool> condition)
        {
            int n = -1;
            foreach (var o in data)
            {
                n++;
                if (condition(o))
                    return n;
            }
            return n;
        }

        //---------------------------------------------------
        // 集合操作
        //---------------------------------------------------
        /// <summary>合并两个集合（会排除重复项）。功能同Union，返回值不一样</summary>
        public static List<T> Union<T>(this List<T> list1, List<T> list2)
        {
            List<T> data = new List<T>(list1);
            foreach (var item in list2)
            {
                if (!data.Contains(item))
                    data.Add(item);
            }
            return data;
        }

        /// <summary>将列表根据条件拆分成两个列表</summary>
        public static Tuple<List<T>, List<T>> Split<T>(this List<T> raw, Predicate<T> condition)
        {
            List<T> list1 = new List<T>();
            List<T> list2 = new List<T>();
            foreach (var item in raw)
            {
                if (condition(item))
                    list1.Add(item);
                else
                    list2.Add(item);
            }
            return new Tuple<List<T>, List<T>>(list1, list2);
        }



        //---------------------------------------------------
        // 元素操作
        //---------------------------------------------------
        /// <summary>移动元素</summary>
        public static int MoveItem<T>(this List<T> list, int index, int newIndex)
        {
            if (index != newIndex && (index >= 0 && index < list.Count) && (newIndex >= 0 && newIndex < list.Count))
            {
                T temp = list[index];
                list.RemoveAt(index);
                list.Insert(newIndex, temp);
                return newIndex;
            }
            return index;
        }

        /// <summary>移动元素到首位</summary>
        public static int MoveItemHead<T>(this List<T> list, int index)
        {
            return MoveItem(list, index, 0);
        }

        /// <summary>移动元素到尾部</summary>
        public static int MoveItemTail<T>(this List<T> list, int index)
        {
            return MoveItem(list, index, list.Count - 1);
        }


        /// <summary>上移元素</summary>
        public static int MoveItemUp<T>(this List<T> list, int index)
        {
            return MoveItem(list, index, index - 1);
        }

        /// <summary>下移元素</summary>
        public static int MoveItemDown<T>(this List<T> list, int index)
        {
            return MoveItem(list, index, index + 1);
        }

        //---------------------------------------------------
        // 检索和遍历
        //---------------------------------------------------
        /// <summary>遍历过滤（同Where，但名字会冲突; 可考虑用 Query; Search; Filter）</summary>
        public static List<T> Search<T>(this IEnumerable<T> source, Func<T, bool> func)
        {
            var result = new List<T>();
            if (source != null)
                foreach (var item in source)
                {
                    if (func(item))
                        result.Add(item);
                }
            return result;
        }


        /// <summary>遍历并处理（替代ForEach，有返回值）</summary>
        public static List<T> Each<T>(this IEnumerable<T> source, Action<T> action)
        {
            var result = new List<T>();
            if (source != null)
                foreach (var item in source)
                {
                    action(item);
                    result.Add(item);
                }
            return result;
        }

        //public delegate void ActionRef<T1, T2>(ref T1 o1, ref T2 o2);

        /// <summary>遍历并处理（替代ForEach，有返回值）</summary>
        /// <param name="action">参数1为当前元素；参数2为前一个元素（可能为空）</param>
        public static List<T> Each2<T>(this List<T> source, Action<T, T> action)
        {
            var result = new List<T>();
            if (source != null)
            {
                T preItem = default(T);
                foreach (var item in source)
                {
                    action(item, preItem);
                    result.Add(item);
                    preItem = item;
                }
            }
            return result;
        }

        //---------------------------------------------------
        // 遍历转化（Cast）
        //---------------------------------------------------
        /// <summary>遍历并转换</summary>
        public static List<T> CastList<T>(this IEnumerable source)
        {
            var result = new List<T>();
            if (source != null)
                foreach (var item in source)
                    result.Add(item.To<T>());
            return result;
        }

        /// <summary>遍历并转换</summary>
        public static List<T> Cast<T>(this IEnumerable source, Func<object, T> func)
        {
            var result = new List<T>();
            if (source != null)
                foreach (var item in source)
                    result.Add(func(item));
            return result;
        }

        /// <summary>遍历并转换</summary>
        public static List<object> Cast<T>(this IEnumerable<T> source, Func<T, object> func)
        {
            return Cast<T, object>(source, func);
        }

        /// <summary>遍历并转换</summary>
        public static List<TOut> Cast<T, TOut>(this IEnumerable<T> source, Func<T, TOut> func)
        {
            var result = new List<TOut>();
            if (source != null)
                foreach (var item in source)
                    result.Add(func(item));
            return result;
        }

        /// <summary>遍历并浅克隆</summary>
        public static List<T> Clone<T>(this IEnumerable<T> source)
        {
            return Cast(source, t => t);
        }


        /// <summary>转化为整型列表</summary>
        public static List<int> CastInt(this IEnumerable source)
        {
            return source.Cast<int>(t =>
                t.IsEnum()
                    ? Convert.ToInt32(t)
                    : int.Parse(t.ToString())
                    );
        }
        /// <summary>转化为整型列表</summary>
        public static List<Int64> CastLong(this IEnumerable source)
        {
            return source.Cast<Int64>(t =>
                t.IsEnum()
                    ? Convert.ToInt64(t)
                    : long.Parse(t.ToString())
                    );
        }

        /// <summary>转化为整型列表</summary>
        public static List<string> CastString(this IEnumerable source)
        {
            return source.Cast<string>(t => t.ToString());
        }

        /// <summary>转化为枚举列表</summary>
        public static List<T> CastEnum<T>(this IEnumerable source) where T : struct
        {
            return source.Cast<T>(t => (T)Enum.ToObject(typeof(T), Convert.ToInt32(t)));
        }
    }
}