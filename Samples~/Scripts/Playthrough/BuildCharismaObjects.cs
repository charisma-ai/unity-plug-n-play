
#if UNITY_EDITOR

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace CharismaSDK.PlugNPlay
{
    public static class BuildCharismaObjects
    {
        [MenuItem("Charisma/Create Playthrough Metadata")]
        public static void CreateAllMetadataFunctions()
        {
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsSubclassOf(typeof(MetadataFunction)));

            // Display the results
            foreach (var type in types)
            {
                var asset = ScriptableObject.CreateInstance(type);

                if (asset == null)
                {
                    continue;
                }

                Debug.Log($"Creating meta function of type: {type.Name}");

                var metadataFunctionFolder = "Assets/Samples/Charisma.ai Plug-N-Play/0.2.2/Example/Data/Playthrough/Metadata/";
                if (!Directory.Exists(metadataFunctionFolder))
                {
                    Directory.CreateDirectory(metadataFunctionFolder);
                }

                var path = metadataFunctionFolder + type.Name + ".asset";
                AssetDatabase.CreateAsset(asset, path);
            }

            AssetDatabase.SaveAssets();
        }
    }
}

#endif
