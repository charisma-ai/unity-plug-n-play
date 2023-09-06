
#if UNITY_EDITOR

using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CharismaSDK.PlugNPlay
{
    public static class BuildCharismaObjects
    {

        [MenuItem("Charisma/Create Playthrough Metadata")]
        public static void CreateAllMetadataFunctions()
        {
            DirectoryInfo dir = new DirectoryInfo("Assets/");
            FileInfo[] info = dir.GetFiles("*Function.cs", SearchOption.AllDirectories);
            var fullNames = info.Select(f => f.FullName).ToArray();

            foreach (string f in fullNames)
            {
                var index = f.LastIndexOf(@"\");

                var result = f;

                if (index >= 0)
                {
                    result = result.Substring(index + 1);
                    index = result.IndexOf(@".");
                    if (index >= 0)
                    {
                        result = result.Substring(0, index);
                    }
                }

                ScriptableObject asset = ScriptableObject.CreateInstance(result);

                if (asset == null)
                {
                    continue;
                }

                string metadataFunctionFolder = "Assets/Charisma/Data/Playthrough/Metadata/";
                if (!Directory.Exists(metadataFunctionFolder))
                {
                    Directory.CreateDirectory(metadataFunctionFolder);
                }

                var path = metadataFunctionFolder + result + ".asset";
                AssetDatabase.CreateAsset(asset, path);

                Debug.Log("Creating MetadataFunction object at : " + path);
            }


            AssetDatabase.SaveAssets();
        }
    }
}

#endif
