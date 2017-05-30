using System;

namespace Active.Activities.XamlProviders
{
	/// <summary>
	/// A wrapper class for easily handling string->string conversions in different encodings.
	/// </summary>
	public class Encoding
	{
		private Encoding()
		{
		}

		/// <summary>
		/// This enumeration defines the post encryption encodings that can be applied.
		/// </summary>
		public enum CharacterEncoding
		{
			/// <summary>
			/// No Encoding.  The resulting string will contain ASCII values.
			/// </summary>
			None,
			/// <summary>
			/// Base64 Encoding
			/// </summary>
			Base64,
			/// <summary>
			/// Hex (Base 16) Encoding.
			/// </summary>
			Hex,
		}

		/// <summary>
		/// Encode a string to Base64.
		/// </summary>
		/// <param name="data">The string to encode.</param>
		/// <returns>
		/// This method returns a Base64 encoded string.
		/// </returns>
		/// <exception cref="Exception">If any exception occurs.</exception>
		/// <remarks>
		/// Uses UTF8 Text Encoding.
		/// </remarks>
		public static string Base64Encode(string data)
		{
			try
			{
				byte[] encData_byte = new byte[data.Length];
				encData_byte = System.Text.Encoding.UTF8.GetBytes(data);
				string encodedData = Convert.ToBase64String(encData_byte);
				return encodedData;
			}
			catch (Exception e)
			{
				throw new Exception("Error in Base64Encode " + e.Message, e);
			}
		}

		/// <summary>
		/// Decodes a string that is Base64 encoded.
		/// </summary>
		/// <param name="data">The Base64 encoded string to decode.</param>
		/// <returns>
		/// This method returns a string that is not Base64 encoded.
		/// </returns>
		/// <exception cref="Exception">If any error occurs.</exception>
		/// <remarks>
		/// Uses UTF8 Text Encoding.
		/// </remarks>
		public static string Base64Decode(string data)
		{
			try
			{
				System.Text.UTF8Encoding encoder = new System.Text.UTF8Encoding();
				System.Text.Decoder utf8Decode = encoder.GetDecoder();

				byte[] todecode_byte = Convert.FromBase64String(data);
				int charCount = utf8Decode.GetCharCount(todecode_byte, 0, todecode_byte.Length);
				char[] decoded_char = new char[charCount];
				utf8Decode.GetChars(todecode_byte, 0, todecode_byte.Length, decoded_char, 0);
				string result = new String(decoded_char);
				return result;
			}
			catch (Exception e)
			{
				throw new Exception("Error in Base64Decode " + e.Message, e);
			}
		}

		/// <summary>
		/// Decodes a string that is Base64 encoded.
		/// </summary>
		/// <param name="data">The Base64 encoded string (in bytes) to decode.</param>
		/// <returns>
		/// This method returns a string that is not Base64 encoded.
		/// </returns>
		/// <exception cref="Exception">If any error occurs.</exception>
		/// <remarks>
		/// Uses UTF8 Text Encoding.
		/// </remarks>
		public static string Base64Decode(byte[] data)
		{
			try
			{
				System.Text.UTF8Encoding encoder = new System.Text.UTF8Encoding();
				System.Text.Decoder utf8Decode = encoder.GetDecoder();

				byte[] todecode_byte = data;
				int charCount = utf8Decode.GetCharCount(todecode_byte, 0, todecode_byte.Length);
				char[] decoded_char = new char[charCount];
				utf8Decode.GetChars(todecode_byte, 0, todecode_byte.Length, decoded_char, 0);
				string result = new String(decoded_char);
				return result;
			}
			catch (Exception e)
			{
				throw new Exception("Error in Base64Decode " + e.Message, e);
			}
		}

		/// <summary>
		/// Encodes a string into a Hex (Base16) representation of the string.
		/// </summary>
		/// <param name="data">The string to encode.</param>
		/// <returns>
		/// This method returns a string that has been encoded using it's (Hex) Base16 represention.
		/// </returns>
		/// <exception cref="ArgumentNullException">If <paramref name="data"/> is null or empty.</exception>
		/// <remarks>
		/// Uses ASCII (Code page 1252) Text Encoding.
		/// </remarks>
		public static string HexStringEncode(string data)
		{
			return HexUtil.ToString(System.Text.Encoding.GetEncoding(1252).GetBytes(data));
		}

		/// <summary>
		/// Decodes a string that is Hex (Base16) encoded.
		/// </summary>
		/// <param name="data">The Hex (Base16) encoded string to decode.</param>
		/// <returns>
		/// This method returns a string that is not Hex (Base16) encoded.
		/// </returns>
		/// <exception cref="ArgumentException">If <paramref name="data"/> is null, empty or not in the correct format.</exception>
		/// <remarks>
		/// Uses ASCII (Code page 1252) Text Encoding.
		/// </remarks>
		public static string HexStringDecode(string data)
		{
			return System.Text.Encoding.GetEncoding(1252).GetString(HexUtil.GetBytes(data));
		}
	}
}
