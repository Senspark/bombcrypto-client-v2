#region copyright
// --------------------------------------------------------------
//  Copyright (C) Dmitriy Yukhanov - focus [http://codestage.net]
// --------------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.Genuine.CodeHash
{
	using Common;
	using System;
	using System.Collections.Generic;
	using System.Security.Cryptography;
	using UnityEngine;
	using Utils;

	/// <summary>
	/// Contains hashes for the application build.
	/// </summary>
	public class BuildHashes
	{
		/// <summary>
		/// Path to the build file or folder.
		/// </summary>
		public string BuildPath { get; }

		/// <summary>
		/// Contains all sensitive files hashes and relative paths.
		/// </summary>
		public FileHash[] FileHashes { get; }

		/// <summary>
		/// Summary hash for all files in build.
		/// </summary>
		/// Use with caution: summary hash for runtime build may differ from the summary hash
		/// you got in Editor, for example, for Android App Bundles.
		/// Use #FileHashes for more accurate hashes comparison control.
		public string SummaryHash { get; }

		internal BuildHashes(string buildPath, List<FileHash> fileHashes, SHA1Managed sha1)
		{
			fileHashes.Sort((x, y) => string.Compare(x.Hash, y.Hash, StringComparison.Ordinal));

			BuildPath = buildPath;
			SummaryHash = CalculateSummaryCodeHash(fileHashes, sha1);
			FileHashes = fileHashes.ToArray();
		}

		internal BuildHashes(string buildPath, FileHash[] fileHashes, string summaryHash)
		{
			BuildPath = buildPath;
			SummaryHash = summaryHash;
			FileHashes = fileHashes;
		}

		/// <summary>
		/// Checks is passes hash exists in file hashes of this instance.
		/// </summary>
		/// <param name="hash">Target file hash.</param>
		/// <returns>True if such hash presents at #FileHashes and false otherwise.</returns>
		public bool HasFileHash(string hash)
		{
			foreach (var fileHash in FileHashes)
			{
				if (fileHash.Hash == hash)
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Sends enclosing hashes to the console along with file names.
		/// </summary>
		public void PrintToConsole()
		{
			var log = ACTkConstants.LogPrefix + "Build hashed: " + BuildPath + "\n" +
			          "Summary hash: " + SummaryHash +"\nFiles:";
			foreach (var fileHash in FileHashes)
			{
				log += "\n" + fileHash.Path + " : " + fileHash.Hash;
			}

			Debug.Log(log);
		}

		private string CalculateSummaryCodeHash(List<FileHash> fileHashes, SHA1Managed sha1)
		{
			var hashesString = string.Empty;
			foreach (var fileHash in fileHashes)
			{
				hashesString += fileHash.Hash;
			}

			var hashesBytes = StringUtils.StringToBytes(hashesString);
			var codeHashBytes = sha1.ComputeHash(hashesBytes);
			return StringUtils.HashBytesToHexString(codeHashBytes);
		}
	}
}