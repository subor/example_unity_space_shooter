using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class BuildScript
{
    static void PerformBuild()
    {
        string[] scenes = { "Assets/_Complete-Game.unity", };
        BuildPipeline.BuildPlayer(scenes, "StandaloneWindows64", BuildTarget.StandaloneWindows64, BuildOptions.None);
    }
	private static string GetExtension()
	{
		string extension = "";

		switch (EditorUserBuildSettings.activeBuildTarget)
		{

		case BuildTarget.StandaloneWindows:
		case BuildTarget.StandaloneWindows64:
			extension = ".exe";
			break;         
		}

		return extension;
	}

}
