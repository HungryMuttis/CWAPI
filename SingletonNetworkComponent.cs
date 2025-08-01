using MyceliumNetworking;
using System;
using UnityEngine;
using Steamworks;

namespace CWAPI
{
    public abstract class SingletonNetworkComponent<THandler> : MonoBehaviour where THandler : SingletonNetworkComponent<THandler>
    {
        private ManualLogSource? _logger;
        protected abstract BepInEx.Logging.ManualLogSource LogSource { get; }
        protected ManualLogSource Logger => _logger ??= new(LogSource, GetType().Name);
        protected abstract uint MOD_ID { get; }
        public static SingletonNetworkComponent<THandler>? Instance { get; private set; }
        public static THandler? TypedInstance => Instance as THandler;

        protected virtual void Awake()
        {
            if (Instance != null)
            {
                Logger.LogWarning($"This singleton already exists. Destroying self");
                Destroy(gameObject);
                return;
            }

            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
            gameObject.hideFlags |= HideFlags.DontSave;
            Instance = this;
            MyceliumNetwork.RegisterNetworkObject(this, MOD_ID);
            SuccessfulAwake();
        }
        protected virtual void SuccessfulAwake() { }
        protected virtual void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
                MyceliumNetwork.DeregisterNetworkObject(this, MOD_ID);
            }
        }


        protected static bool Send(string methodName, ReliableType reliable, params object[] parameters)
        {
            MyceliumNetwork.RPC(Instance?.MOD_ID ?? throw new InvalidOperationException($"[{typeof(THandler).Name}] Not initialized. Cannot send"), methodName, reliable, parameters);
            return true;
        }
        protected static bool SendTarget(string methodName, CSteamID target, ReliableType reliable, params object[] parameters)
        {
            MyceliumNetwork.RPCTarget(Instance?.MOD_ID ?? throw new InvalidOperationException($"[{typeof(THandler).Name}] Not initialized. Cannot send"), methodName, target, reliable, parameters);
            return true;
        }

        protected static bool Send(string methodName, CSteamID target, ReliableType reliable, params object[] parameters) => SendTarget(methodName, target, reliable, parameters);
    }
}
