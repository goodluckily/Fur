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
using Fur.DynamicApiController;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Fur.SpecificationDocument
{
    /// <summary>
    /// 规范化文档构建器
    /// </summary>
    [NonBeScan]
    internal static class SpecificationDocumentBuilder
    {
        /// <summary>
        /// 规范化文档配置
        /// </summary>
        private static readonly SpecificationDocumentSettingsOptions _specificationDocumentSettings;

        /// <summary>
        /// 文档默认分组
        /// </summary>
        private static readonly IEnumerable<GroupOrder> _defaultGroups;

        /// <summary>
        /// 文档分组列表
        /// </summary>
        private static readonly IEnumerable<string> _groups;

        /// <summary>
        /// 带排序的分组名
        /// </summary>
        private static readonly Regex _groupOrderRegex;

        /// <summary>
        /// 构造函数
        /// </summary>
        static SpecificationDocumentBuilder()
        {
            // 载入配置
            _specificationDocumentSettings = App.GetOptions<SpecificationDocumentSettingsOptions>();

            // 初始化常量
            _groupOrderRegex = new Regex(@"@(?<order>[0-9]+$)");
            GetActionGroupsCached = new ConcurrentDictionary<MethodInfo, IEnumerable<GroupOrder>>();
            GetControllerGroupsCached = new ConcurrentDictionary<Type, IEnumerable<GroupOrder>>();
            GetGroupOpenApiInfoCached = new ConcurrentDictionary<string, SpecificationOpenApiInfo>();
            GetControllerTagCached = new ConcurrentDictionary<ControllerActionDescriptor, string>();
            GetActionTagCached = new ConcurrentDictionary<ApiDescription, string>();

            // 默认分组，支持多个逗号分割
            _defaultGroups = new List<GroupOrder> { ResolveGroupOrder(_specificationDocumentSettings.DefaultGroupName) };

            // 加载所有分组
            _groups = ReadGroups();
        }

        /// <summary>
        /// 构建Swagger全局配置
        /// </summary>
        /// <param name="swaggerOptions">Swagger 全局配置</param>
        internal static void Build(SwaggerOptions swaggerOptions)
        {
            // 生成V2版本
            swaggerOptions.SerializeAsV2 = _specificationDocumentSettings.FormatAsV2 == true;
        }

        /// <summary>
        /// Swagger 生成器构建
        /// </summary>
        /// <param name="swaggerGenOptions">Swagger 生成器配置</param>
        /// <param name="configure">自定义配置</param>
        internal static void BuildGen(SwaggerGenOptions swaggerGenOptions, Action<SwaggerGenOptions> configure = null)
        {
            // 创建分组文档
            CreateSwaggerDocs(swaggerGenOptions);

            // 加载分组控制器和动作方法列表
            LoadGroupControllerWithActions(swaggerGenOptions);

            // 配置 Swagger SchemaId
            ConfigureSchemaId(swaggerGenOptions);

            // 使用内部枚举定义模型，无需创建 SchemaId
            swaggerGenOptions.UseInlineDefinitionsForEnums();

            // 配置动作方法标签
            ConfigureTagsAction(swaggerGenOptions);

            // 加载注释描述文件
            LoadXmlComments(swaggerGenOptions);

            // 配置授权
            ConfigureSecurities(swaggerGenOptions);

            // 自定义配置
            configure?.Invoke(swaggerGenOptions);
        }

        /// <summary>
        /// Swagger UI 构建
        /// </summary>
        /// <param name="swaggerUIOptions"></param>
        internal static void BuildUI(SwaggerUIOptions swaggerUIOptions)
        {
            // 配置分组终点路由
            CreateGroupEndpoint(swaggerUIOptions);

            // 配置文档标题
            swaggerUIOptions.DocumentTitle = _specificationDocumentSettings.DocumentTitle;

            // 配置UI地址
            swaggerUIOptions.RoutePrefix = _specificationDocumentSettings.RoutePrefix;

            // 文档展开设置
            swaggerUIOptions.DocExpansion(_specificationDocumentSettings.DocExpansionState.Value);

            // 注入 MiniProfiler 组件
            InjectMiniProfilerPlugin(swaggerUIOptions);
        }

        /// <summary>
        /// 创建分组文档
        /// </summary>
        /// <param name="swaggerGenOptions">Swagger生成器对象</param>
        private static void CreateSwaggerDocs(SwaggerGenOptions swaggerGenOptions)
        {
            foreach (var group in _groups)
            {
                var groupOpenApiInfo = GetGroupOpenApiInfo(group) as OpenApiInfo;
                swaggerGenOptions.SwaggerDoc(group, groupOpenApiInfo);
            }
        }

        /// <summary>
        /// 加载分组控制器和动作方法列表
        /// </summary>
        /// <param name="swaggerGenOptions">Swagger 生成器配置</param>
        private static void LoadGroupControllerWithActions(SwaggerGenOptions swaggerGenOptions)
        {
            swaggerGenOptions.DocInclusionPredicate((currentGroup, apiDescription) =>
            {
                if (!apiDescription.TryGetMethodInfo(out var method)) return false;

                return GetActionGroups(method).Any(u => u.Group == currentGroup);
            });
        }

        /// <summary>
        ///  配置标签
        /// </summary>
        /// <param name="swaggerGenOptions"></param>
        private static void ConfigureTagsAction(SwaggerGenOptions swaggerGenOptions)
        {
            swaggerGenOptions.TagActionsBy(apiDescription =>
            {
                return new[] { GetActionTag(apiDescription) };
            });
        }

        /// <summary>
        /// 配置 Swagger SchemaId
        /// </summary>
        /// <param name="swaggerGenOptions">Swagger 生成器配置</param>
        private static void ConfigureSchemaId(SwaggerGenOptions swaggerGenOptions)
        {
            // 本地函数
            static string DefaultSchemaIdSelector(Type modelType)
            {
                if (!modelType.IsConstructedGenericType) return modelType.Name;

                var prefix = modelType.GetGenericArguments()
                    .Select(genericArg => DefaultSchemaIdSelector(genericArg))
                    .Aggregate((previous, current) => previous + current);

                // 通过 Of 拼接多个泛型
                return modelType.Name.Split('`').First() + "Of" + prefix;
            }

            // 调用本地函数
            swaggerGenOptions.CustomSchemaIds(modelType => DefaultSchemaIdSelector(modelType));
        }

        /// <summary>
        /// 加载注释描述文件
        /// </summary>
        /// <param name="swaggerGenOptions">Swagger 生成器配置</param>
        private static void LoadXmlComments(SwaggerGenOptions swaggerGenOptions)
        {
            var xmlComments = _specificationDocumentSettings.XmlComments;
            foreach (var xmlComment in xmlComments)
            {
                var assemblyXmlName = xmlComment.EndsWith(".xml") ? xmlComment : $"{xmlComment}.xml";
                var assemblyXmlPath = Path.Combine(AppContext.BaseDirectory, assemblyXmlName);
                if (File.Exists(assemblyXmlPath))
                {
                    swaggerGenOptions.IncludeXmlComments(assemblyXmlPath, true);
                }
            }
        }

        /// <summary>
        /// 配置授权
        /// </summary>
        /// <param name="swaggerGenOptions">Swagger 生成器配置</param>
        private static void ConfigureSecurities(SwaggerGenOptions swaggerGenOptions)
        {
            // 判断是否启用了授权
            if (_specificationDocumentSettings.EnableAuthorized != true || _specificationDocumentSettings.SecurityDefinitions.Length == 0) return;

            var openApiSecurityRequirement = new OpenApiSecurityRequirement();

            // 生成安全定义
            foreach (var securityDefinition in _specificationDocumentSettings.SecurityDefinitions)
            {
                // Id 必须定义
                if (string.IsNullOrEmpty(securityDefinition.Id)) continue;

                // 添加安全定义
                var openApiSecurityScheme = securityDefinition as OpenApiSecurityScheme;
                swaggerGenOptions.AddSecurityDefinition(securityDefinition.Id, openApiSecurityScheme);

                // 添加安全需求
                var securityRequirement = securityDefinition.Requirement;

                // C# 9.0 模式匹配新语法
                if (securityRequirement is { Scheme: { Reference: not null } })
                {
                    securityRequirement.Scheme.Reference.Id ??= securityDefinition.Id;
                    openApiSecurityRequirement.Add(securityRequirement.Scheme, securityRequirement.Accesses);
                }
            }

            // 添加安全需求
            if (openApiSecurityRequirement.Count > 0)
            {
                swaggerGenOptions.AddSecurityRequirement(openApiSecurityRequirement);
            }
        }

        /// <summary>
        /// 配置分组终点路由
        /// </summary>
        /// <param name="swaggerUIOptions"></param>
        private static void CreateGroupEndpoint(SwaggerUIOptions swaggerUIOptions)
        {
            foreach (var group in _groups)
            {
                var groupOpenApiInfo = GetGroupOpenApiInfo(group);

                swaggerUIOptions.SwaggerEndpoint($"/swagger/{group}/swagger.json", groupOpenApiInfo?.Title ?? group);
            }
        }

        /// <summary>
        /// 注入 MiniProfiler 插件
        /// </summary>
        /// <param name="swaggerUIOptions"></param>
        private static void InjectMiniProfilerPlugin(SwaggerUIOptions swaggerUIOptions)
        {
            if (App.Settings.InjectMiniProfiler != true) return;

            // 启用 MiniProfiler 组件
            var thisType = typeof(SpecificationDocumentBuilder);
            var thisAssembly = thisType.Assembly;

            // 自定义 Swagger 首页
            swaggerUIOptions.IndexStream = () => thisAssembly.GetManifestResourceStream($"{thisType.Namespace}.Assets.index-mini-profiler.html");
        }

        /// <summary>
        /// <see cref="GetControllerGroups(MethodInfo)"/> 缓存集合
        /// </summary>
        private static readonly ConcurrentDictionary<string, SpecificationOpenApiInfo> GetGroupOpenApiInfoCached;

        /// <summary>
        /// 获取分组配置信息
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        private static SpecificationOpenApiInfo GetGroupOpenApiInfo(string group)
        {
            var isCached = GetGroupOpenApiInfoCached.TryGetValue(group, out var specificationOpenApiInfo);
            if (isCached) return specificationOpenApiInfo;

            // 本地函数
            static SpecificationOpenApiInfo Function(string group)
            {
                return _specificationDocumentSettings.GroupOpenApiInfos.FirstOrDefault(u => u.Group == group) ?? new SpecificationOpenApiInfo { Group = group };
            }

            // 调用本地函数
            specificationOpenApiInfo = Function(group);
            GetGroupOpenApiInfoCached.TryAdd(group, specificationOpenApiInfo);
            return specificationOpenApiInfo;
        }

        /// <summary>
        /// 读取所有分组信息
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<string> ReadGroups()
        {
            // 获取所有的控制器和动作方法
            var controllers = App.CanBeScanTypes.Where(u => Penetrates.IsController(u));
            var actions = controllers.SelectMany(c => c.GetMethods().Where(u => IsAction(u, c)));

            // 合并所有分组
            var groupOrders = controllers.SelectMany(u => GetControllerGroups(u))
                .Union(
                    actions.SelectMany(u => GetActionGroups(u))
                )
                .Where(u => u != null)
                // 分组后取最大排序
                .GroupBy(u => u.Group)
                .Select(u => new GroupOrder
                {
                    Group = u.Key,
                    Order = u.Max(x => x.Order)
                });

            // 分组排序
            return groupOrders
                .OrderByDescending(u => u.Order)
                .ThenBy(u => u.Group)
                .Select(u => u.Group);
        }

        /// <summary>
        /// <see cref="GetControllerGroups(MethodInfo)"/> 缓存集合
        /// </summary>
        private static readonly ConcurrentDictionary<Type, IEnumerable<GroupOrder>> GetControllerGroupsCached;

        /// <summary>
        /// 获取控制器分组列表
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static IEnumerable<GroupOrder> GetControllerGroups(Type type)
        {
            var isCached = GetControllerGroupsCached.TryGetValue(type, out var groups);
            if (isCached) return groups;

            // 本地函数
            static IEnumerable<GroupOrder> Function(Type type)
            {
                // 如果控制器没有定义 [ApiDescriptionSettings] 特性，则返回默认分组
                if (!type.IsDefined(typeof(ApiDescriptionSettingsAttribute), true)) return _defaultGroups;

                // 读取分组
                var apiDescriptionSettings = type.GetCustomAttribute<ApiDescriptionSettingsAttribute>(true);
                if (apiDescriptionSettings.Groups == null || apiDescriptionSettings.Groups.Length == 0) return _defaultGroups;

                // 处理排序
                var groupOrders = new List<GroupOrder>();
                foreach (var group in apiDescriptionSettings.Groups)
                {
                    groupOrders.Add(ResolveGroupOrder(group));
                }

                return groupOrders;
            }

            // 调用本地函数
            groups = Function(type);
            GetControllerGroupsCached.TryAdd(type, groups);
            return groups;
        }

        /// <summary>
        /// <see cref="GetActionGroups(MethodInfo)"/> 缓存集合
        /// </summary>
        private static readonly ConcurrentDictionary<MethodInfo, IEnumerable<GroupOrder>> GetActionGroupsCached;

        /// <summary>
        /// 获取动作方法分组列表
        /// </summary>
        /// <param name="method">方法</param>
        /// <returns></returns>
        private static IEnumerable<GroupOrder> GetActionGroups(MethodInfo method)
        {
            var isCached = GetActionGroupsCached.TryGetValue(method, out var groups);
            if (isCached) return groups;

            // 本地函数
            static IEnumerable<GroupOrder> Function(MethodInfo method)
            {
                // 如果动作方法没有定义 [ApiDescriptionSettings] 特性，则返回所在控制器分组
                if (!method.IsDefined(typeof(ApiDescriptionSettingsAttribute), true)) return GetControllerGroups(method.DeclaringType);

                // 读取分组
                var apiDescriptionSettings = method.GetCustomAttribute<ApiDescriptionSettingsAttribute>(true);
                if (apiDescriptionSettings.Groups == null || apiDescriptionSettings.Groups.Length == 0) return GetControllerGroups(method.DeclaringType);

                // 处理排序
                var groupOrders = new List<GroupOrder>();
                foreach (var group in apiDescriptionSettings.Groups)
                {
                    groupOrders.Add(ResolveGroupOrder(group));
                }

                return groupOrders;
            }

            // 调用本地函数
            groups = Function(method);
            GetActionGroupsCached.TryAdd(method, groups);
            return groups;
        }

        /// <summary>
        /// <see cref="GetActionTag(ApiDescription)"/> 缓存集合
        /// </summary>
        private static readonly ConcurrentDictionary<ControllerActionDescriptor, string> GetControllerTagCached;

        /// <summary>
        /// 获取控制器标签
        /// </summary>
        /// <param name="apiDescription">控制器接口描述器</param>
        /// <returns></returns>
        private static string GetControllerTag(ControllerActionDescriptor controllerActionDescriptor)
        {
            var isCached = GetControllerTagCached.TryGetValue(controllerActionDescriptor, out var tag);
            if (isCached) return tag;

            // 本地函数
            static string Function(ControllerActionDescriptor controllerActionDescriptor)
            {
                var type = controllerActionDescriptor.ControllerTypeInfo;
                // 如果动作方法没有定义 [ApiDescriptionSettings] 特性，则返回所在控制器名
                if (!type.IsDefined(typeof(ApiDescriptionSettingsAttribute), true)) return controllerActionDescriptor.ControllerName;

                // 读取标签
                var apiDescriptionSettings = type.GetCustomAttribute<ApiDescriptionSettingsAttribute>(true);
                return string.IsNullOrEmpty(apiDescriptionSettings.Tag) ? controllerActionDescriptor.ControllerName : apiDescriptionSettings.Tag;
            }

            // 调用本地函数
            tag = Function(controllerActionDescriptor);
            GetControllerTagCached.TryAdd(controllerActionDescriptor, tag);
            return tag;
        }

        /// <summary>
        /// <see cref="GetActionTag(ApiDescription)"/> 缓存集合
        /// </summary>
        private static readonly ConcurrentDictionary<ApiDescription, string> GetActionTagCached;

        /// <summary>
        /// 获取动作方法标签
        /// </summary>
        /// <param name="apiDescription">接口描述器</param>
        /// <returns></returns>
        private static string GetActionTag(ApiDescription apiDescription)
        {
            var isCached = GetActionTagCached.TryGetValue(apiDescription, out var tag);
            if (isCached) return tag;

            // 本地函数
            static string Function(ApiDescription apiDescription)
            {
                if (!apiDescription.TryGetMethodInfo(out var method)) return "unknown";

                // 获取控制器描述器
                var controllerActionDescriptor = apiDescription.ActionDescriptor as ControllerActionDescriptor;

                // 如果动作方法没有定义 [ApiDescriptionSettings] 特性，则返回所在控制器名
                if (!method.IsDefined(typeof(ApiDescriptionSettingsAttribute), true)) return GetControllerTag(controllerActionDescriptor);

                // 读取标签
                var apiDescriptionSettings = method.GetCustomAttribute<ApiDescriptionSettingsAttribute>(true);
                return string.IsNullOrEmpty(apiDescriptionSettings.Tag) ? GetControllerTag(controllerActionDescriptor) : apiDescriptionSettings.Tag;
            }

            // 调用本地函数
            tag = Function(apiDescription);
            GetActionTagCached.TryAdd(apiDescription, tag);
            return tag;
        }

        /// <summary>
        /// 是否是动作方法
        /// </summary>
        /// <param name="method">方法</param>
        /// <param name="declaringType">声明类型</param>
        /// <returns></returns>
        private static bool IsAction(MethodInfo method, Type declaringType)
        {
            // 不是非公开、抽象、静态、泛型方法
            if (!method.IsPublic || method.IsAbstract || method.IsStatic || method.IsGenericMethod) return false;

            // 如果所在类型不是控制器，则该行为也被忽略
            if (method.DeclaringType != declaringType) return false;

            // 不是能被导出忽略的接方法
            if (method.IsDefined(typeof(ApiExplorerSettingsAttribute), true) && method.GetCustomAttribute<ApiExplorerSettingsAttribute>(true).IgnoreApi) return false;

            return true;
        }

        /// <summary>
        /// 解析分组名称中的排序
        /// </summary>
        /// <param name="name">分组名</param>
        /// <returns></returns>
        private static GroupOrder ResolveGroupOrder(string group)
        {
            string realGroup;
            var order = 0;

            if (!_groupOrderRegex.IsMatch(group)) realGroup = group;
            else
            {
                realGroup = _groupOrderRegex.Replace(group, "");
                order = int.Parse(_groupOrderRegex.Match(group).Groups["order"].Value);
            }

            var groupOpenApiInfo = GetGroupOpenApiInfo(realGroup);
            return new GroupOrder
            {
                Group = realGroup,
                Order = groupOpenApiInfo.Order ?? order
            };
        }
    }
}