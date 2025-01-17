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
using Microsoft.AspNetCore.Authorization;
using System;

namespace Fur.Authorization
{
    /// <summary>
    /// 策略授权特性
    /// </summary>
    [NonBeScan, AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class AuthorizePolicyAttribute : AuthorizeAttribute
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="policies">多个策略</param>
        public AuthorizePolicyAttribute(params string[] policies)
        {
            Policies = policies;
        }

        /// <summary>
        /// 策略
        /// </summary>
        public string[] Policies
        {
            get => Policy[Penetrates.AuthorizePolicyPrefix.Length..].Split(',', StringSplitOptions.RemoveEmptyEntries);
            set => Policy = $"{Penetrates.AuthorizePolicyPrefix}${string.Join(',', value)}";
        }
    }
}