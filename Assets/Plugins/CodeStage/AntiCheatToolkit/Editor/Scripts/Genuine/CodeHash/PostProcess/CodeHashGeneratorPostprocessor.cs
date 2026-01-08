#region copyright
// --------------------------------------------------------------
//  Copyright (C) Dmitriy Yukhanov - focus [http://codestage.net]
// --------------------------------------------------------------
#endregion

#if UNITY_2018_1_OR_NEWER

namespace CodeStage.AntiCheat.EditorCode.PostProcessors
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.IO;
	using System.Security.Cryptography;
	using Common;
	using Genuine.CodeHash;
	using ICSharpCode.SharpZipLib.Zip;
	using UnityEditor;
	using UnityEditor.Build;
	using UnityEditor.Build.Reporting;
	using Utils;
	using Debug = UnityEngine.Debug;

	/// <summary>
	/// Does calculates code hash after build if you use option "Generate code hash".
	/// Listen to HashesGenerate or look for hash for each build in the Editor Console.
	/// </summary>
	/// Resulting hash in most cases should match value you get from the \ref CodeStage.AntiCheat.Genuine.CodeHash.CodeHashGenerator "CodeHashGenerator"
	public class CodeHashGeneratorPostprocessor : IPostprocessBuildWithReport
	{
		/// <summary>
		/// Use to subscribe to the HashesGenerated event.
		/// </summary>
		public static CodeHashGeneratorPostprocessor Instance { get; private set; }

		public delegate void OnHashesGenerate(BuildReport report, Dictionary<string, string> buildHashes);

		[Obsolete("Please use OnHashesGenerated instead.")]
		public event OnHashesGenerate HashesGenerate;

		/// <summary>
		/// HashesGenerate event delegate.
		/// </summary>
		/// <param name="report">Standard post-build report from Unity.</param>
		/// <param name="hashedBuilds">Build hashing results array.</param>
		///
		/// You may generate multiple actual builds within single build operation,
		/// like multiple APKs when you use "Split APKs by target architecture" option,
		/// so you may have more than one valid hashed builds for one actual build procedure.
		public delegate void OnHashesGenerated(BuildReport report, BuildHashes[] hashedBuilds);

		/// <summary>
		/// You may listen to this event if you wish to post-process resulting code hash,
		/// e.g. upload it to the server for the later runtime check with CodeHashGenerator.
		/// </summary>
		public event OnHashesGenerated HashesGenerated;

		public CodeHashGeneratorPostprocessor()
		{
			Instance = this;
		}

		~CodeHashGeneratorPostprocessor()
		{
			if (Instance == this)
			{
				Instance = null;
			}

#pragma warning disable 618
			HashesGenerate = null;
#pragma warning restore 618

			HashesGenerated = null;
		}

		// to make sure this postprocessor will run as late as possible
		public int callbackOrder
		{
			get { return int.MaxValue; }
		}

		[Obsolete("Please use CalculateExternalBuildHashes() method instead.")]
		public static string CalculateExternalBuildHash(out string selectedBuildPath)
		{
			var result = CalculateExternalBuildHash();
			selectedBuildPath = result.Path;
			return result.Hash;
		}

		private static FileHash CalculateExternalBuildHash()
		{
			var result = CalculateExternalBuildHashes();
			if (result == null)
			{
				return null;
			}

			return new FileHash(result.BuildPath, result.SummaryHash);
		}

		/// <summary>
		/// Calls selection dialog and calculates hashes for the selected build.
		/// </summary>
		/// <returns>Valid BuildHashes instance or null in case of error / user cancellation.</returns>
		public static BuildHashes CalculateExternalBuildHashes()
		{
			var buildPath = EditorUtility.OpenFilePanel("Select Standalone Windows build exe or Android build apk / aab", "", "exe,apk,aab");
			if (string.IsNullOrEmpty(buildPath))
			{
				return null;
			}

			var extension = Path.GetExtension(buildPath);
			if (string.IsNullOrEmpty(extension))
			{
				return null;
			}

			extension = extension.ToLower(CultureInfo.InvariantCulture);

			BuildHashes result = null;
			var sha1 = new SHA1Managed();

			try
			{
				var il2Cpp = PlayerSettings.GetScriptingBackend(EditorUserBuildSettings.selectedBuildTargetGroup) ==
				             ScriptingImplementation.IL2CPP;

				if (extension == ".apk" || extension == ".aab")
				{
					result = GetAndroidArchiveHashes(buildPath, CodeHashGenerator.GetFileFiltersAndroid(il2Cpp), sha1);
				}
				else
				{
					var buildFolder = Path.GetDirectoryName(buildPath);
					var filters = CodeHashGenerator.GetFileFiltersStandaloneWindows(il2Cpp);
					result = StandaloneWindowsWorker.GetBuildHashes(buildFolder, filters, sha1);
				}
			}
			catch (Exception e)
			{
				Debug.LogError(ACTkConstants.LogPrefix + "Error while trying to hash build: " + e);
			}
			finally
			{
				sha1.Clear();
				EditorUtility.ClearProgressBar();
			}

			return result;
		}

		// called by Unity
		public void OnPostprocessBuild(BuildReport report)
		{
			if (!ACTkSettings.Instance.PreGenerateBuildHash || !CodeHashGenerator.IsTargetPlatformCompatible())
			{
				return;
			}

			try
			{
				EditorUtility.DisplayProgressBar("ACTk: Generating code hash", "Preparing...", 0);
				var hashedBuilds = GetHashedBuilds(report);

				if (hashedBuilds == null || hashedBuilds.Count == 0)
				{
					Debug.Log(ACTkConstants.LogPrefix + "Couldn't pre-generate code hash. " +
					          "Please run your build and generate hash with CodeHashGenerator.");
					return;
				}

				foreach (var hashedBuild in hashedBuilds)
				{
					hashedBuild.PrintToConsole();
				}

#pragma warning disable 618
				if (HashesGenerate != null)
				{
					var obsoleteDictionary = new Dictionary<string, string>();
					foreach (var hashedBuild in hashedBuilds)
					{
						obsoleteDictionary.Add(hashedBuild.BuildPath, hashedBuild.SummaryHash);
					}
					HashesGenerate.Invoke(report, obsoleteDictionary);
				}
#pragma warning restore 618

				if (HashesGenerated != null)
				{
					HashesGenerated.Invoke(report, hashedBuilds.ToArray());
				}
			}
			catch (Exception e)
			{
				Debug.LogError(e);
			}
			finally
			{
				EditorUtility.ClearProgressBar();
			}
		}

		private List<BuildHashes> GetHashedBuilds(BuildReport report)
		{
			var fileFilters = GetFileFilters();
			var sha1 = new SHA1Managed();
			List<BuildHashes> result = null;
#if UNITY_ANDROID
			result = GetAndroidBuildHashes(report, fileFilters, sha1);
#elif UNITY_STANDALONE_WIN
			result = GetStandaloneWindowsBuildHashes(report, fileFilters, sha1);
#endif
			sha1.Clear();
			return result;
		}

		private static FileFilter[] GetFileFilters()
		{
			var il2Cpp = false;
#if UNITY_EDITOR
			il2Cpp = PlayerSettings.GetScriptingBackend(EditorUserBuildSettings.selectedBuildTargetGroup) == ScriptingImplementation.IL2CPP;
#elif ENABLE_IL2CPP
			il2Cpp = true;
#endif

#if UNITY_ANDROID
			return CodeHashGenerator.GetFileFiltersAndroid(il2Cpp);
#elif UNITY_STANDALONE_WIN
			return CodeHashGenerator.GetFileFiltersStandaloneWindows(il2Cpp);
#else
			return null;
#endif
		}

		// --------------------------------------------------------------
		// Android build post-processing
		// --------------------------------------------------------------

		private List<BuildHashes> GetAndroidBuildHashes(BuildReport report, FileFilter[] fileFilters, SHA1Managed sha1)
		{
			var result = new List<BuildHashes>();

			foreach (var reportFile in report.GetFiles())
			{
				var path = reportFile.path;
				var extension = Path.GetExtension(path);
				if (!string.IsNullOrEmpty(extension))
				{
					extension = extension.ToLower(CultureInfo.InvariantCulture);
				}

				if (extension == ".apk" || extension == ".aab")
				{
					var hash = GetAndroidArchiveHashes(path, fileFilters, sha1);
					result.Add(hash);
				}
			}

			if (result.Count == 0)
			{
				Debug.LogWarning(EditorTools.ConstructError("Couldn't find compiled APK or AAB build.\n" +
				                                            "This is fine if you use Export Project feature. In other case:"));
			}

			return result;
		}

		private static BuildHashes GetAndroidArchiveHashes(string path, FileFilter[] fileFilters, SHA1Managed sha1)
		{
			var fileHashes = HashSuitableFilesInZipFile(path, fileFilters, sha1);
			return new BuildHashes(path, fileHashes, sha1);
		}

		private static List<FileHash> HashSuitableFilesInZipFile(string path, FileFilter[] fileFilters, SHA1Managed sha1)
		{
			ZipFile zf = null;

			var fileHashes = new List<FileHash>();

			try
			{
				var fs = File.OpenRead(path);
				zf = new ZipFile(fs);

				var i = 0f;

				foreach (ZipEntry zipEntry in zf)
				{
					i++;

					if (!zipEntry.IsFile)
					{
						continue;
					}

					var entryFileName = zipEntry.Name;
					var suitableFile = false;

					foreach (var fileFilter in fileFilters)
					{
						if (fileFilter.MatchesPath(entryFileName))
						{
							suitableFile = true;
							break;
						}
					}

					if (!suitableFile) continue;

					EditorUtility.DisplayProgressBar("ACTk: Generating code hash", "Hashing files...", i / zf.Count);

					var zipStream = zf.GetInputStream(zipEntry);

					var hash = sha1.ComputeHash(zipStream);
					var hashString = StringUtils.HashBytesToHexString(hash);
					fileHashes.Add(new FileHash(entryFileName, hashString));
					//Debug.Log("Path: " + zipEntry.Name + "\nHash: " + hashString);
					zipStream.Close();
				}
			}
			catch (Exception e)
			{
				Debug.LogError(ACTkConstants.LogPrefix + "Error while calculating code hash: " + e);
				return null;
			}
			finally
			{
				if (zf != null)
				{
					zf.IsStreamOwner = true;
					zf.Close();
				}
			}

			return fileHashes;
		}

#if UNITY_STANDALONE_WIN
		// --------------------------------------------------------------
		// Standalone Windows build post-processing
		// --------------------------------------------------------------
		private List<BuildHashes> GetStandaloneWindowsBuildHashes(BuildReport report, FileFilter[] fileFilters, SHA1Managed sha1)
		{
			var result = new List<BuildHashes>();
			var folder = Path.GetDirectoryName(report.summary.outputPath);
			if (folder == null)
			{
				Debug.LogError(EditorTools.ConstructError("Could not found build folder for this file: " + report.summary.outputPath));
				return result;
			}

			var buildHashes = StandaloneWindowsWorker.GetBuildHashes(folder, fileFilters, sha1);
			if (buildHashes == null)
			{
				return result;
			}

			result.Add(buildHashes);

			return result;
		}
#endif
	}
}

#endif