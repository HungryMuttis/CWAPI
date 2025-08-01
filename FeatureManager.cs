using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Configuration;

namespace CWAPI
{
    public class FeatureManager
    {
        private readonly List<IFeature> Features = [];
        private readonly ManualLogSource Logger;
        private readonly ConfigFile Config;

        public FeatureManager(BepInEx.Logging.ManualLogSource logger, ConfigFile configs)
        {
            Logger = new(logger, nameof(FeatureManager));
            Config = configs;
            RegisterFeaturesFromAssembly(Assembly.GetCallingAssembly());
        }

        public void RegisterFeaturesFromAssembly(Assembly assembly)
        {
            Logger.LogDebug("Scanning for features...");
            Type baseType = typeof(Feature<>);
            assembly.GetTypes()
                .Where(t =>
                {
                    if (!t.IsClass || t.IsAbstract || t.GetCustomAttribute<FeatureAttribute>() == null) return false;
                    Type? current = t.BaseType;
                    while (current != null)
                    {
                        if (current.IsGenericType && current.GetGenericTypeDefinition() == baseType)
                            return true;
                        current = current.BaseType;
                    }
                    return false;
                }).ToList()
                .ForEach(t =>
                {
                    if (Activator.CreateInstance(t) is IFeature feature)
                    {
                        Features.Add(feature);
                        Logger.LogDebug($"Discovered and registered feature: {feature.FeatureName}");
                    }    
                });
        }

        public bool InitializeFeatures(bool handleExceptions = false) => Features.All(f =>
        {
            try
            {
                ConfigSection Section = new(Config, f.FeatureName);
                f.CreateRequiredConfig(Section);
                f.CreateConfig(Section);
                if (f.Enabled)
                {
                    if (f.Required)
                        Logger.LogInfo($"Feature '{f.FeatureName}' is required. Initializing...");
                    else
                        Logger.LogInfo($"Feature '{f.FeatureName}' is enabled. Initializing...");
                    f.Initialize();
                }
                else
                    Logger.LogInfo($"Feature '{f.FeatureName}' is disabled.");
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError($"There was an error loading feature '{f.FeatureName}'. Exception: {ex}");
                if (!handleExceptions) throw;
                return false;
            }
        });
    }
}