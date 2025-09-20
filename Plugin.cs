using System;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;

[BepInPlugin(GUID,NAME,VER)]
public class Plugin : BaseUnityPlugin
{
    const string GUID = "WaterGun.WaterGunLib";
    const string VER = "1.0.0";
    const string NAME = "WaterGunLib";

    void Awake()
    {
        var types = Assembly.GetExecutingAssembly().GetTypes();
        foreach (var type in types)
        {
            var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            foreach (var method in methods)
            {
                var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                if (attributes.Length > 0)
                {
                    method.Invoke(null, null);
                }
            }
        }
        Logger.LogInfo("WaterGunLib: Loaded");
    }
}