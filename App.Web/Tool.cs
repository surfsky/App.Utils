using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Web
{
    internal static class Tool
    {
        //--------------------------------------------------
        // 为空
        //--------------------------------------------------
        /// <summary>判断对象是否不为空、空字符串、空列表</summary>
        public static bool IsNotEmpty(this object o)
        {
            return !o.IsEmpty();
        }

        /// <summary>判断对象是否为空、空字符串、空列表</summary>
        public static bool IsEmpty(this object o)
        {
            if (o == null)
                return true;
            if (o is string)
                return string.IsNullOrEmpty(o as string);
            if (o is IEnumerable)
                return (o as IEnumerable).Count() == 0;
            return false;
        }

        /// <summary>获取列表的长度</summary>
        public static int Count(this IEnumerable data)
        {
            var n = 0;
            var e = data.GetEnumerator();
            while (e.MoveNext())
            {
                n++;
            }
            return n;
        }


    }
}
