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

using Fur.ConfigurableOptions;
using Microsoft.Extensions.Configuration;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;

namespace Fur.SpecificationDocument
{
    /// <summary>
    /// 规范化文档配置选项
    /// </summary>
    [OptionsSettings("AppSettings:SpecificationDocumentSettings")]
    public sealed class SpecificationDocumentSettingsOptions : IConfigurableOptions<SpecificationDocumentSettingsOptions>
    {
        /// <summary>
        /// 文档标题
        /// </summary>
        public string DocumentTitle { get; set; }

        /// <summary>
        /// 默认分组名
        /// </summary>
        public string DefaultGroupName { get; set; }

        /// <summary>
        /// 启用授权支持
        /// </summary>
        public bool? EnableAuthorized { get; set; }

        /// <summary>
        /// 格式化为V2版本
        /// </summary>
        public bool? FormatAsV2 { get; set; }

        /// <summary>
        /// 配置规范化文档地址
        /// </summary>
        public string RoutePrefix { get; set; }

        /// <summary>
        /// 文档展开设置
        /// </summary>
        public DocExpansion? DocExpansionState { get; set; }

        /// <summary>
        /// XML 描述文件
        /// </summary>
        public string[] XmlComments { get; set; }

        /// <summary>
        /// 分组信息
        /// </summary>
        public SpecificationOpenApiInfo[] GroupOpenApiInfos { get; set; }

        /// <summary>
        /// 安全定义
        /// </summary>
        public SpecificationOpenApiSecurityScheme[] SecurityDefinitions { get; set; }

        /// <summary>
        /// 后期配置
        /// </summary>
        /// <param name="options"></param>
        public void PostConfigure(SpecificationDocumentSettingsOptions options, IConfiguration configuration)
        {
            options.DocumentTitle ??= "Specification Api Document";
            options.DefaultGroupName ??= "Default";
            options.FormatAsV2 ??= false;
            options.RoutePrefix ??= string.Empty;
            options.DocExpansionState ??= DocExpansion.List;
            XmlComments ??= new string[]
           {
                "Fur.Application",
                "Fur.Web.Entry",
                "Fur.Web.Core"
           };
            GroupOpenApiInfos ??= new SpecificationOpenApiInfo[]
            {
                new SpecificationOpenApiInfo()
                {
                    Group=options.DefaultGroupName
                }
            };
            SecurityDefinitions ??= Array.Empty<SpecificationOpenApiSecurityScheme>();
        }
    }
}