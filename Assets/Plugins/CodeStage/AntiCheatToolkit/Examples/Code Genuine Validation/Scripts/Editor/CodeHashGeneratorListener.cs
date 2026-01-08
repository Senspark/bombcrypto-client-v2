#region copyright
// --------------------------------------------------------------
//  Copyright (C) Dmitriy Yukhanov - focus [http://codestage.net]
// --------------------------------------------------------------
#endregion

// please comment next line and build GenuineValidator scene for Windows PC platform to see CodeHashGeneratorListener example in action
#define ACTK_MUTE_EXAMPLES

#if UNITY_2018_1_OR_NEWER && !ACTK_MUTE_EXAMPLES
namespace CodeStage.AntiCheat.Examples.Genuine
{
	using AntiCheat.Genuine.CodeHash;
	using Common;
	using EditorCode.PostProcessors;
	using ObscuredTypes;
#if UNITY_STANDALONE_WIN
	using EditorCode;
#endif

	using System.IO;
	using UnityEditor;
	using UnityEditor.Build;
	using UnityEditor.Build.Reporting;
	using UnityEngine;

	// please check GenuineValidatorExample.cs to see runtime hash validation example

	public class CodeHashGeneratorListener : IPostprocessBuildWithReport
	{
		// using CodeHashGeneratorPostprocessor's 'order - 1' to subscribe before it finishes its job
		public int callbackOrder
		{
			get { return CodeHashGeneratorPostprocessor.Instance.callbackOrder - 1; }
		}

		public void OnPostprocessBuild(BuildReport report)
		{
			// make sure example scene is built as a first scene, feel free to remove this in your real production code

			var exampleBuilt = true;
			foreach (var editorBuildSettingsScene in EditorBuildSettings.scenes)
			{
				if (!editorBuildSettingsScene.enabled)
				{
					continue;
				}

				if (!editorBuildSettingsScene.path.EndsWith("Code Genuine Validation/GenuineValidator.unity"))
				{
					exampleBuilt = false;
					break;
				}
			}

			if (!exampleBuilt)
			{
				return;
			}

			// make sure current platform is Windows Standalone
#if !UNITY_STANDALONE_WIN
			Debug.LogError("Please switch to Standalone Windows platform in order to use full GenuineValidator example.");
#else
			// make sure hash generation enabled in settings
			if (!ACTkSettings.Instance.PreGenerateBuildHash)
			{
				Debug.LogError("Please enable code hash generation on build in the ACTk Settings in order to use full GenuineValidator example.");
				return;
			}

			// just subscribing to the hash generation event
			CodeHashGeneratorPostprocessor.Instance.HashesGenerated += OnHashesGenerated;
#endif
		}

		private static void OnHashesGenerated(BuildReport report, BuildHashes[] hashedBuilds)
		{
			Debug.Log("CodeHashGeneratorListener example listener saying hello.");

			var whitelistedHashes = string.Empty;

			// Upload hashes to the server or do anything you would like to.
			//
			// Note, you may have multiple builds each with own hashes in some cases after build,
			// e.g. when using "Split APKs by target architecture" option.
			foreach (var hashedBuild in hashedBuilds)
			{
				hashedBuild.PrintToConsole();

				whitelistedHashes += hashedBuild.SummaryHash + GenuineValidatorExample.Separator;

				var fileHashes = hashedBuild.FileHashes;
				var fileHashesLength = fileHashes.Length;
				for (var i = 0; i < fileHashesLength; i++)
				{
					var fileHash = fileHashes[i];
					whitelistedHashes += fileHash.Hash;

					if (i != fileHashesLength - 1)
					{
						whitelistedHashes += GenuineValidatorExample.Separator;
					}
				}
			}

			// for example, you may put hashes next to the standalone build to compare them offline
			// just as a proof of concept, but please consider uploading your hashes to the server
			// and make comparison on the server-side instead when possible to add cheaters some more pain

			var outputFolder = Path.GetDirectoryName(report.summary.outputPath);
			if (string.IsNullOrEmpty(outputFolder) || !Directory.Exists(outputFolder))
			{
				Debug.LogError(ACTkConstants.LogPrefix + "Couldn't find build folder!");
				return;
			}

			var filePath = Path.Combine(outputFolder, GenuineValidatorExample.FileName);

			// encrypt to hide hashes from the eye
			var encryptedValue = ObscuredString.Encrypt(whitelistedHashes, GenuineValidatorExample.StringKey);

			// now just get raw bytes and write them to the file to compare hashes in runtime
			var bytes = GenuineValidatorExample.UnicodeCharsToBytes(encryptedValue);
			File.WriteAllBytes(filePath, bytes);
		}
	}
}
#endif