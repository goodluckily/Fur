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

using System;

namespace Fur.DependencyInjection
{
    /// <summary>
    /// 注册服务启动配置
    /// </summary>
    [NonBeScan, AttributeUsage(AttributeTargets.Class)]
    public class StartupOrderAttribute : Attribute
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="order"></param>
        public StartupOrderAttribute(int order)
        {
            Order = order;
        }

        /// <summary>
        /// 排序
        /// </summary>
        public int Order { get; set; }
    }
}