
#if UNITY_EDITOR

using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Compilation;

namespace CharismaSDK.PlugNPlay
{
    /// <summary>
    /// Automatically adds and (sometimes!) removes definition symbols based on assembly presence
    /// 
    /// NOTE: 
    /// If you have manually removed any of the Defined Assembly packages expected here
    /// outside of Unity, this may cause scripting errors.
    /// You will need to go into
    /// ==============================
    /// Project Settings > Player > Other Settings > Scripting Define Symbols 
    /// ==============================
    /// and remove the added Defines manually too.
    /// 
    /// Defines the OVR_LIPSYNC symbol for now.
    /// </summary>
    internal static class DefineHelper
    {
        private static readonly string[] DEFINES = new string[] { "OVR_LIPSYNC" };
        private static readonly string[] EXPECTED_ASSEMBLY = new string[] { "Oculus.LipSync" };


        [InitializeOnLoadMethod]
        private static void EnsureScriptingDefineSymbol()
        {
            var currentTarget = EditorUserBuildSettings.selectedBuildTargetGroup;

            if (currentTarget == BuildTargetGroup.Unknown)
            {
                return;
            }

            var definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(currentTarget).Trim();
            var defines = definesString.Split(';');

            bool changed = false;
            var allAssemblies = CompilationPipeline.GetAssemblies();

            for (int i = 0; i < DEFINES.Length; i++)
            {
                var define = DEFINES[i];
                var assemblyName = EXPECTED_ASSEMBLY[i];

                var containsDefine = defines.Contains(define);
                var containsAssembly = allAssemblies.Any(x => x.name == assemblyName);

                if (!containsDefine && containsAssembly)
                {
                    if (definesString.EndsWith(";", StringComparison.InvariantCulture) == false)
                    {
                        definesString += ";";
                    }

                    definesString += define;
                    changed = true;
                }
                else if (containsDefine && !containsAssembly)
                {
                    int index = definesString.IndexOf(define);
                    string cleanPath = (index < 0)
                        ? definesString
                        : definesString.Remove(index - 1, define.Length);
                    definesString = cleanPath;
                    changed = true;
                }
            }

            if (changed)
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(currentTarget, definesString);
            }
        }
    }
}

#endif
