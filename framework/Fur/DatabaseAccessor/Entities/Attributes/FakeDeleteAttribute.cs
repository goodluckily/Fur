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
using System;

namespace Fur.DatabaseAccessor
{
    /// <summary>
    /// 假删除/软删除
    /// </summary>
    [NonBeScan, AttributeUsage(AttributeTargets.Property)]
    public class FakeDeleteAttribute : Attribute
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="state"></param>
        public FakeDeleteAttribute(object state)
        {
            State = state;
        }

        /// <summary>
        /// 假删除/软删除状态
        /// </summary>
        public object State { get; set; }
    }
}