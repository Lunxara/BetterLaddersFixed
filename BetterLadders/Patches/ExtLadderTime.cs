using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace BetterLadders.Patches
{
    internal class ExtLadderTime
    {

        [HarmonyTranspiler, HarmonyPatch(typeof(ExtensionLadderItem), nameof(ExtensionLadderItem.Update))]
        private static IEnumerable<CodeInstruction> ExtLadderTimeTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            Plugin.Logger.LogInfo("Starting ExtLadderTimeTranspiler");
            var code = new List<CodeInstruction>(instructions);
            if (Config.Instance.timeMultiplier < 0)
            {
                Plugin.Logger.LogWarning("timeMultiplier is set to a negative value, not changing extension ladder time");
                return code;
            }
            if (Config.Instance.timeMultiplier > 0)
            {
                for (int i = 0; i < code.Count - 1; i++)
                {
                    //if (!this.ladderBlinkWarning && this.ladderTimer > 15f)
                    //if (this.ladderTimer >= 20f)
                    if (code[i].opcode == OpCodes.Ldfld && code[i].operand is FieldInfo field && field.Name == "ladderTimer" &&
                        code[i + 1].opcode == OpCodes.Ldc_R4)
                    {
                        //float initalTime = (float)code[i + 1].operand; // 15f or 20f
                        //code.Insert(i + 2, new CodeInstruction(OpCodes.Pop)); // using pop allows finding the original value again in GetPatchedMethods()
                        //code.Insert(i + 3, new CodeInstruction(OpCodes.Ldc_R4, initalTime * Config.Instance.timeMultiplier));
                        code.Insert(i + 2, new CodeInstruction(OpCodes.Ldc_R4, Config.Instance.timeMultiplier));
                        code.Insert(i + 3, new CodeInstruction(OpCodes.Mul));
                        Plugin.Instance.TranspilerLogger(code, i, -2, 5, "ExtLadderTime");
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
                        code.Insert(i + 4, new CodeInstruction(OpCodes.Ret));
                        Plugin.Instance.TranspilerLogger(code, i, -2, 6, "ExtLadderTime");
                        return code;
                    }
                }
                return code;
            }
            return code;
        }
    }
}
