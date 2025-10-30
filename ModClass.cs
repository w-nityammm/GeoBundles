using GeoBundles;
using Modding;
using SFCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace GeoBundles
{
    public class GeoBundles : Mod, ILocalSettings<MySaveSettings>
    {
        private bool wasAtBench = false;
        private Sprite _myGeoBundleSprite;

        public static GeoBundles Instance { get; private set; }
        public MySaveSettings SaveSettings { get; set; } = new MySaveSettings();
        public void OnLoadLocal(MySaveSettings s) => SaveSettings = s;
        public MySaveSettings OnSaveLocal() => SaveSettings;

        public GeoBundles() : base("GeoBundles")
        {
            Instance = this;
            ModHooks.LanguageGetHook += OnLanguageGet;
            ModHooks.GetPlayerIntHook += OnGetPlayerInt;
            ModHooks.SetPlayerIntHook += OnSetPlayerInt;

            _myGeoBundleSprite = LoadSprite("GeoBundles.Sprites.bundle.png");

            Log($"Constructor: Is sprite loaded? {_myGeoBundleSprite != null}");
            ItemHelper.AddCountedItem(
                sprite: _myGeoBundleSprite,
                playerdataInt: nameof(MySaveSettings.geoBundles),
                nameConvo: "GEO_BUNDLE_NAME",
                descConvo: "GEO_BUNDLE_DESC"
            );
        }

        public override string GetVersion() => "v1.0";

        public override List<(string, string)> GetPreloadNames()
        {
            return new List<(string, string)>
            {
                ("SFCore", "1.0.0.0")
            };
        }

        public override void Initialize()
        {
            Log("Initialize: GeoBundles Initialized");
            ModHooks.HeroUpdateHook += OnHeroUpdate;
        }

        private int OnGetPlayerInt(string name, int orig)
        {
            if (name == nameof(MySaveSettings.geoBundles))
            {
                return Instance.SaveSettings.geoBundles;
            }
            return orig;
        }

        private int OnSetPlayerInt(string name, int orig)
        {
            if (name == nameof(MySaveSettings.geoBundles))
            {
                Instance.SaveSettings.geoBundles = orig;
                return orig;
            }
            return orig;
        }

        private string OnLanguageGet(string key, string sheetTitle, string orig)
        {
            if (sheetTitle == "UI")
            {
                switch (key)
                {
                    case "GEO_BUNDLE_NAME":
                        return "Geo Bundle";
                    case "GEO_BUNDLE_DESC":
                        return "A heavy, cloth-wrapped bundle of Geo, packed for easy transport, doesn't get lost after death :) . Press K to craft 1 bundle for 300 geo. Press J to break 1 bundle into 260 geo. Must be at a bench to craft.";
                }
            }
            return orig;
        }

        public void OnHeroUpdate()
        {
            if (PlayerData.instance == null || HeroController.instance == null)
            {
                wasAtBench = false;
                return;
            }
            bool atBench = PlayerData.instance.atBench;
            if (atBench && !wasAtBench)
            {
                Log("at bench");
            }
            wasAtBench = atBench;

            if (Input.GetKeyDown(KeyCode.K))
            {
                if (atBench)
                {
                    if (PlayerData.instance.geo >= 300)
                    {
                        HeroController.instance.TakeGeo(300);
                        Instance.SaveSettings.geoBundles++;
                        Log($"Purchased 1 Geo Bundle. Total: {Instance.SaveSettings.geoBundles}");
                    }
                    else
                    {
                        Log("Not enough Geo to purchase a bundle.");
                    }
                }
                else
                {
                    Log("Must be at a bench to craft a Geo Bundle.");
                }
            }
            if (Input.GetKeyDown(KeyCode.J))
            {
                if (Instance.SaveSettings.geoBundles >= 1)
                {
                    Instance.SaveSettings.geoBundles--;
                    HeroController.instance.AddGeo(260);
                    Log($"Sold 1 Geo Bundle. Total: {Instance.SaveSettings.geoBundles}");
                }
                else
                {
                    Log("No Geo Bundles to sell.");
                }
            }
        }

        public Sprite LoadSprite(string resourcePath)
        {
            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                Stream stream = assembly.GetManifestResourceStream(resourcePath);

                if (stream == null)
                {
                    Log($"Error: Embedded resource not found at path: {resourcePath}");
                    return null;
                }

                byte[] buffer = new byte[stream.Length];
                stream.Read(buffer, 0, buffer.Length);
                stream.Dispose();

                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(buffer);
                texture.Apply();

                Rect rect = new Rect(0, 0, texture.width, texture.height);
                Vector2 pivot = new Vector2(0.5f, 0.5f);
                return Sprite.Create(texture, rect, pivot);
            }
            catch (Exception e)
            {
                Log($"Error loading sprite: {e}");
                return null;
            }
        }
    }
}
