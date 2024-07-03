using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace BetterLadders.Patches
{
    internal class KillTrigger
    {
        [HarmonyTranspiler, HarmonyPatch(typeof(ExtensionLadderItem), nameof(ExtensionLadderItem.LadderAnimation), MethodType.Enumerator)] // check method
        internal static IEnumerable<CodeInstruction> KillTriggerTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            Plugin.Logger.LogInfo("Starting KillTrigger transpiler");
            var code = new List<CodeInstruction>(instructions);
            for (int i = 0; i < code.Count - 1; i++)
            {
                if (code[i].opcode == OpCodes.Ldfld && code[i].operand is FieldInfo field && field.Name == "killTrigger" &&
                    code[i + 1].opcode == OpCodes.Ldc_I4_1)
                {
                    code.Insert(i + 2, new CodeInstruction(OpCodes.Pop)); // Rather than replacing the value, pop the Ldc_I4_1 so that it can still be found later when joining a new host
                    code.Insert(i + 3, new CodeInstruction(Config.Instance.enableKillTrigger ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0));
                    Plugin.Instance.TranspilerLogger(code, i, -2, 6, "KillTrigger");
                }
            }
            return code;
        }
    }
}
