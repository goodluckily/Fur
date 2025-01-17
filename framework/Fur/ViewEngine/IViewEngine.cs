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
using System.Threading.Tasks;

namespace Fur.ViewEngine
{
    /// <summary>
    /// 视图引擎接口
    /// </summary>
    public interface IViewEngine
    {
        /// <summary>
        /// 编译模板
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="content"></param>
        /// <param name="builderAction"></param>
        /// <returns></returns>
        IViewEngineCompiledTemplate<T> Compile<T>(string content, Action<IViewEngineCompilationOptionsBuilder> builderAction = null)
            where T : IViewEngineTemplate;

        /// <summary>
        /// 编译模板
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="content"></param>
        /// <param name="builderAction"></param>
        /// <returns></returns>
        Task<IViewEngineCompiledTemplate<T>> CompileAsync<T>(string content, Action<IViewEngineCompilationOptionsBuilder> builderAction = null)
            where T : IViewEngineTemplate;

        /// <summary>
        /// 编译模板
        /// </summary>
        /// <param name="content"></param>
        /// <param name="builderAction"></param>
        /// <returns></returns>
        IViewEngineCompiledTemplate Compile(string content, Action<IViewEngineCompilationOptionsBuilder> builderAction = null);

        /// <summary>
        /// 编译模板
        /// </summary>
        /// <param name="content"></param>
        /// <param name="builderAction"></param>
        /// <returns></returns>
        Task<IViewEngineCompiledTemplate> CompileAsync(string content, Action<IViewEngineCompilationOptionsBuilder> builderAction = null);
    }
}