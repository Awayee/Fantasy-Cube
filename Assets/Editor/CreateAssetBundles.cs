using UnityEditor;
using System.IO;

public class CreateAssetBundles{
	[MenuItem("Assets/Create AssetBundles")]
	static void BuildAllAssetBundles(){
        string path = "Assets/AssetBundles";
		if(!Directory.Exists(path))
            Directory.CreateDirectory(path);
        BuildPipeline.BuildAssetBundles(path, BuildAssetBundleOptions.None, BuildTarget.Android);
    }
}
