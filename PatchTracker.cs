using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using System.Linq;
using SecretHistories;
using HarmonyLib;

public class Patch
{
    public static Harmony harmony {get; set;}

    public Type constructor;
    public MethodBase original;
    public MethodInfo patch;
    public bool isPatched = false;

    public void DoPatch()
    {
        if (this.isPatched) {
            NoonUtility.LogWarning(string.Format("{0}: Already Patched! {1}", harmony.Id, this.patch));
            return;
        }
        switch (this.patch.Name) {
            case "Prefix":
                harmony.Patch(this.original, prefix: new HarmonyMethod(this.patch));
                break;
            case "Postfix":
                harmony.Patch(this.original, postfix: new HarmonyMethod(this.patch));
                break;
            case "Transpiler":
                harmony.Patch(this.original, transpiler: new HarmonyMethod(this.patch));
                break;
            default:
                NoonUtility.LogWarning(string.Format("{0}: Unknown patch {1}", harmony.Id, this.patch));
                break;
        }
        this.isPatched = true;
    }

    public void UnPatch()
    {
        if (!this.isPatched) {
            NoonUtility.Log(string.Format("{0}: Unpatching Nothing {1}", harmony.Id, this.patch));
            return;
        }
        harmony.Unpatch(this.original, this.patch);
        this.isPatched = false;
    }

    public virtual void WhenSettingUpdated(object newValue) {}

}

public class PatchHelper
{
    public static int FindLdstrOperand(List<CodeInstruction> codes, string operand, int skip=0, int start=0)
    {
        for (int i = start; i < codes.Count; i++)
        {
            if (codes[i].opcode == OpCodes.Ldstr)
            {
                string op = codes[i].operand as string;
                if (op == operand)
                {
                    if (skip > 0)
                    {
                        skip--;
                    } else {
                        return i;
                    }
                }
            }
        }
        return -1;
    }

    public static int FindOpcode(List<CodeInstruction> codes, OpCode opcode, int skip=0, int start=0)
    {
        for (int i = start; i < codes.Count; i++)
        {
            if (codes[i].opcode == opcode)
            {
                string op = codes[i].operand as string;
                if (skip > 0)
                {
                    skip--;
                } else {
                    return i;
                }
            }
        }
        NoonUtility.LogWarning(string.Format("{0}: Unfound opcode {1}", Patch.harmony.Id, opcode));
        return -1;
    }
}

public class PatchTracker : ValueTracker<bool>
{

    public Patch patch {get; set;}

    public PatchTracker(string settingId, Patch patch, TrackerUpdate<bool> whenUpdated=null, TrackerUpdate<bool> beforeUpdated=null, bool start=true)
        : base(settingId, new bool[2] {false, true}, whenUpdated, beforeUpdated, false)
    {
        this.patch = patch;
        if (start)
            this.Start();
    }

    public override void WhenSettingUpdated(object newValue)
    {
        this.patch.WhenSettingUpdated(newValue);
        bool prev = this.current;
        this.SetCurrent(newValue);
        if (prev != this.current) {
            if (this.current) {
                NoonUtility.Log(string.Format("{0}: Patching {1}, {2}", Patch.harmony.Id, patch.original, patch.patch));
                try {
                    this.patch.DoPatch();
                } catch (Exception ex) {
                    NoonUtility.LogWarning(ex.ToString());
                    NoonUtility.LogException(ex);
                }
            } else {
                NoonUtility.Log(string.Format("{0}: Unpatching {1}, {2}", Patch.harmony.Id, patch.original, patch.patch));
                try {
                    this.patch.UnPatch();
                } catch (Exception ex) {
                    NoonUtility.LogWarning(ex.ToString());
                    NoonUtility.LogException(ex);
                }
            }
        }
        this.CallWhen();
    }
}

