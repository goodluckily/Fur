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
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace Fur.ViewEngine
{
    /// <summary>
    /// 匿名类型包装器
    /// </summary>
    [NonBeScan]
    public class AnonymousTypeWrapper : DynamicObject
    {
        /// <summary>
        /// 匿名模型
        /// </summary>
        private readonly object model;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="model"></param>
        public AnonymousTypeWrapper(object model)
        {
            this.model = model;
        }

        /// <summary>
        /// 获取成员信息
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var propertyInfo = model.GetType().GetProperty(binder.Name);

            if (propertyInfo == null)
            {
                result = null;
                return false;
            }

            result = propertyInfo.GetValue(model, null);

            if (result == null)
            {
                return true;
            }

            var type = result.GetType();

            if (result.IsAnonymous())
            {
                result = new AnonymousTypeWrapper(result);
            }

            if (type.IsArray)
            {
                result = ((IEnumerable<object>)result).Select(e => new AnonymousTypeWrapper(e)).ToList();
            }

            return true;
        }
    }
}