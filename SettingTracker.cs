using SecretHistories.UI;
using SecretHistories.Entities;
using SecretHistories.Fucine;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class SettingTracker: ISettingSubscriber
{
    public string settingId;

    public SettingTracker(string settingId, bool start=true)
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

    public abstract void WhenSettingUpdated(object newValue);
    public abstract void BeforeSettingUpdated(object newValue);
}

public class ValueTracker<T> : SettingTracker
{
    public T[] values {get; set;}

    public T current {get; protected set;}

    public ValueTracker(string settingId, T[] values, bool start=true)
        : base(settingId, false)
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

    public override void BeforeSettingUpdated(object newValue) {}

    public override void WhenSettingUpdated(object newValue)
    {
        this.SetCurrent(newValue);
    }
}

public class KeybindTracker : SettingTracker
{
    public Key key {get; private set;}

    public KeybindTracker(string settingId, bool start=true)
        : base(settingId, false)
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

    public override void BeforeSettingUpdated(object newValue) {}

    public override void WhenSettingUpdated(object newValue)
    {
        this.key = KeybindTracker.ToKey((string) newValue);
    }

    public bool wasPressedThisFrame => Keyboard.current[this.key].wasPressedThisFrame;

}


