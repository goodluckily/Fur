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

namespace Fur.DataValidation
{
    /// <summary>
    /// 验证逻辑
    /// </summary>
    [NonBeScan]
    public enum ValidationPattern
    {
        /// <summary>
        /// 全部都要验证通过
        /// </summary>
        AllOfThem,

        /// <summary>
        /// 至少一个验证通过
        /// </summary>
        AtLeastOne
    }
}