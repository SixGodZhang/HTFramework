﻿using System;
using System.Collections.Generic;
using System.Reflection;

namespace HT.Framework
{
    /// <summary>
    /// 默认的自定义模块管理器助手
    /// </summary>
    public sealed class DefaultCustomModuleHelper : ICustomModuleHelper
    {
        /// <summary>
        /// 自定义模块管理器
        /// </summary>
        public IModuleManager Module { get; set; }
        /// <summary>
        /// 所有自定义模块
        /// </summary>
        public Dictionary<string, CustomModuleBase> CustomModules { get; private set; } = new Dictionary<string, CustomModuleBase>();

        /// <summary>
        /// 初始化助手
        /// </summary>
        public void OnInit()
        {
            List<Type> types = ReflectionToolkit.GetTypesInRunTimeAssemblies(type =>
            {
                return type.IsSubclassOf(typeof(CustomModuleBase)) && !type.IsAbstract;
            });
            for (int i = 0; i < types.Count; i++)
            {
                CustomModuleAttribute att = types[i].GetCustomAttribute<CustomModuleAttribute>();
                if (att != null && att.IsEnable && !CustomModules.ContainsKey(att.ModuleName))
                {
                    CustomModules.Add(att.ModuleName, Activator.CreateInstance(types[i]) as CustomModuleBase);
                }
            }

            foreach (var module in CustomModules)
            {
                module.Value.OnInit();
            }
        }
        /// <summary>
        /// 助手准备工作
        /// </summary>
        public void OnReady()
        {
            foreach (var module in CustomModules)
            {
                module.Value.OnReady();
            }
        }
        /// <summary>
        /// 刷新助手
        /// </summary>
        public void OnUpdate()
        {
            foreach (var module in CustomModules)
            {
                if (module.Value.IsRunning)
                {
                    module.Value.OnUpdate();
                }
            }
        }
        /// <summary>
        /// 终结助手
        /// </summary>
        public void OnTerminate()
        {
            foreach (var module in CustomModules)
            {
                module.Value.OnTerminate();
            }

            CustomModules.Clear();
        }
        /// <summary>
        /// 暂停助手
        /// </summary>
        public void OnPause()
        {
            foreach (var module in CustomModules)
            {
                module.Value.OnPause();
            }
        }
        /// <summary>
        /// 恢复助手
        /// </summary>
        public void OnResume()
        {
            foreach (var module in CustomModules)
            {
                module.Value.OnResume();
            }
        }
    }
}