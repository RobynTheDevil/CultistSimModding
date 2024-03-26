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

    public MethodInfo original;
    public MethodInfo patch;

    public void DoPatch()
    {
        Patch.harmony.Patch(this.original, transpiler: new HarmonyMethod(this.patch));
    }

    public void UnPatch()
    {
        Patch.harmony.Unpatch(this.original, this.patch);
    }
}

public class PatchHelper
{
    public static int FindLdstrOperand(List<CodeInstruction> codes, string operand, int skip=0)
    {
        for (int i = 0; i < codes.Count; i++)
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

    public static int FindOpcode(List<CodeInstruction> codes, OpCode opcode, int skip=0)
    {
        for (int i = 0; i < codes.Count; i++)
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
        //NoonUtility.Log(string.Format("{0}: Patch When {1}", Patch.harmony.Id, settingId));
        bool prev = this.current;
        this.SetCurrent(newValue);
        //NoonUtility.Log(string.Format("{0}: Values {1}, {2}", Patch.harmony.Id, prev, newValue));
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
