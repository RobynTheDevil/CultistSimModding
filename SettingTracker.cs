using SecretHistories.UI;
using SecretHistories.Entities;
using SecretHistories.Fucine;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public delegate void SettingTrackerUpdate(object newValue);

public class SettingTracker: ISettingSubscriber
{
    public string settingId;
    public SettingTrackerUpdate whenUpdated;
    public SettingTrackerUpdate beforeUpdated;

    public SettingTracker(string settingId, SettingTrackerUpdate whenUpdated=null, SettingTrackerUpdate beforeUpdated=null, bool start=true)
    {
        this.settingId = settingId;
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

    public void WhenSettingUpdated(object newValue)
    {
        if (this.whenUpdated != null)
            this.whenUpdated(newValue);
    }

    public void BeforeSettingUpdated(object newValue)
    {
        if (this.beforeUpdated != null)
            this.beforeUpdated(newValue);
    }
}

public class ValueTracker<T> : SettingTracker
{
    public T[] values {get; set;}

    public T current {get; protected set;}

    public ValueTracker(string settingId, T[] values, SettingTrackerUpdate whenUpdated=null, SettingTrackerUpdate beforeUpdated=null, bool start=true)
        : base(settingId, whenUpdated, beforeUpdated, false)
    {
        this.values = values;
        if (start)
            this.Start();
    }

    public void SetCurrent(object newValue)
    {
        if (!(newValue is int num))
            num = 1;
        int index = Mathf.Min(this.values.Length - 1, Mathf.Max(num, 0));
        this.current = this.values[index];
    }

    public new void WhenSettingUpdated(object newValue)
    {
        this.SetCurrent(newValue);
        base.WhenSettingUpdated(newValue);
    }
}

public class KeybindTracker : SettingTracker
{
    public Key key {get; private set;}

    public KeybindTracker(string settingId, SettingTrackerUpdate whenUpdated=null, SettingTrackerUpdate beforeUpdated=null, bool start=true)
        : base(settingId, whenUpdated, beforeUpdated, false)
    {
        if (start)
            this.Start();
    }

	public static Key ToKey(string id)
	{
		int num = id.LastIndexOf('/');
		string s = num < 0 ? id : id.Substring(num + 1);
		s = int.TryParse(s, out _) ? "Digit" + s : s;
        s = s.Length > 1 ? char.ToUpper(s[0]) + s.Substring(1) : s.ToUpper();
		return (Key) Enum.Parse(typeof (Key), s);
	}

    public new void WhenSettingUpdated(object newValue)
    {
        this.key = KeybindTracker.ToKey((string) newValue);
        base.WhenSettingUpdated(newValue);
    }

    public bool wasPressedThisFrame => Keyboard.current[this.key].wasPressedThisFrame;

}


