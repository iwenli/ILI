﻿#region << 版 本 注 释 >>
/*----------------------------------------------------------------
 *  Copyright (C) 2018 天下商机（txooo.com）版权所有
 * 
 *  文 件 名：StringExtension
 *  所属项目：System
 *  创建用户：iwenli(HouWeiya)
 *  创建时间：2018/5/21 10:39:44
 *  
 *  功能描述：
 *          1、
 * 
 *  修改标识：  
 *  修改描述：
 *  修改时间：
 *  待 完 善：
 *          1、 
----------------------------------------------------------------*/
#endregion

using Iwenli.Text;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace System
{
    /// <summary>
	/// 包含了与字符串相关的一些常用扩展方法
	/// </summary>
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static class StringExtension
    {
        #region char

        /// <summary>
        /// 转换为BYTE
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static byte ToHexByte(this char code)
        {
            if (code >= 'A' && code <= 'F') return (byte)(10 + (code - 'A'));
            if (code >= 'a' && code <= 'f') return (byte)(10 + (code - 'a'));
            if (code >= '0' && code <= '9') return (byte)(code - '0');
            return 0;
        }

        #endregion

        #region Common

        /// <summary>
        /// 判断当前字符串是否为空或长度为零
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns>true为空或长度为零</returns>
        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        /// <summary>
        /// 将字符串分割成行数组
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string[] ToLines(this string value)
        {
            if (String.IsNullOrEmpty(value))
                return null;

            return value.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// 替换模板字符
        /// </summary>
        /// <param name="template">模板内容</param>
        /// <param name="tags">标签数组</param>
        /// <param name="dest">内容数组</param>
        /// <returns>替换后的结果</returns>
        public static string TemplateTagReplace(this string template, string[] tags, string[] dest)
        {
            if (tags.Length != dest.Length) throw new InvalidOperationException("数组长度必须一样.");

            for (int i = 0; i < tags.Length; i++)
            {
                template = template.Replace(tags[i], dest[i]);
            }

            return template;
        }

        static readonly Regex ExpressConvertor = new Regex(@"(\r|\n|'|""|\\|/)");

        /// <summary>
        /// 将字符串转义为表达式
        /// </summary>
        /// <param name="value">要转换的字符串</param>
        /// <returns>转义后的结果</returns>
        public static string EncodeToJsExpression(this string value)
        {
            if (value.IsNullOrEmpty()) return string.Empty;

            return ExpressConvertor.Replace(value, (s) =>
            {
                if (s.Value == "\r") return "\\r";
                if (s.Value == "\n") return "\\n";
                return string.Concat("\\", s.Value);
            });
        }

        static readonly Regex ExpressReConvertor = new Regex(@"\\(r|n|'|""|\|/|u([a-fA-F0-9]{4}))");

        /// <summary>
        /// 将字符串从JS的转义中转换
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string DecodeFromJsExpression(this string value)
        {
            if (value.IsNullOrEmpty()) return string.Empty;

            return ExpressReConvertor.Replace(value, (s) =>
            {
                var key = s.Groups[1].Value;

                if (key == "r") return "\r";
                if (key == "n") return "\n";
                if (key[0] == 'u')
                {
                    return ((char)s.Groups[2].Value.ToInt32(style: NumberStyles.AllowHexSpecifier)).ToString();
                }
                return key;
            });
        }

        /// <summary>
        /// 为字符串设定默认值
        /// </summary>
        /// <param name="value">要设置的值</param>
        /// <param name="defaultValue">如果要设定的值为空，则返回此默认值</param>
        /// <returns>设定后的结果</returns>
        public static string DefaultForEmpty(this string value, string defaultValue)
        {
            return string.IsNullOrEmpty(value) ? defaultValue : value;
        }

        /// <summary>
        /// 使用指定参数来对当前字符串进行格式化
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="args">The args.</param>
        /// <returns></returns>
        public static string FormatWith(this string value, params object[] args)
        {
            if (value == null) throw new ArgumentNullException("value");
            else if (value.Length == 0) return string.Empty;
            else if (args.Length == 0) return value;
            else return string.Format(value, args);
        }

        static readonly Regex _formatObjectReg = new Regex(@"^\w+[\.\-_0-9a-z]+@[0-9a-z]+([\-_\.][0-9a-z]+)*\.[a-z]{2,3}$", RegexOptions.Compiled);
        /// <summary>
        /// 通过指定参数对当前字符串格式化
        /// </summary>
        /// <param name="value">被格式化字符串,形如[{Param1}推荐友{Param2}购买...]</param>
        /// <param name="obj">包含Param1，Param2的Class对象</param>
        /// <returns></returns>
        public static string FormatObj(this string value, object obj)
        {
            if (obj == null) return value;

            Type type = obj.GetType();
            PropertyInfo[] properties = type.GetProperties(
                BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance);

            MatchEvaluator evaluator = match =>
            {
                string propertyName = match.Groups["Name"].Value;
                string propertyFormat = match.Groups["Format"].Value;

                PropertyInfo propertyInfo = properties.FirstOrDefault(p => p.Name == propertyName);
                if (propertyInfo != null)
                {
                    object propertyValue = propertyInfo.GetValue(obj, null);
                    if (string.IsNullOrEmpty(propertyFormat) == false)
                        return string.Format("{0:" + propertyFormat + "}", propertyValue);
                    else return propertyValue.ToString();
                }
                else return match.Value;
            };
            return _formatObjectReg.Replace(value, evaluator);
        }

        /// <summary>
        /// 通过指定参数对当前字符串格式化
        /// </summary>
        /// <param name="value">被格式化字符串,形如[{Param1}推荐友{Param2}购买...]</param>
        /// <param name="obj">包含Param1，Param2的NameValueCollection对象</param>
        /// <returns></returns>
        public static string FormatDic(this string format, NameValueCollection table)
        {
            MatchEvaluator evaluator = match =>
            {
                string propertyName = match.Groups["Name"].Value;
                string propertyFormat = match.Groups["Format"].Value;

                var propertyInfo = table[propertyName];
                if (propertyInfo != null)
                {
                    object propertyValue = propertyInfo;
                    if (string.IsNullOrEmpty(propertyFormat) == false)
                        return string.Format("{0:" + propertyFormat + "}", propertyValue);
                    else return propertyValue.ToString();
                }
                else return match.Value;
            };
            return _formatObjectReg.Replace(format, evaluator);
        }

        static readonly Regex _emailReg = new Regex(@"^\w+[\.\-_0-9a-z]+@[0-9a-z]+([\-_\.][0-9a-z]+)*\.[a-z]{2,3}$", RegexOptions.IgnoreCase);
        /// <summary>
        /// 判断一个字符串是否是邮件地址
        /// </summary>
        /// <param name="value">地址</param>
        /// <returns>如果是，则返回 true</returns>
        public static bool IsEmail(this string value)
        {
            return !value.IsNullOrEmpty() && _emailReg.IsMatch(value);
        }

        /// <summary>
        /// 比较两个字符串在忽略大小写的情况下是否相等
        /// </summary>
        /// <param name="value">字符串1</param>
        /// <param name="compareTo">要比较的字符串</param>
        /// <returns>是否相等</returns>
        public static bool IsIgnoreCaseEqualTo(this string value, string compareTo)
        {
            return string.Compare(value, compareTo, true) == 0;
        }

        /// <summary>
        /// 按照字节截取字符串
        /// </summary>
        /// <param name="value">字符串</param>
        /// <param name="byteLength">字节长度，一个汉字两个字节</param>
        /// <returns>截取后的字符串</returns>
        /// <exception cref="System.ArgumentException">指定了截取后的省略字符串，但要截取的字符串过短，不足以容纳省略字符串</exception>
        public static string GetSubString(this string value, int byteLength)
        {
            return GetSubString(value, byteLength, string.Empty);
        }
        /// <summary>
        /// 按照字节截取字符串
        /// </summary>
        /// <param name="value">字符串</param>
        /// <param name="byteLength">字节长度，一个汉字两个字节</param>
        /// <param name="tailString">如果截取了，那么省略字符串</param>
        /// <returns>截取后的字符串</returns>
        /// <exception cref="System.ArgumentException">指定了截取后的省略字符串，但要截取的字符串过短，不足以容纳省略字符串</exception>
        public static string GetSubString(this string value, int byteLength, string tailString)
        {
            if (value.IsNullOrEmpty()) return value;
            tailString = tailString ?? "";
            var tailLength = tailString.Select(s => (int)s > 255 ? 2 : 1).Sum();

            if (tailLength > 0) byteLength -= tailLength;
            if (byteLength < 1) throw new ArgumentException("截取长度过短，不应该再存在省略字符串");

            var currentLength = 0;
            var wordPosition = 0;
            while (currentLength < byteLength && wordPosition < value.Length) currentLength += value[wordPosition++] > 255 ? 2 : 1;
            if (wordPosition == value.Length) return value;
            else return value.Substring(0, wordPosition) + tailString;
        }

        /// <summary>
        /// 确认字符串是以指定字符串结尾的
        /// </summary>
        /// <param name="value">字符串</param>
        /// <param name="ending">结尾</param>
        /// <returns></returns>
        public static string EnsureEndWith(this string value, string ending)
        {
            if (value == null || value.EndsWith(ending)) return value;
            return value + ending;
        }

        /// <summary>
        /// 获得指定字符串的字节长度
        /// </summary>
        /// <param name="value">值</param>
        /// <returns><see cref="T:System.Int32"/></returns>
        public static int GetByteCount(this string value)
        {
            return GetByteCount(value, Encoding.Unicode);
        }

        /// <summary>
        /// 获得指定字符串的字节长度
        /// </summary>
        /// <param name="value">值</param>
        /// <param name="encoding">编码</param>
        /// <returns><see cref="T:System.Int32"/></returns>
        public static int GetByteCount(this string value, Encoding encoding)
        {
            if (value.IsNullOrEmpty()) return 0;
            return encoding.GetByteCount(value);
        }

        /// <summary>
        /// 转换为字节数组
        /// </summary>
        /// <param name="value">字符串值</param>
        /// <returns>结果字节数组</returns>
        public static byte[] ToBytes(this string value)
        {
            return ToBytes(value, null);
        }
        /// <summary>
        /// 转换为字节数组
        /// </summary>
        /// <param name="value">字符串值</param>
        /// <param name="encoding">使用的编码</param>
        /// <returns>结果字节数组</returns>
        public static byte[] ToBytes(this string value, Encoding encoding)
        {
            return value.IsNullOrEmpty() ? new byte[] { } :
                (encoding ?? Encoding.Unicode).GetBytes(value);
        }

        /// <summary>
        /// 返回指定的字符串中是否包含另外一个子字符串
        /// </summary>
        /// <param name="str">字符串</param>
        /// <param name="key">关键字</param>
        /// <param name="comparison">比较方式</param>
        /// <returns>包含为true， 否则为false</returns>
        public static bool Contains(this string str, string key, StringComparison comparison)
        {
            return str.IndexOf(key, comparison) != -1;
        }

        /// <summary>
        /// 按照标签分割并枚举
        /// </summary>
        /// <param name="text">文本</param>
        /// <param name="startTag">开始标签</param>
        /// <param name="endTag">结束标签</param>
        /// <param name="startPos">开始位置。默认为0</param>
        /// <returns>符合要求的代码片段</returns>
        public static IEnumerable<string> SplitByTag(this string text, string startTag, string endTag, int startPos = 0)
        {
            var index = 0;
            while ((index = text.IndexOf(startTag, startPos, StringComparison.OrdinalIgnoreCase)) != -1)
            {
                var endIndex = text.IndexOf(endTag, index + startTag.Length, StringComparison.OrdinalIgnoreCase);
                if (endIndex == -1) yield break;

                var str = text.Substring(index, endIndex - index + endTag.Length);
                startPos = endIndex + endTag.Length + 1;
                yield return str;
            }
        }

        /// <summary>
        /// 对字符串进行正则表达式匹配，并返回所有匹配的字符串数组
        /// </summary>
        /// <param name="text">字符串</param>
        /// <param name="pattern">正则表达式模式</param>
        /// <param name="options">选项</param>
        /// <returns>如果匹配失败，则返回false</returns>
        public static string[] RegMatch(this string text, string pattern, RegexOptions options = RegexOptions.IgnoreCase)
        {
            var m = Regex.Match(text, pattern, options);
            if (!m.Success) return null;

            return m.Groups.Cast<Group>().Select(s => s.Value).ToArray();
        }

        /// <summary>
        /// 对字符串进行正则表达式匹配，并返回所有匹配的字符串数组
        /// </summary>
        /// <param name="text">字符串</param>
        /// <param name="pattern">正则表达式模式</param>
        /// <param name="options">选项</param>
        /// <returns>如果匹配失败，则返回false</returns>
        public static List<string[]> RegMatches(this string text, string pattern, RegexOptions options = RegexOptions.IgnoreCase)
        {
            var m = Regex.Matches(text, pattern, options);

            return m.Cast<Match>().Select(s => s.Groups.Cast<Group>().Select(x => x.Value).ToArray()).ToList();
        }

        /// <summary>
        /// 在字符串中搜索指定的特征字符串并截取其中的一段。
        /// </summary>
        /// <param name="text">源字符串</param>
        /// <param name="beginTag">开始特征字符串</param>
        /// <param name="includeTag">是否包含标签本身，默认为 <see langword="true" /></param>
        /// <param name="endTag">结束特征字符串</param>
        /// <param name="beginIndex">开始索引</param>
        /// <param name="comparison">比较类型</param>
        /// <returns></returns>
        public static string SearchStringTag(this string text, string beginTag, string endTag, int beginIndex = 0, bool includeTag = true, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            return SearchStringTag(text, beginTag, endTag, ref beginIndex, includeTag, comparison);
        }

        /// <summary>
        /// 在字符串中搜索指定的特征字符串并截取其中的一段。
        /// </summary>
        /// <param name="text">源字符串</param>
        /// <param name="beginTag">开始特征字符串</param>
        /// <param name="endTag">结束特征字符串</param>
        /// <param name="beginIndex">开始索引。当搜索完成后，将会指向匹配结束后的下一个位置</param>
        /// <param name="includeTag">是否包含标签本身，默认为 <see langword="true" /></param>
        /// <param name="comparison">比较类型</param>
        /// <returns></returns>
        public static string SearchStringTag(this string text, string beginTag, string endTag, ref int beginIndex, bool includeTag = true, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            if (string.IsNullOrEmpty(text) || beginIndex >= text.Length)
                return string.Empty;

            var startIndex = beginIndex;
            var endIndex = text.Length;
            var tagStartEmpty = beginTag.IsNullOrEmpty();
            var tagEndEmpty = endTag.IsNullOrEmpty();

            if (!tagStartEmpty)
                startIndex = text.IndexOf(beginTag, startIndex, comparison);
            if (startIndex == -1)
                return string.Empty;

            if (!tagEndEmpty)
                endIndex = text.IndexOf(endTag, startIndex + (!tagStartEmpty ? beginTag.Length + 1 : 1), comparison);
            if (endIndex == -1)
                return string.Empty;

            beginIndex = endIndex + (!tagEndEmpty ? endTag.Length + 1 : 1);

            return text.Substring(
                                 startIndex + (includeTag || tagStartEmpty ? 0 : beginTag.Length),
                                endIndex - startIndex - (includeTag || tagStartEmpty ? 0 : beginTag.Length) + (tagEndEmpty || !includeTag ? 0 : endTag.Length)
                );
        }


        #endregion

        #region 流操作



        #endregion 

        #region ToInt32

        /// <summary>
        /// 将字符串转换为Int值，如果转换失败，则返回0
        /// </summary>
        /// <param name="value">字符串</param>
        /// <param name="defaultValue">如果转换失败,则返回的默认值</param>
        /// <returns>转换后的 <see cref="System.Int32"/></returns>
        public static int ToInt32(this string value, int defaultValue = 0, NumberStyles style = NumberStyles.Any, IFormatProvider provider = null)
        {
            int temp;
            return int.TryParse(value, style, provider, out temp) ? temp : defaultValue;
        }

        /// <summary>
        /// 将字符串转换为Int值，如果转换失败，则返回null
        /// </summary>
        /// <param name="value">字符串</param>
        /// <returns>转换后的 <see cref="System.Int32"/></returns>
        public static int? ToInt32Nullable(this string value, NumberStyles style = NumberStyles.Any, IFormatProvider provider = null)
        {
            int temp;
            return int.TryParse(value, style, provider, out temp) ? (int?)temp : null;
        }

        static char[] StringSpliter = new char[] { ',', '|', '\\', '/', ':', ';', '_', '#', '$', '%', '@', '!', '^', '&', '*' };

        /// <summary>
        /// 将字符串分割为整数数组
        /// </summary>
        /// <param name="value">要分割的字符串</param>
        /// <returns>返回最终的 <see cref="System.Int32"/>数组</returns>
        public static int[] SplitAsIntArray(this string value, NumberStyles style = NumberStyles.Any, IFormatProvider provider = null)
        {
            if (string.IsNullOrEmpty(value)) return new int[0];
            return value.Split(StringSpliter, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.ToInt32(style: style, provider: provider)).ToArray();
        }

        #endregion

        #region ToInt64

        /// <summary>
        /// 将字符串转换为Int值
        /// </summary>
        /// <param name="value">字符串</param>
        /// <param name="defaultValue">如果转换失败,则返回的默认值</param>
        /// <returns>转换后的 <see cref="System.Int64"/></returns>
        public static long ToInt64(this string value, long defaultValue = 0, NumberStyles style = NumberStyles.Any, IFormatProvider provider = null)
        {
            long temp;
            return long.TryParse(value, style, provider, out temp) ? temp : defaultValue;
        }

        /// <summary>
        /// 将字符串转换为Int值
        /// </summary>
        /// <param name="value">字符串</param>
        /// <returns>转换后的 <see cref="System.Int64"/></returns>
        public static long? ToInt64Nullable(this string value, NumberStyles style = NumberStyles.Any, IFormatProvider provider = null)
        {
            long temp;
            return long.TryParse(value, style, provider, out temp) ? (long?)temp : null;
        }

        #endregion

        #region ToSingle

        /// <summary>
        /// 转换字符串为浮点数.如果转换失败,则返回指定的默认值
        /// </summary>
        /// <param name="value">要转换的字符串</param>
        /// <param name="defaultValue">如果转换失败,则返回的默认值</param>
        /// <returns>转换后的 <see cref="System.Single"/></returns>
        public static float ToSingle(this string value, float defaultValue = 0.0F, NumberStyles style = NumberStyles.Any, IFormatProvider provider = null)
        {
            float temp;
            return float.TryParse(value, style, provider, out temp) ? temp : defaultValue;
        }

        /// <summary>
        /// 转换字符串为浮点数.如果转换失败,则返回null
        /// </summary>
        /// <param name="value">要转换的字符串</param>
        /// <returns>转换后的 <see cref="System.Single"/></returns>
        public static float? ToSingleNullable(this string value, NumberStyles style = NumberStyles.Any, IFormatProvider provider = null)
        {
            float temp;
            return float.TryParse(value, style, provider, out temp) ? (float?)temp : null;
        }

        #endregion

        #region ToDateTime

        /// <summary>
        /// 转换字符串为日期时间.如果转换失败,则返回指定的默认值
        /// </summary>
        /// <param name="value">要转换的字符串</param>
        /// <param name="defaultValue">如果转换失败,则返回的默认值</param>
        /// <param name="formatProvider">格式</param>
        /// <param name="styles"></param>
        /// <returns>
        /// 转换后的 <see cref="System.DateTime"/>
        /// </returns>
        public static DateTime ToDateTime(this string value, DateTime defaultValue, IFormatProvider formatProvider = null, DateTimeStyles styles = DateTimeStyles.None)
        {
            DateTime temp;
            return DateTime.TryParse(value, formatProvider, styles, out temp) ? temp : defaultValue;
        }


        /// <summary>
        /// 转换字符串为日期时间.如果转换失败,则返回指定的默认值
        /// </summary>
        /// <param name="value">要转换的字符串</param>
        /// <param name="defaultValue">如果转换失败,则返回的默认值</param>
        /// <returns>转换后的 <see cref="System.DateTime"/></returns>
        public static DateTime ToDateTimeExact(this string value, DateTime defaultValue, string format, IFormatProvider formatProvider = null, DateTimeStyles styles = DateTimeStyles.None)
        {
            DateTime temp;
            return DateTime.TryParseExact(value, format, formatProvider, styles, out temp) ? temp : defaultValue;
        }


        /// <summary>
        /// 转换字符串为日期时间.如果转换失败,则返回指定的默认值
        /// </summary>
        /// <param name="value">要转换的字符串</param>
        /// <param name="defaultValue">如果转换失败,则返回的默认值</param>
        /// <returns>转换后的 <see cref="System.DateTime"/></returns>
        public static DateTime ToDateTimeExact(this string value, DateTime defaultValue, string[] formats, IFormatProvider formatProvider = null, DateTimeStyles styles = DateTimeStyles.None)
        {
            DateTime temp;
            return DateTime.TryParseExact(value, formats, formatProvider, styles, out temp) ? temp : defaultValue;
        }


        /// <summary>
        /// 转换字符串为日期时间.如果转换失败,则返回指定的默认值
        /// </summary>
        /// <param name="value">要转换的字符串</param>
        /// <returns>转换后的 <see cref="System.DateTime"/></returns>
        public static DateTime ToDateTime(this string value, IFormatProvider formatProvider = null, DateTimeStyles styles = DateTimeStyles.None)
        {
            DateTime temp;
            return DateTime.TryParse(value, formatProvider, styles, out temp) ? temp : DateTime.MinValue;
        }

        /// <summary>
        /// 转换字符串为日期时间.如果转换失败,则返回指定的默认值
        /// </summary>
        /// <param name="value">要转换的字符串</param>
        /// <returns>转换后的 <see cref="System.DateTime"/></returns>
        public static DateTime ToDateTimeExact(this string value, string[] format = null, IFormatProvider formatProvider = null, DateTimeStyles styles = DateTimeStyles.None)
        {
            DateTime temp;
            return DateTime.TryParseExact(value, format, formatProvider, styles, out temp) ? temp : DateTime.MinValue;
        }

        /// <summary>
        /// 转换字符串为日期时间.如果转换失败,则返回 <see cref="F:System.DataTime.MinValue"/>
        /// </summary>
        /// <param name="value">要转换的字符串</param>
        /// <returns>转换后的 <see cref="System.DateTime"/></returns>
        public static DateTime ToDateTimeExact(this string value, string format, IFormatProvider formatProvider = null, DateTimeStyles styles = DateTimeStyles.None)
        {
            DateTime temp;
            return DateTime.TryParseExact(value, format, formatProvider, styles, out temp) ? temp : DateTime.MinValue;
        }

        /// <summary>
        /// 转换字符串为日期时间.如果转换失败,则返回null
        /// </summary>
        /// <param name="value">要转换的字符串</param>
        /// <returns>转换后的 <see cref="System.DateTime"/></returns>
        public static DateTime? ToDateTimeNullable(this string value, IFormatProvider formatProvider = null, DateTimeStyles styles = DateTimeStyles.None)
        {
            DateTime temp;
            return DateTime.TryParse(value, out temp) ? (DateTime?)temp : null;
        }


        /// <summary>
        /// 转换字符串为日期时间.如果转换失败,则返回null
        /// </summary>
        /// <param name="value">要转换的字符串</param>
        /// <returns>转换后的 <see cref="System.DateTime"/></returns>
        public static DateTime? ToDateTimeNullableExact(this string value, string format, IFormatProvider formatProvider = null, DateTimeStyles styles = DateTimeStyles.None)
        {
            DateTime temp;
            return DateTime.TryParseExact(value, format, formatProvider, styles, out temp) ? (DateTime?)temp : null;
        }

        /// <summary>
        /// 转换字符串为日期时间.如果转换失败,则返回null
        /// </summary>
        /// <param name="value">要转换的字符串</param>
        /// <returns>转换后的 <see cref="System.DateTime"/></returns>
        public static DateTime? ToDateTimeNullableExact(this string value, string[] formats, IFormatProvider formatProvider = null, DateTimeStyles styles = DateTimeStyles.None)
        {
            DateTime temp;
            return DateTime.TryParseExact(value, formats, formatProvider, styles, out temp) ? (DateTime?)temp : null;
        }

#if NET40

		/// <summary>
		/// 将字符串分析为可空Timespan
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static TimeSpan? ToTimeSpanNullable(this string value, IFormatProvider formatProvider = null)
		{
			TimeSpan ts;
			if (TimeSpan.TryParse(value, formatProvider, out ts))
				return ts;

			return null;
		}


		/// <summary>
		/// 将字符串分析为可空Timespan
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static TimeSpan? ToTimeSpanNullableExact(this string value, string format, IFormatProvider formatProvider = null, TimeSpanStyles styles = TimeSpanStyles.None)
		{
			TimeSpan ts;
			if (TimeSpan.TryParseExact(value, format, formatProvider, styles, out ts))
				return ts;

			return null;
		}

		/// <summary>
		/// 将字符串分析为可空Timespan
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static TimeSpan? ToTimeSpanNullableExact(this string value, string[] format, IFormatProvider formatProvider = null, TimeSpanStyles styles = TimeSpanStyles.None)
		{
			TimeSpan ts;
			if (TimeSpan.TryParseExact(value, format, formatProvider, styles, out ts))
				return ts;

			return null;
		}


		/// <summary>
		/// 将字符串分析为Timespan
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static TimeSpan ToTimeSpan(this string value, TimeSpan timeSpan, IFormatProvider formatProvider = null)
		{
			TimeSpan ts;
			if (TimeSpan.TryParse(value, formatProvider, out ts))
				return ts;

			return timeSpan;
		}


		/// <summary>
		/// 将字符串分析为Timespan
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static TimeSpan ToTimeSpanExact(this string value, TimeSpan timeSpan, string format, IFormatProvider formatProvider = null, TimeSpanStyles styles = TimeSpanStyles.None)
		{
			TimeSpan ts;
			if (TimeSpan.TryParseExact(value, format, formatProvider, styles, out ts))
				return ts;

			return timeSpan;
		}

		/// <summary>
		/// 将字符串分析为Timespan
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static TimeSpan ToTimeSpanExact(this string value, TimeSpan timeSpan, string[] format, IFormatProvider formatProvider = null, TimeSpanStyles styles = TimeSpanStyles.None)
		{
			TimeSpan ts;
			if (TimeSpan.TryParseExact(value, format, formatProvider, styles, out ts))
				return ts;

			return timeSpan;
		}


		/// <summary>
		/// 将字符串分析为Timespan
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static TimeSpan ToTimeSpan(this string value, IFormatProvider formatProvider = null)
		{
			TimeSpan ts;
			if (TimeSpan.TryParse(value, formatProvider, out ts))
				return ts;

			return TimeSpan.Zero;
		}


		/// <summary>
		/// 将字符串分析为Timespan
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static TimeSpan ToTimeSpanExact(this string value, string format, IFormatProvider formatProvider = null, TimeSpanStyles styles = TimeSpanStyles.None)
		{
			TimeSpan ts;
			if (TimeSpan.TryParseExact(value, format, formatProvider, styles, out ts))
				return ts;

			return TimeSpan.Zero;
		}

		/// <summary>
		/// 将字符串分析为Timespan
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static TimeSpan ToTimeSpanExact(this string value, string[] format, IFormatProvider formatProvider = null, TimeSpanStyles styles = TimeSpanStyles.None)
		{
			TimeSpan ts;
			if (TimeSpan.TryParseExact(value, format, formatProvider, styles, out ts))
				return ts;

			return TimeSpan.Zero;
		}
#endif


        #endregion

        #region ToDouble

        /// <summary>
        /// 转换字符串为双精度数.如果转换失败,则返回 0.0
        /// </summary>
        /// <param name="value">要转换的字符串</param>
        /// <param name="defaultValue">默认值，默认为 0.0</param>
        /// <param name="styles">分析的格式</param>
        /// <param name="formatProvider">区域信息</param>
        /// <returns>转换后的 <see cref="System.Double"/></returns>
        public static double ToDouble(this string value, double defaultValue = 0.0, NumberStyles styles = NumberStyles.Any, IFormatProvider formatProvider = null)
        {
            double temp;
            return double.TryParse(value, styles, formatProvider, out temp) ? temp : defaultValue;
        }


        /// <summary>
        /// 转换字符串为双精度数.如果转换失败,则返回指定的默认值
        /// </summary>
        /// <param name="value">要转换的字符串</param>
        /// <returns>转换后的 <see cref="System.Double"/></returns>
        public static double? ToDoubleNullable(this string value, NumberStyles styles = NumberStyles.Any, IFormatProvider formatProvider = null)
        {
            double temp;
            return double.TryParse(value, styles, formatProvider, out temp) ? (double?)temp : null;
        }

        #endregion

        #region ToDecimal

        /// <summary>
        /// 转换字符串为双精度数.如果转换失败,则返回指定的默认值
        /// </summary>
        /// <param name="value">要转换的字符串</param>
        /// <param name="defaultValue">如果转换失败,则返回的默认值</param>
        /// <returns>转换后的 <see cref="System.Double"/></returns>
        public static decimal ToDecimal(this string value, decimal defaultValue = 0m, NumberStyles styles = NumberStyles.Any, IFormatProvider formatProvider = null)
        {
            decimal temp;
            return decimal.TryParse(value, styles, formatProvider, out temp) ? temp : defaultValue;
        }

        /// <summary>
        /// 转换字符串为双精度数.如果转换失败,则返回 0.0
        /// </summary>
        /// <param name="value">要转换的字符串</param>
        /// <returns>转换后的 <see cref="System.Double"/></returns>
        public static decimal? ToDecimalNullable(this string value, NumberStyles styles = NumberStyles.Any, IFormatProvider formatProvider = null)
        {
            decimal temp;
            return decimal.TryParse(value, styles, formatProvider, out temp) ? (decimal?)temp : null;
        }

        #endregion

        #region ToPoint

        /// <summary>
        /// 将字符串转换为坐标点格式
        /// </summary>
        /// <param name="location">字符串</param>
        /// <returns><see cref="T:System.Drawing.Point"/></returns>
        public static System.Drawing.Point ParseToPoint(this string location)
        {
            if (string.IsNullOrEmpty(location))
                return System.Drawing.Point.Empty;

            var pts = location.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (pts.Length < 2) return System.Drawing.Point.Empty;

            return new System.Drawing.Point(pts[0].ToInt32(), pts[1].ToInt32());
        }

        #endregion

        #region 对象扩展

        /// <summary>
        /// 转换为文件夹对象
        /// </summary>
        /// <param name="folder">文件夹路径</param>
        /// <returns>对应的文件夹信息对象</returns>
        public static System.IO.DirectoryInfo AsDirectoryInfo(this string folder)
        {
            return new System.IO.DirectoryInfo(folder);
        }

        /// <summary>
        /// 转换为文件信息对象
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns><see cref="T:System.IO.FileInfo"/></returns>
        public static System.IO.FileInfo AsFileInfo(this string filePath)
        {
            return new System.IO.FileInfo(filePath);
        }

        #endregion

        #region ToByte[]

        /// <summary>
        /// 将Base64格式的字符串转换为字节数组
        /// </summary>
        /// <param name="base64String">要转换的Base64字符串</param>
        /// <returns>字节数组</returns>
        public static byte[] ConvertBase64ToBytes(this string base64String)
        {
            if (base64String.IsNullOrEmpty()) return null;

            return Convert.FromBase64String(base64String);
        }


        /// <summary>
        /// 压缩数据组
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static byte[] Compress(this string source)
        {
            using (var ms = new System.IO.MemoryStream())
            using (var gzip = new System.IO.Compression.GZipStream(ms, System.IO.Compression.CompressionMode.Compress))
            using (var sw = new System.IO.StreamWriter(gzip, System.Text.Encoding.UTF8))
            {
                sw.Write(source);
                sw.Dispose();
                return ms.ToArray();
            }
        }

        /// <summary>
        /// 解压缩数据组
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string DecompressAsString(this byte[] source)
        {
            using (var ms = new System.IO.MemoryStream(source))
            using (var gzip = new System.IO.Compression.GZipStream(ms, System.IO.Compression.CompressionMode.Decompress))
            using (var sr = new System.IO.StreamReader(gzip, System.Text.Encoding.UTF8, true))
            {
                return sr.ReadToEnd();
            }
        }

        /// <summary>
        /// 将HEX字符串转换为对应的字节数组
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static byte[] ConvertHexStringToBytes(this string source)
        {
            if (source.IsNullOrEmpty())
                return new byte[0];

            var matches = Regex.Matches(source, @"[a-f\d]{2}", RegexOptions.IgnoreCase);
            return matches.Cast<Match>().Select(s => (byte)((s.Value[0].ToHexByte() << 4) + s.Value[0].ToHexByte())).ToArray();
        }

        #endregion

        #region 其它对象到字符串转换

        /// <summary>
        /// 将坐标点转换为字符串格式
        /// </summary>
        /// <param name="point">坐标点位置</param>
        /// <returns>
        /// 字符串格式
        /// </returns>
        public static string ToStringFormat(this System.Drawing.Point point)
        {
            return point.X + "," + point.Y;
        }

        #endregion

        #region MD5

        /// <summary>
        /// 计算指定字符串的MD5值
        /// </summary>
        /// <param name="key">要计算Hash的字符串</param>
        /// <returns>字符串的Hash</returns>
        public static string MD5(this string key)
        {
            return key.MD5(System.Text.Encoding.UTF8);
        }

        /// <summary>
        /// 计算指定字符串的MD5值
        /// </summary>
        /// <param name="key">要计算Hash的字符串</param>
        /// <param name="encoding">计算Hash的编码方法</param>
        /// <returns>字符串的Hash</returns>
        public static string MD5(this string key, string encoding)
        {
            return key.MD5(System.Text.Encoding.GetEncoding(encoding));
        }

        /// <summary>
        /// 计算指定字符串的MD5值
        /// </summary>
        /// <param name="key">要计算Hash的字符串</param>
        /// <param name="encoding">计算Hash的编码方法</param>
        /// <returns>字符串的Hash</returns>
        public static string MD5(this string key, System.Text.Encoding encoding)
        {
            if (key == null) throw new ArgumentNullException();

            var md5 = System.Security.Cryptography.MD5.Create();
            var has = md5.ComputeHash(encoding.GetBytes(key));
            return BitConverter.ToString(has).Replace("-", "").ToUpper();
        }

        #endregion

        #region Base64编码解码

        /// <summary>
        /// Base64编码（UTF-8编码）
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string Base64Encode(this string data)
        {
            return Base64Encode(data, System.Text.Encoding.UTF8);
        }
        /// <summary>
        /// Base64编码
        /// </summary>
        /// <param name="data"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string Base64Encode(this string data, System.Text.Encoding encoding)
        {
            if (data == null) throw new ArgumentNullException();
            try
            {
                byte[] encData_byte = new byte[data.Length];
                encData_byte = encoding.GetBytes(data);
                string encodedData = Convert.ToBase64String(encData_byte);
                return encodedData;
            }
            catch (Exception e)
            {
                throw new Exception("Error in base64Encode" + e.Message);
            }
        }


        /// <summary>
        /// Base64解码（UTF-8编码）
        /// </summary>
        /// <param name="base64String"></param>
        /// <returns></returns>
        public static string Base64Decode(this string base64String)
        {
            return Base64Decode(base64String, System.Text.Encoding.UTF8);
        }
        /// <summary>
        /// Base64解码
        /// </summary>
        /// <param name="base64String"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string Base64Decode(this string base64String, System.Text.Encoding encoding)
        {
            try
            {
                byte[] todecode_byte = Convert.FromBase64String(base64String);
                int charCount = encoding.GetCharCount(todecode_byte, 0, todecode_byte.Length);
                char[] decoded_char = new char[charCount];
                encoding.GetChars(todecode_byte, 0, todecode_byte.Length, decoded_char, 0);
                return new String(decoded_char);
            }
            catch (Exception e)
            {
                throw new Exception("Error in base64Decode" + e.Message);
            }
        }
        #endregion

        #region 16进制转化
        /// <summary>
        /// 按字符十六进制字符编码
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string Base16Encode(this string str)
        {
            StringBuilder ret = new StringBuilder();
            foreach (byte b in Encoding.Default.GetBytes(str))
            {
                ret.AppendFormat("{0:X2}", b);
            }
            return ret.ToString();
        }
        /// <summary>
        /// 按字符十六进制字符解码
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string Base16Decode(this string str)
        {
            try
            {
                byte[] inputByteArray = new byte[str.Length / 2];
                for (int x = 0; x < str.Length / 2; x++)
                {
                    int i = (Convert.ToInt32(str.Substring(x * 2, 2), 16));
                    inputByteArray[x] = (byte)i;
                }
                return Encoding.Default.GetString(inputByteArray);
            }
            catch (Exception ee)
            {
                return null;
            }
        }
        #endregion

        #region AES加密解密方法
        const string _iKey = @")O[N-]6*^NKI,YF}+efc{}JK#JH8>Z'e9M";
        const string _iIV = @"L+\~fUF($#:KL*C4,I+)bkf";
        /// <summary>
        /// AES加密 向量 和 秘钥缓存
        /// </summary>
        static Dictionary<string, string> _keyCache = new Dictionary<string, string>();

        /// <summary>
        /// 使用默认密钥加密
        /// </summary>
        /// <param name="plainStr">明文字符串</param>
        /// <returns></returns>
        public static string AESEncrypt(this string plainStr)
        {
            return AESEncrypt(plainStr, _iKey, _iIV);
        }
        /// <summary>
        /// AES加密
        /// </summary>
        /// <param name="plainStr">明文字符串</param>
        /// <param name="key">秘钥</param>
        /// <param name="iv">向量</param>
        /// <returns>密文</returns>
        public static string AESEncrypt(this string plainStr, string key, string iv)
        {
            if (string.IsNullOrEmpty(plainStr))
            {
                return string.Empty;
            }
            string encrypt = new AES(key, iv).AESEncrypt(plainStr);//加密
            if (string.IsNullOrEmpty(encrypt))
            {
                return string.Empty;
            }
            else
            {
                return encrypt.Base16Encode();//转换
            }
        }

        /// <summary>
        /// 使用默认密钥解密
        /// </summary>
        /// <param name="decryptStr">密文字符串</param>
        /// <returns>明文</returns>
        public static string AESDecrypt(this string decryptStr)
        {
            return AESDecrypt(decryptStr, _iKey, _iIV);
        }
        /// <summary>
        /// AES解密
        /// </summary>
        /// <param name="decryptStr">密文字符串</param>
        /// <param name="key">蜜钥</param>
        /// <param name="iv">向量</param> 
        /// <returns>明文</returns>
        public static string AESDecrypt(this string decryptStr, string key, string iv)
        {
            decryptStr = decryptStr.Base16Decode();//16进制转换
            if (string.IsNullOrEmpty(decryptStr))
            {
                return string.Empty;
            }
            string decrypt = new AES(key, iv).AESDecrypt(decryptStr);//解密
            if (string.IsNullOrEmpty(decrypt))
            {
                return string.Empty;
            }
            else
            {
                return decrypt;         //返回
            }
        }

        #endregion

        #region SHA1
        /// <summary>
        /// 计算指定字符串的SHA1值(全部大写)
        /// </summary>
        /// <param name="key">要计算Hash的字符串</param>
        /// <returns>字符串的Hash</returns>
        public static string SHA1(this string key)
        {
            return key.SHA1(System.Text.Encoding.UTF8);
        }

        /// <summary>
        /// 计算指定字符串的SHA1值(全部大写)
        /// </summary>
        /// <param name="key">要计算Hash的字符串</param>
        /// <param name="encoding">计算Hash的编码方法</param>
        /// <returns>字符串的Hash</returns>
        public static string SHA1(this string key, string encoding)
        {
            return key.SHA1(System.Text.Encoding.GetEncoding(encoding));
        }

        /// <summary>
        /// 计算指定字符串的SHA1值(全部大写)
        /// </summary>
        /// <param name="key">要计算Hash的字符串</param>
        /// <param name="encoding">计算Hash的编码方法</param>
        /// <returns>字符串的Hash</returns>
        public static string SHA1(this string key, System.Text.Encoding encoding)
        {
            if (key == null) throw new ArgumentNullException();

            var sha1 = System.Security.Cryptography.SHA1.Create();
            var has = sha1.ComputeHash(encoding.GetBytes(key));
            return BitConverter.ToString(has).Replace("-", "").ToUpper();
        }
        #endregion

        #region HMAC-SHA1
        const string _sKey = @")O[N-]6*^NKI,YF}+efc{}JK#JH8>Z'e9M";
        /// <summary>
        /// HMAC-SHA1加密（使用默认秘钥）
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string HMACSHA1(this string data)
        {
            return HMACSHA1(data, _sKey);
        }
        /// <summary>
        /// HMACSHA1加密
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string HMACSHA1(this string data, string key)
        {
            HMACSHA1 hmacsha1 = new HMACSHA1();
            hmacsha1.Key = Encoding.ASCII.GetBytes(key);

            byte[] dataBuffer = System.Text.Encoding.ASCII.GetBytes(data);
            byte[] hashBytes = hmacsha1.ComputeHash(dataBuffer);

            return Convert.ToBase64String(hashBytes);
        }
        #endregion
    }
}
