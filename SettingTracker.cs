using SecretHistories.UI;
using SecretHistories.Entities;
using SecretHistories.Fucine;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public delegate void TrackerUpdate<T>(SettingTracker<T> tracker);

public class SettingTracker<T>: ISettingSubscriber
{
    public T current {get; protected set;}
    public string settingId {get; protected set;}
    public TrackerUpdate<T> whenUpdated {get; protected set;}
    public TrackerUpdate<T> beforeUpdated {get; protected set;}

    public SettingTracker(string settingId, TrackerUpdate<T> whenUpdated=null, TrackerUpdate<T> beforeUpdated=null, bool start=true)
    {
        this.settingId = settingId;
        this.whenUpdated = whenUpdated;
        this.beforeUpdated = beforeUpdated;
        if (start)
            this.Start();
    }

    public void Start()
    {
        Setting setting = Watchman.Get<Compendium>().GetEntityById<Setting>(this.settingId);
        if (setting == null)
        {
            NoonUtility.LogWarning(string.Format("Setting Missing: {0}", this.settingId));
        }
        setting.AddSubscriber((ISettingSubscriber) this);
        this.WhenSettingUpdated(setting.CurrentValue);
    }

    public virtual void SetCurrent(object newValue)
    {
        try {
            this.current = (T) newValue;
        } catch {
            NoonUtility.LogWarning(string.Format("SettingTracker {0}: Unable to set current to {1}", this.settingId, newValue));
        }
    }

    public virtual void CallWhen()
    {
        if (this.whenUpdated != null)
        {
            this.whenUpdated(this);
        }
    }

    public virtual void CallBefore()
    {
        if (this.beforeUpdated != null)
        {
            this.beforeUpdated(this);
        }
    }

    public virtual void WhenSettingUpdated(object newValue)
    {
        this.SetCurrent(newValue);
        this.CallWhen();
    }

    public virtual void BeforeSettingUpdated(object newValue)
    {
        this.CallBefore();
    }
}

public class ValueTracker<T> : SettingTracker<T>
{
    public T[] values {get; set;}

    public ValueTracker(string settingId, T[] values, TrackerUpdate<T> whenUpdated=null, TrackerUpdate<T> beforeUpdated=null, bool start=true)
        : base(settingId, whenUpdated, beforeUpdated, false)
    {
        this.values = values;
        if (start)
            this.Start();
    }

    public override void SetCurrent(object newValue)
    {
        if (!(newValue is int num))
            num = 1;
        int index = Mathf.Min(this.values.Length - 1, Mathf.Max(num, 0));
        this.current = this.values[index];
    }

}

public class KeybindTracker : SettingTracker<Key>
{
    public KeybindTracker(string settingId, TrackerUpdate<Key> whenUpdated=null, TrackerUpdate<Key> beforeUpdated=null, bool start=true)
        : base(settingId, whenUpdated, beforeUpdated, false)
    {
        this.current = Key.None;
        if (start)
            this.Start();
    }

	public static Key ToKey(string id)
	{
		int num = id.LastIndexOf('/');
		string s = num < 0 ? id : id.Substring(num + 1);
		s = int.TryParse(s, out _) ? "Digit" + s : s;
        s = s.Length > 1 ? char.ToUpper(s[0]) + s.Substring(1) : s.ToUpper();
        Key ret = Key.None;
        try {
            ret = (Key) Enum.Parse(typeof (Key), s);
        } catch {
            NoonUtility.LogWarning(string.Format("Unable to parse keybind: {}", id));
        }
		return ret;
	}

    public override void SetCurrent(object newValue)
    {
        this.current = KeybindTracker.ToKey((string) newValue);
    }

    public bool wasPressedThisFrame()
    {
        if (Keyboard.current == null || this.current == Key.None)
            return false;
        try {
            return Keyboard.current[this.current].wasPressedThisFrame;
        } catch {
            NoonUtility.LogWarning(string.Format("Unable to find key: {0}", this.current));
        }
        return false;
    }

}


