﻿using BepInEx.Configuration;

namespace CWAPI
{
    public interface IFeature
    {
        string FeatureName { get; }
        bool Enabled { get; }
        bool Required { get; }

        void CreateRequiredConfig(ConfigSection section);
        void CreateConfig(ConfigSection section);
        void Initialize();
    }
    public abstract class Feature<T> : IFeature where T : Feature<T>
    {
        public static T Instance { get; private set; } = default!;

        private ManualLogSource? _logger;
        private ConfigEntry<bool>? _enabled;

        public bool Enabled => _enabled?.Value ?? Required;
        internal ManualLogSource Logger => _logger ??= new ManualLogSource(LogSource, FeatureName);

        public abstract BepInEx.Logging.ManualLogSource LogSource { get; }
        public virtual bool Required { get; }
        public abstract string FeatureName { get; }
        public abstract string FeatureDescription { get; }

        public Feature()
        {
            Instance = (T)this;
        }

        public void CreateRequiredConfig(ConfigSection section)
        {
            if (!Required)
                _enabled = section.Bind(
                    nameof(Enabled),
                    true,
                    $"Enables feature: {FeatureDescription}"
                );
        }
        public virtual void CreateConfig(ConfigSection section) { }
        public abstract void Initialize();

        public static void Debug(object data) => Instance.Logger.LogDebug(data);
        public static void Message(object data) => Instance.Logger.LogMessage(data);
        public static void Info(object data) => Instance.Logger.LogInfo(data);
        public static void Warning(object data) => Instance.Logger.LogWarning(data);
        public static void Error(object data) => Instance.Logger.LogError(data);
        public static void Fatal(object data) => Instance.Logger.LogFatal(data);
    }
}