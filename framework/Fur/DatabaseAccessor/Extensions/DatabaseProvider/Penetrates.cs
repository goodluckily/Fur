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
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace Fur.DatabaseAccessor
{
    /// <summary>
    /// 常量、公共方法配置类
    /// </summary>
    [NonBeScan]
    internal static class Penetrates
    {
        /// <summary>
        /// 数据库上下文定位器集合
        /// </summary>
        internal static readonly ConcurrentDictionary<Type, Type> DbContextLocators;

        /// <summary>
        /// 构造函数
        /// </summary>
        static Penetrates()
        {
            DbContextLocators = new ConcurrentDictionary<Type, Type>();
        }

        /// <summary>
        /// 获取数据库上下文连接字符串
        /// </summary>
        /// <typeparam name="TDbContext"></typeparam>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        internal static string GetDbContextConnectionString<TDbContext>(string connectionString = default)
            where TDbContext : DbContext
        {
            if (!string.IsNullOrEmpty(connectionString)) return connectionString;

            // 如果没有配置数据库连接字符串，那么查找特性
            var dbContextType = typeof(TDbContext);
            if (!dbContextType.IsDefined(typeof(DbContextAttribute), true)) return default;

            // 获取配置特性
            var dbContextAttribute = dbContextType.GetCustomAttribute<DbContextAttribute>(true);
            var connStr = dbContextAttribute.ConnectionString;

            if (string.IsNullOrEmpty(connStr)) return default;
            if (connStr.Contains(";")) return connStr;
            else return App.Configuration.GetConnectionString(connStr);
        }

        /// <summary>
        /// 配置 SqlServer 数据库上下文
        /// </summary>
        /// <param name="connectionString">数据库连接字符串</param>
        /// <param name="optionBuilder">数据库上下文选项构建器</param>
        /// <param name="interceptors">拦截器</param>
        /// <returns></returns>
        internal static Action<IServiceProvider, DbContextOptionsBuilder> ConfigureDbContext(string connectionString, Action<DbContextOptionsBuilder> optionBuilder, params IInterceptor[] interceptors)
        {
            return (serviceProvider, options) =>
            {
                if (App.HostEnvironment.IsDevelopment())
                {
                    options/*.UseLazyLoadingProxies()*/
                                .EnableDetailedErrors()
                                .EnableSensitiveDataLogging();
                }

                // 如果连接字符串不为空
                if (!string.IsNullOrEmpty(connectionString))
                {
                    optionBuilder.Invoke(options);
                }

                // 添加拦截器
                AddInterceptors(interceptors, options);

                //options.UseInternalServiceProvider(serviceProvider);
            };
        }

        /// <summary>
        /// 数据库数据库拦截器
        /// </summary>
        /// <param name="interceptors">拦截器</param>
        /// <param name="options"></param>
        private static void AddInterceptors(IInterceptor[] interceptors, DbContextOptionsBuilder options)
        {
            if (App.Settings.InjectMiniProfiler != true) return;

            // 添加拦截器
            var interceptorList = new List<IInterceptor>
            {
                new SqlConnectionProfilerInterceptor()
            };
            if (interceptors != null || interceptors.Length > 0)
            {
                interceptorList.AddRange(interceptors);
            }
            options.AddInterceptors(interceptorList.ToArray());
        }
    }
}