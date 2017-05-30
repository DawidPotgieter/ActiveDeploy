using System;
using System.Text;

namespace Active.Activities.XamlProviders
{
	/// <summary>
	/// Utility class for doing Hex encoding/decoding
	/// </summary>
	public sealed class HexUtil
	{

		/// <summary>
		/// Holds the hex characters for a fast lookup
		/// </summary>
		private static readonly char[] HEX_CHARS = "0123456789ABCDEF".ToCharArray();
		//Keep the array sorted for faster search
		static HexUtil()
		{
			Array.Sort(HEX_CHARS);
		}

		/// <summary>
		/// No explicit client creation needed/allowed
		/// </summary>
		private HexUtil()
		{
		}


		/// <summary>
		/// Creates a byte array from the hexadecimal string. Each two characters are combined
		/// to create one byte. First two hexadecimal characters become first byte in returned array.
		/// If input hex string contains an odd number of bytes, then last character is dropped to
		/// make the returned byte array as even bytes. It raises ArgumentException, if a
		/// Non-hexadecimal character is encountered while processing the string.
		/// </summary>
		/// <param name="hexString">String to convert to byte array. It should contain only
		/// Hex chars [0-9 a-f A-F] only, else error will be raised. See description above </param>
		/// <returns>byte array, in the same left-to-right order as the hexString</returns>
		///
		public static byte[] GetBytes(string hexString)
		{

			if (!IsValidHexCharLength(hexString))
			{
				throw new ArgumentException("Invalid hexString size");
			}

			//check for non hex characters
			StringBuilder tempString = new StringBuilder(hexString.Length);
			foreach (char c in hexString)
			{
				if (IsHexChar(c))
				{
					tempString.Append(c);
				}
				else
				{
					throw new ArgumentException("Non Hexadecimal character '" + c + "' in hexString");
				}
			}

			string verifiedHexString = tempString.ToString();
			tempString = null;

			//check for valid length. If number of characters is odd in the hex string then
			//drop the last character
			if ((verifiedHexString.Length % 2) != 0)
			{
				verifiedHexString = verifiedHexString.Substring(0, verifiedHexString.Length - 1);
			}


			//Convert each hex character to byte
			//Hex byte length is half of actual ascii byte length
			int byteArrayLength = verifiedHexString.Length / 2;
			byte[] hexbytes = new byte[byteArrayLength];
			string tmp_substring = null;
			try
			{
				for (int i = 0; i < hexbytes.Length; i++)
				{
					int charIndex = i * 2;
					tmp_substring = verifiedHexString.Substring(charIndex, 2);
					hexbytes[i] = Convert.ToByte(tmp_substring, 16);
				}
			}
			catch (FormatException ex)
			{
				throw new ArgumentException("hexString must be a valid hexadecimal", ex);
			}

			return hexbytes;
		}



		/// <summary>
		/// Converts the input bytes array to a String assuming each character as a hexadecimal
		/// character. Uses .NET Byte.ToString("X2") implementation to achieve the encoding
		/// </summary>
		/// <param name="bytes">hex encoded bytes to be converted to the string</param>
		/// <returns>A string representation of the input bytes assuming Hex encoding</returns>
		public static string ToString(byte[] bytes)
		{

			if (bytes == null || bytes.Length == 0)
			{
				throw new ArgumentNullException("bytes");
			}

			StringBuilder hexString = new StringBuilder();
			foreach (byte byt in bytes)
			{
				hexString.Append(byt.ToString("X2"));
			}
			return hexString.ToString();
		}


		/// <summary>
		/// Determines if given string is in proper hexadecimal string format
		/// </summary>
		/// <param name="hexString">string to be tested for valid hexadecimal content</param>
		/// <returns>true if hexString contains only hex characters and
		/// atleast two characters long else false.</returns>
		public static bool IsHexString(string hexString)
		{

			bool hexFormat = IsValidHexCharLength(hexString);
			if (hexFormat)
			{
				foreach (char ch in hexString)
				{
					if (!IsHexChar(ch))
					{
						hexFormat = false;
						break;
					}
				}
			}
			return hexFormat;
		}

		/// <summary>
		/// Checks to see if the input string is not null and atleast two
		/// characters long
		/// </summary>
		/// <param name="hexString">string to be tested</param>
		/// <returns>true if hexString is not null and atleast two
		/// characters long</returns>
		private static bool IsValidHexCharLength(string hexString)
		{
			return ((hexString != null) && (hexString.Length >= 2));
		}

		/// <summary>
		/// Returns true if c is a hexadecimal character [A-F, a-f, 0-9]
		/// </summary>
		/// <param name="c">Character to test</param>
		/// <returns>true if c is a hex char, false if not</returns>
		public static bool IsHexChar(Char c)
		{
			c = Char.ToUpper(c);
			//look-up the char in HEX_CHARS Array
			bool isHexChar = (Array.BinarySearch(HEX_CHARS, c) >= 0);
			return isHexChar;
		}


		/// <summary>
		/// This is a utility method to convert any input byte[] to the requiredSize. If the input
		/// byte[] is smaller than requiredSize, then a new byte array is created where all lower order
		/// places are filled with input byte[] and balance places are filled with 0 (zeros). inputBytes
		/// array is truncated if it is larger than requiredSize.
		/// </summary>
		/// <param name="inputBytes">input byte aray</param>
		/// <param name="requiredSize">the size of the byte array to be created</param>
		/// <returns>a byte array of requiredSize</returns>
		private static byte[] CreateLegalByteArray(byte[] inputBytes, int requiredSize)
		{

			byte[] newBytes = null;
			int inputLength = inputBytes.Length;

			if (inputLength == requiredSize)
			{
				//nothing to do
				newBytes = inputBytes;
			}
			else
			{
				//create a new Byte array of reuired lenght and fill the content with
				//given byte[] starting from 0 index in the new byte[]
				newBytes = new byte[requiredSize];
				int len = newBytes.Length;
				if (len > inputLength)
				{
					len = inputLength;
				}
				Array.Copy(inputBytes, newBytes, len); //note: balance is filled with 0 (zero)
			}
			return newBytes;
		}
	}
}
