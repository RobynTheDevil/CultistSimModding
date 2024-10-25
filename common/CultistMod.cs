using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using SecretHistories;
using SecretHistories.UI;
using SecretHistories.Manifestations;
using SecretHistories.Entities;
using SecretHistories.Spheres;
using SecretHistories.Abstract;
using HarmonyLib;

public class CultistMod : MonoBehaviour
{
    public static bool started = false;
    public static PatchTracker showTimers {get; private set;}

    public void Start() => SceneManager.sceneLoaded += Load;

    public void OnDestroy() => SceneManager.sceneLoaded -= Load;

    public static void Load(Scene scene, LoadSceneMode mode) {
        try
        {
            if (!started) {
                showTimers = new PatchTracker("CopyableText", new MainPatch(), WhenSettingUpdated);
                started = true;
            } else {
                showTimers.Subscribe();
            }
        }
        catch (Exception ex)
        {
          NoonUtility.LogException(ex);
        }
        NoonUtility.Log("CopyableText: Trackers Started");
    }

    public static void Initialise() {
        //Harmony.DEBUG = true;
        Patch.harmony = new Harmony("robynthedevil.copyabletext");
		new GameObject().AddComponent<CopyableText>();
        NoonUtility.Log("CopyableText: Initialised");
	}

    public static IEnumerable<Token> GetTokens() {
        return Watchman.Get<HornedAxe>().GetExteriorSpheres()
            .Where<Sphere>((Func<Sphere, bool>) (x => (double) x.TokenHeartbeatIntervalMultiplier > 0.0))
            .SelectMany<Sphere, Token>((Func<Sphere, IEnumerable<Token>>) (x => x.GetTokens()))
            .Where<Token>((Func<Token, bool>) (x => x.Payload is ElementStack && ((ElementStack)x.Payload).Decays));
    }

    public static void WhenSettingUpdated(SettingTracker<bool> tracker) {
        NoonUtility.Log(string.Format("CopyableText: Setting Updated {0}", tracker.current));
        if (tracker.current) {
            Enable();
        } else {
            Disable();
        }
    }

    public static void Enable() {
        IEnumerable<Token> tokens = GetTokens();
        foreach (Token token in tokens) {
            Traverse.Create(token).Field("_manifestation").Field("_alwaysDisplayDecayView").SetValue(true);
            Traverse.Create(token).Field("_manifestation").Method("ShowDecayView").GetValue();
        }
    }

    public static void Disable() {
        IEnumerable<Token> tokens = GetTokens();
        foreach (Token token in tokens) {
            Traverse.Create(token).Field("_manifestation").Field("_alwaysDisplayDecayView").SetValue(false);
            Traverse.Create(token).Field("_manifestation").Method("HideDecayView").GetValue();
        }
    }
}

