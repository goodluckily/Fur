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

using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Fur.DatabaseAccessor
{
    /// <summary>
    /// Sql 操作仓储接口
    /// </summary>
    public interface ISqlRepository : ISqlRepository<DbContextLocator>
    {
    }

    /// <summary>
    /// Sql 操作仓储接口
    /// </summary>
    /// <typeparam name="TDbContextLocator">数据库上下文定位器</typeparam>
    public interface ISqlRepository<TDbContextLocator>
        : ISqlExecutableRepository<TDbContextLocator>
        , ISqlQueryableRepository<TDbContextLocator>
        , IRepositoryDependency
       where TDbContextLocator : class, IDbContextLocator
    {
        /// <summary>
        /// 数据库操作对象
        /// </summary>
        DatabaseFacade Database { get; }

        /// <summary>
        /// 切换仓储
        /// </summary>
        /// <typeparam name="TChangeDbContextLocator">数据库上下文定位器</typeparam>
        /// <returns>仓储</returns>
        ISqlRepository<TChangeDbContextLocator> Change<TChangeDbContextLocator>()
            where TChangeDbContextLocator : class, IDbContextLocator;
    }
}