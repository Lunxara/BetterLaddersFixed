using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace BetterLadders.Patches
{
    internal class TransitionSpeed
    {
        [HarmonyTranspiler, HarmonyPatch(typeof(InteractTrigger), nameof(InteractTrigger.ladderClimbAnimation), MethodType.Enumerator)]
        static IEnumerable<CodeInstruction> TransitionSpeedTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            Plugin.Logger.LogWarning("Starting TransitionSpeed transpiler - this might show up multiple times");
            var code = new List<CodeInstruction>(instructions);
            if (Config.Instance.transitionSpeedMultiplier <= 0)
            {
                Plugin.Logger.LogError($"transitionSpeedMultiplier ({Config.Instance.transitionSpeedMultiplier}) is an invalid value");
                return code;
            }
            bool patching = Plugin.Instance.transpilersArePatching;
            for (int i = 0; i < code.Count; i++)
            {
                if (code[i].opcode == OpCodes.Call && 
                    code[i].operand is MethodInfo method &&
                    method.Name.Contains("get_deltaTime"))
                {
                    if (patching)
                    {
                        code.Insert(i + 1, new CodeInstruction(OpCodes.Ldc_R4, Config.Instance.transitionSpeedMultiplier));
                        code.Insert(i + 2, new CodeInstruction(OpCodes.Mul));
                    }
                    else
                    {
                        if (code[i + 1].opcode == OpCodes.Ldc_R4 && (float)code[i + 1].operand == Config.Instance.transitionSpeedMultiplier && code[i + 2].opcode == OpCodes.Mul)
                        {
                            code.RemoveRange(i + 1, i + 2);
                        }
                    }
                    Plugin.Instance.TranspilerLogger(code, i, -2, 4, "TransitionSpeed");
                    /*
                    bool alreadyPatched = code[i+1].opcode == OpCodes.Ldc_R4 && code[i+2].opcode == OpCodes.Mul;
                    if (alreadyPatched)
                    {
                        code[i + 1] = new CodeInstruction(OpCodes.Ldc_R4, Config.Instance.transitionSpeedMultiplier);
                        Plugin.Logger.LogInfo("Updated TransitionSpeed with host's new value");
                        Plugin.Logger.LogInfo("==========================================================");
                        //Updates the patched value with the host's new value
                        continue; // maybe keep return code;
                    }
                    code.Insert(i + 1, new CodeInstruction(OpCodes.Ldc_R4, Config.Instance.transitionSpeedMultiplier));
                    code.Insert(i + 2, new CodeInstruction(OpCodes.Mul));
                    Plugin.Instance.TranspilerLogger(code, i, -2, 4, "TransitionSpeed");
                    // maybe keep return code;
                    */
                }
            }
            return code;
        }
    }
}
