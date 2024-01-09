using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace BetterLadders.Patches
{
    internal class KillTrigger
    {
        [HarmonyTranspiler, HarmonyPatch(typeof(ExtensionLadderItem), nameof(ExtensionLadderItem.LadderAnimation), MethodType.Enumerator)] // check method
        static IEnumerable<CodeInstruction> KillTriggerTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            Plugin.Logger.LogWarning("Starting KillTrigger transpiler");
            var code = new List<CodeInstruction>(instructions);
            bool patching = Plugin.Instance.transpilersArePatching;
            for (int i = 0; i < code.Count - 1; i++)
            {
                if (code[i].opcode == OpCodes.Ldfld &&
                    code[i].operand is FieldInfo field && field.Name == "killTrigger" &&
                    code[i + 1].opcode == OpCodes.Ldc_I4_1)
                {
                    if (patching)
                    {
                        code.Insert(i + 2, new CodeInstruction(OpCodes.Pop)); // Rather than replacing the value, pop the Ldc_I4_1 so that it can still be found later when joining a new host
                        code.Insert(i + 3, new CodeInstruction(Config.Instance.enableKillTrigger ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0));
                    }
                    else
                    {
                        if (code[i + 2].opcode == OpCodes.Pop && code[i + 2].opcode == OpCodes.Ldc_I4_1 && code[i + 2].opcode == OpCodes.Ldc_I4_0)
                        {
                            code.RemoveRange(i + 2, i + 3);
                        }
                    }
                    Plugin.Instance.TranspilerLogger(code, i, -2, 6, "KillTrigger");
                }
            }
            return code;
        }

/*        static List<CodeInstruction> FindAndReplace(ref List<CodeInstruction> code)
        {
            Plugin.Logger.LogWarning("Starting KillTrigger transpiler");
            for (int i = 0; i < code.Count; i++)
            {
                //this.killTrigger.enabled = true;
                if (code[i].opcode == OpCodes.Ldfld &&
                    code[i].operand is FieldInfo field && field.Name == "killTrigger" &&
                    code[i + 1].opcode == OpCodes.Ldc_I4_1)
                {
                    bool alreadyPatched = code[i + 2].opcode == OpCodes.Pop;
                    if (alreadyPatched)
                    {
                        code[i + 3] = new CodeInstruction(Config.Instance.enableKillTrigger ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
                        Plugin.Logger.LogInfo("Updated KillTrigger with host's new value");
                        Plugin.Logger.LogInfo("==========================================================");
                        //Updates the patched value with the host's new value
                        continue;
                    }
                    code.Insert(i + 2, new CodeInstruction(OpCodes.Pop)); // Rather than replacing the value, pop the Ldc_I4_1 so that it can still be found later when joining a new host
                    code.Insert(i + 3, new CodeInstruction(OpCodes.Ldc_I4_0));
                    Plugin.Instance.TranspilerLogger(code, i, -2, 6, "KillTrigger");
                }
            }
            return code;
        }*/
    }
}
