using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace BetterLadders.Patches
{
    internal class ExtLadderTime
    {

        [HarmonyTranspiler, HarmonyPatch(typeof(ExtensionLadderItem), nameof(ExtensionLadderItem.Update))]
        static IEnumerable<CodeInstruction> ExtLadderTimeTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            Plugin.Logger.LogWarning("Starting ExtLadderTime transpiler");
            var code = new List<CodeInstruction>(instructions);
            if (Config.Instance.timeMultiplier < 0)
            {
                Plugin.Logger.LogWarning("timeMultiplier is set to a negative value, not changing extension ladder time");
                return code;
            }
            bool patching = Plugin.Instance.transpilersArePatching;
            if (Config.Instance.timeMultiplier > 0)
            {
                for (int i = 0; i < code.Count - 1; i++)
                {
                    //if (!this.ladderBlinkWarning && this.ladderTimer > 15f)
                    //if (this.ladderTimer >= 20f)
                    if (code[i].opcode == OpCodes.Ldfld && code[i].operand is FieldInfo field && field.Name == "ladderTimer" &&
                        code[i + 1].opcode == OpCodes.Ldc_R4)
                    {
                        float initalTime = (float)code[i + 1].operand; // 15f or 20f
                        Plugin.Logger.LogInfo($"Patching: {patching}");
                        if (patching)
                        {
                            code.Insert(i + 2, new CodeInstruction(OpCodes.Pop)); // using pop allows finding the original value again in GetPatchedMethods()
                            code.Insert(i + 3, new CodeInstruction(OpCodes.Ldc_R4, initalTime * Config.Instance.timeMultiplier));
                        }
                        else
                        {
                            if (code[i + 2].opcode == OpCodes.Pop && code[i + 3].opcode == OpCodes.Ldc_R4 && (float)code[i + 3].operand == initalTime * Config.Instance.timeMultiplier)
                            {
                                code.RemoveRange(i + 2, i + 3);
                            }
                        }
                        Plugin.Instance.TranspilerLogger(code, i, -2, 5, "ExtLadderTime");
                        /*
                        List<bool> PatchedMethods = GetPatchedMethods(code); // test this method
                        bool customTimeAlreadyPatched = PatchedMethods[0];
                        bool permanentTimeAlreadyPatched = PatchedMethods[1];

                        if (customTimeAlreadyPatched)
                        {
                            code[i + 3] = new CodeInstruction(OpCodes.Ldc_R4, initalTime * Config.Instance.timeMultiplier);
                            Plugin.Logger.LogInfo($"Custom timeMultiplier was patched previously, setting new value to {(float)code[i + 3].operand}");
                        }
                        if (permanentTimeAlreadyPatched)
                        {
                            TryRemovePermanentTime(ref code); // fix this method ??? fixed ???
                            Plugin.Logger.LogInfo($"Permanent timeMultiplier was patched previously, replacing with custom timeMultiplier");
                        }
                        if (customTimeAlreadyPatched) continue;
                        */
                    }
                }
                return code;
            }
            else if (Config.Instance.timeMultiplier == 0) // if 0, permanent
            {
                for (int i = 0; i < code.Count - 5; i++)
                {
                    //14 | if (this.ladderAnimationBegun)
                    if (code[i].opcode == OpCodes.Ret &&
                        code[i + 2].opcode == OpCodes.Ldfld && code[i + 2].operand is FieldInfo field && field.Name == "ladderAnimationBegun" &&
                        code[i + 5].opcode == OpCodes.Ldarg_0) // this one is true regardless of if the original method is patched or not; there are two ldarg.0's in a row
                    {
                        if (patching)
                        {
                            code.Insert(i + 4, new CodeInstruction(OpCodes.Ret));
                        }
                        else
                        {
                            if (code[i + 4].opcode == OpCodes.Ret)
                            {
                                code.RemoveAt(i + 4);
                            }
                        }
                        Plugin.Instance.TranspilerLogger(code, i, -2, 6, "ExtLadderTime");
                        return code;
                        /*
                        List<bool> PatchedMethods = GetPatchedMethods(code); // test this method
                        bool customTimeAlreadyPatched = PatchedMethods[0];
                        bool permanentTimeAlreadyPatched = PatchedMethods[1];

                        if (permanentTimeAlreadyPatched)
                        {
                            return code;
                        }

                        return code;
                        */
                    }
                }
                return code;
            }
            return code;
        }
        static List<bool> GetPatchedMethods(List<CodeInstruction> code)
        {
            List<bool> PatchedMethods = [false, false];
            int numFound = 0;
            for (int i = 0; i < code.Count - 3; i++)
            {
                if (code[i].opcode == OpCodes.Ldfld && code[i].operand is FieldInfo field && field.Name == "ladderTimer" &&
                        code[i + 1].opcode == OpCodes.Ldc_R4 &&
                        code[i + 2].opcode == OpCodes.Pop &&
                        code[i + 3].opcode == OpCodes.Ldc_R4)
                {
                    Plugin.Logger.LogInfo("Custom time already patched - GetPatchedMethods()");
                    numFound++;
                    if (numFound == 2) PatchedMethods[0] = true;
                }
            }
            for (int i = 0; i < code.Count - 5; i++)
            {
                if (code[i].opcode == OpCodes.Ret &&
                    code[i + 2].opcode == OpCodes.Ldfld && code[i + 2].operand is FieldInfo field && field.Name == "ladderAnimationBegun" &&
                    code[i + 4].opcode == OpCodes.Ret &&
                    code[i + 5].opcode == OpCodes.Ldarg_0) // this one is true regardless of if the original method is patched or not; there are two ldarg.0's in a row
                {
                    Plugin.Logger.LogInfo("Permanent time already patched - GetPatchedMethods()");
                    PatchedMethods[1] = true;
                }
            }
            return PatchedMethods;
        }
        static bool TryRemovePermanentTime(ref List<CodeInstruction> code)
        {
            for (int i = 0; i < code.Count - 5; i++)
            {
                //14 | if (this.ladderAnimationBegun)
                if (code[i].opcode == OpCodes.Ret &&
                    code[i + 2].opcode == OpCodes.Ldfld && code[i + 2].operand is FieldInfo field && field.Name == "ladderAnimationBegun" &&
                    code[i + 4].opcode == OpCodes.Ret &&
                    code[i + 5].opcode == OpCodes.Ldarg_0) // this one is true regardless of if the original method is patched or not; there are two ldarg.0's in a row
                {
                    code.RemoveAt(i + 4);
                    Plugin.Logger.LogInfo("Removed previous (permanent) time");
                    return true;
                }
            }
            return false;
        }
    }
}
