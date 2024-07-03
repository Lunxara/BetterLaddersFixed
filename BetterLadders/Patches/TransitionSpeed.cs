using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace BetterLadders.Patches
{
    internal class TransitionSpeed
    {
        [HarmonyTranspiler, HarmonyPatch(typeof(InteractTrigger), nameof(InteractTrigger.ladderClimbAnimation), MethodType.Enumerator)]
        internal static IEnumerable<CodeInstruction> TransitionSpeedTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            Plugin.Logger.LogInfo("Starting TransitionSpeed transpiler");
            var code = new List<CodeInstruction>(instructions);
            if (Config.Instance.transitionSpeedMultiplier <= 0)
            {
                Plugin.Logger.LogError($"transitionSpeedMultiplier ({Config.Instance.transitionSpeedMultiplier}) is an invalid value");
                return code;
            }
            for (int i = 0; i < code.Count; i++)
            {
                if (code[i].opcode == OpCodes.Call && 
                    code[i].operand is MethodInfo method && method.Name == "get_deltaTime")
                {
                    code.Insert(i + 1, new CodeInstruction(OpCodes.Ldc_R4, Config.Instance.transitionSpeedMultiplier));
                    code.Insert(i + 2, new CodeInstruction(OpCodes.Mul));
                    Plugin.Instance.TranspilerLogger(code, i, -2, 4, "TransitionSpeed");
                }
            }
            return code;
        }
    }
}
