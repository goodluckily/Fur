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
using Fur.UnifyResult;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// 规范化结果服务拓展
    /// </summary>
    [NonBeScan]
    public static class UnifyResultServiceCollectionExtensions
    {
        /// <summary>
        /// 添加规范化结果服务
        /// </summary>
        /// <typeparam name="TUnifyResultProvider"></typeparam>
        /// <param name="mvcBuilder"></param>
        /// <returns></returns>
        public static IMvcBuilder AddUnifyResult<TResult, TUnifyResultProvider>(this IMvcBuilder mvcBuilder)
            where TResult : class, new()
            where TUnifyResultProvider : class, IUnifyResultProvider
        {
            // 添加规范化提供器
            mvcBuilder.Services.AddSingleton<IUnifyResultProvider, TUnifyResultProvider>();

            // 添加成功规范化结果
            mvcBuilder.AddMvcOptions(options =>
            {
                options.Filters.Add<SuccessUnifyResultFilter>();
                options.Filters.Add(new ProducesResponseTypeAttribute(typeof(TResult), 200));
            });

            return mvcBuilder;
        }
    }
}