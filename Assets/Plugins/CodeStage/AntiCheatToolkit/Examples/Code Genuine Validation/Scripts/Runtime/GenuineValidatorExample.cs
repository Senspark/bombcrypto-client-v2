namespace CodeStage.AntiCheat.Examples.Genuine
{
	using System;
	using System.IO;
	using System.Security.Cryptography;
	using System.Text;
	using Utils;
	using UnityEngine;
	using ObscuredTypes;

#if UNITY_2018_1_OR_NEWER
	using CodeStage.AntiCheat.Genuine.CodeHash;
#endif

	// use this to check hash generated with CodeHashGeneratorListener.cs example file
	// note: this is an example for the Windows Standalone platform only
	public class GenuineValidatorExample : MonoBehaviour
	{
		public static readonly char[] StringKey = {'\x674', '\x345', '\x856', '\x968', '\x322'};

		// let's choose some non-obvious file name which will not be hashed (not .dll or .exe)
		public const string FileName = "Textures.asset";

		// 💖 looks like a really lovely separator =)
		public const string Separator = "💖";

		public static readonly int SeparatorLength = Separator.Length;

		private string status;

		// just an unoptimized example of SHA1 hashing
		public static string GetHash(string firstBuildHash)
		{
			var stringBytes = StringUtils.StringToBytes(firstBuildHash);
			var sha1 = new SHA1Managed();
			var hash = sha1.ComputeHash(stringBytes);
			sha1.Clear();
			return StringUtils.HashBytesToHexString(hash);
		}

		private void Awake()
		{
#if UNITY_2018_1_OR_NEWER
			CodeHashGenerator.HashGenerated += OnGotHash;
			status = "Press Check";
#else
			status = "Unity 2018.1 or newer required!";
#endif
		}

		private void OnGUI()
		{
#if UNITY_2018_1_OR_NEWER
			if (GUILayout.Button("Check"))
			{
				OnCheckHashClick();
			}
#endif
			GUILayout.Label(status);
		}

#if UNITY_2018_1_OR_NEWER
		private void OnCheckHashClick()
		{
			status = "Checking...";
			CodeHashGenerator.Generate();
		}

		private void OnGotHash(HashGeneratorResult result)
		{
			if (!result.Success)
			{
				status = "Error: " + result.ErrorMessage;
				return;
			}

			var filePath = Path.Combine(Path.GetFullPath(Application.dataPath + @"\..\"), FileName);
			if (!File.Exists(filePath))
			{
				status = "No super secret file found, you're cheater!\n" + filePath;
				return;
			}

			var allBytes = File.ReadAllBytes(filePath);
			var allChars = BytesToUnicodeChars(allBytes);
			var decrypted = ObscuredString.Decrypt(allChars, StringKey);

			var separatorIndex = decrypted.IndexOf(Separator, StringComparison.InvariantCulture);
			if (separatorIndex == -1)
			{
				status = "Super secret file is corrupted, you're cheater!";
				return;
			}

			var whitelistedHashes = decrypted.Split(new[] { Separator }, StringSplitOptions.RemoveEmptyEntries);
			var originalSummaryHash = whitelistedHashes[0];

			// compare summary hashes first
			if (originalSummaryHash != result.SummaryHash)
			{
				// check all files against whitelisted hashes if summary differs
				// (it can differ if some files are absent due to build separation)
				for (var i = 1; i < whitelistedHashes.Length; i++)
				{
					var hash = whitelistedHashes[i];
					if (!result.HasFileHash(hash))
					{
						status = "Code hash differs, you're cheater!\nSummary hashes:\n" + originalSummaryHash + "\n" + result.SummaryHash + "\nWhitelisted hashes count: " + whitelistedHashes.Length;
						return;
					}
				}
			}

			status = "All fine!";
		}

		public static char[] BytesToUnicodeChars(byte[] input)
		{
			var chars = new char[input.Length / sizeof(char)];
			Buffer.BlockCopy(input, 0, chars, 0, input.Length);
			return chars;
		}

		public static byte[] UnicodeCharsToBytes(char[] input)
		{
			var bytes = new byte[input.Length * sizeof(char)];
			Buffer.BlockCopy(input, 0, bytes, 0, bytes.Length);
			return bytes;
		}
#endif
	}
}