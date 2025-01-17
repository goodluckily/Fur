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

using Fur.DatabaseAccessor;
using Fur.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// 数据库访问器服务拓展类
    /// </summary>
    [NonBeScan]
    public static class DatabaseAccessorServiceCollectionExtensions
    {
        /// <summary>
        /// 添加数据库上下文
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="configure">配置</param>
        /// <returns>服务集合</returns>
        public static IServiceCollection AddDatabaseAccessor(this IServiceCollection services, Action<IServiceCollection> configure = null)
        {
            // 添加数据库选项配置支持
            services.AddConfigurableOptions<DatabaseAccessorSettingsOptions>();

            // 配置数据库上下文
            configure?.Invoke(services);

            // 注册数据库上下文池
            services.TryAddScoped<IDbContextPool, DbContextPool>();

            // 注册 Sql 仓储
            services.TryAddScoped(typeof(ISqlRepository<>), typeof(SqlRepository<>));

            // 注册 Sql 非泛型仓储
            services.TryAddScoped<ISqlRepository, SqlRepository>();

            // 注册多数据库上下文仓储
            services.TryAddScoped(typeof(IRepository<,>), typeof(EFCoreRepository<,>));

            // 注册泛型仓储
            services.TryAddScoped(typeof(IRepository<>), typeof(EFCoreRepository<>));

            // 注册非泛型仓储
            services.TryAddScoped<IRepository, EFCoreRepository>();

            // 解析数据库上下文
            services.AddScoped(provider =>
            {
                DbContext dbContextResolve(Type locator)
                {
                    // 判断定位器是否绑定了数据库上下文
                    var isRegistered = Penetrates.DbContextLocators.TryGetValue(locator, out var dbContextType);
                    if (!isRegistered) throw new InvalidOperationException("The DbContext for locator binding was not found");

                    // 动态解析数据库上下文
                    return provider.GetService(dbContextType) as DbContext;
                }
                return (Func<Type, DbContext>)dbContextResolve;
            });

            // 注册 Sql 代理接口
            services.AddDispatchProxy<SqlDispatchProxy, ISqlDispatchProxy>();

            // 注册全局工作单元过滤器
            services.Configure<MvcOptions>(options => options.Filters.Add<UnitOfWorkFilter>());

            return services;
        }

        /// <summary>
        /// 注册数据库上下文
        /// </summary>
        /// <typeparam name="TDbContext">数据库上下文</typeparam>
        /// <typeparam name="TDbContextLocator">数据库上下文定位器</typeparam>
        /// <param name="services">服务提供器</param>
        public static IServiceCollection RegisterDbContext<TDbContext, TDbContextLocator>(this IServiceCollection services)
            where TDbContext : DbContext
            where TDbContextLocator : class, IDbContextLocator
        {
            // 将数据库上下文和定位器一一保存起来
            var isSuccess = Penetrates.DbContextLocators.TryAdd(typeof(TDbContextLocator), typeof(TDbContext));
            if (!isSuccess) throw new InvalidOperationException("The locator is bound to another DbContext");

            // 注册数据库上下文
            services.TryAddScoped<TDbContext>();

            return services;
        }
    }
}