﻿// -----------------------------------------------------------------------------
// Fur 是 .NET 5 平台下极易入门、极速开发的 Web 应用框架。
// Copyright © 2020 Fur, Baiqian Co.,Ltd.
//
// 框架名称：Fur
// 框架作者：百小僧
// 框架版本：1.0.0
// 源码地址：https://gitee.com/monksoul/Fur
// 开源协议：Apache-2.0（http://www.apache.org/licenses/LICENSE-2.0）
// -----------------------------------------------------------------------------

using Fur.DependencyInjection;
using System.Text;
using System.Threading.Tasks;

namespace Fur.ViewEngine
{
    /// <summary>
    /// 视图模板实现类
    /// </summary>
    [NonBeScan]
    public abstract class ViewEngineTemplate : IViewEngineTemplate
    {
        /// <summary>
        /// 字符串构建器
        /// </summary>
        private readonly StringBuilder stringBuilder = new StringBuilder();

        /// <summary>
        /// 特性后缀
        /// </summary>
        private string attributeSuffix;

        /// <summary>
        /// 视图模型
        /// </summary>
        public dynamic Model { get; set; }

        /// <summary>
        /// 插入字面量
        /// </summary>
        /// <param name="literal"></param>
        public virtual void WriteLiteral(string literal = null)
        {
            stringBuilder.Append(literal);
        }

        /// <summary>
        /// 插入对象
        /// </summary>
        /// <param name="obj"></param>
        public virtual void Write(object obj = null)
        {
            stringBuilder.Append(obj);
        }

        /// <summary>
        /// 插入属性
        /// </summary>
        /// <param name="name"></param>
        /// <param name="prefix"></param>
        /// <param name="prefixOffset"></param>
        /// <param name="suffix"></param>
        /// <param name="suffixOffset"></param>
        /// <param name="attributeValuesCount"></param>
        public virtual void BeginWriteAttribute(string name, string prefix, int prefixOffset, string suffix, int suffixOffset, int attributeValuesCount)
        {
            attributeSuffix = suffix;
            stringBuilder.Append(prefix);
        }

        /// <summary>
        /// 插入属性值
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="prefixOffset"></param>
        /// <param name="value"></param>
        /// <param name="valueOffset"></param>
        /// <param name="valueLength"></param>
        /// <param name="isLiteral"></param>
        public virtual void WriteAttributeValue(string prefix, int prefixOffset, object value, int valueOffset, int valueLength, bool isLiteral)
        {
            stringBuilder.Append(prefix);
            stringBuilder.Append(value);
        }

        /// <summary>
        /// 结束插入属性
        /// </summary>
        public virtual void EndWriteAttribute()
        {
            stringBuilder.Append(attributeSuffix);
            attributeSuffix = null;
        }

        /// <summary>
        /// 执行
        /// </summary>
        /// <returns></returns>
        public virtual Task ExecuteAsync()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// 返回结果
        /// </summary>
        /// <returns></returns>
        public virtual string Result()
        {
            return stringBuilder.ToString();
        }
    }

    /// <summary>
    /// 视图模板实现类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [NonBeScan]
    public abstract class ViewEngineTemplateBase<T> : ViewEngineTemplate
    {
        /// <summary>
        /// 强类型
        /// </summary>
        public new T Model { get; set; }
    }
}