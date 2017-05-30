using System;
using System.Security.Cryptography;
using System.IO;

namespace Active.Activities.XamlProviders
{

	/// <summary>
	/// This class uses a symmetric key algorithm (Rijndael/AES) to encrypt and 
	/// decrypt data. As long as encryption and decryption routines use the same
	/// parameters to generate the keys, the keys are guaranteed to be the same.
	/// </summary>
	public static class RijndaelAES
	{
		/// <summary>
		/// This is the value that will be used to salt the password if applySalt is specified.  
		/// It has a default value if you do not set this static instance.
		/// </summary>
		public static string Salt { get; set; }

		/// <summary>
		/// Static constructor simply initializes <see cref="Salt"/> with a default value.
		/// </summary>
		static RijndaelAES()
		{
			Salt = "l;jkasdf0u2345kjnmnasd#$^hj;aksjg[p80()7ukl;nm2345kljSDGadsfg)&*Unkl/asdthi346kljdfg";
		}

		/// <summary>
		/// Use AES to encrypt data string with no encoding or salting.
		/// The same <paramref name="password"/> must be used to decrypt the string.
		/// </summary>
		/// <param name="data">Clear string to encrypt.</param>
		/// <param name="password">Password used to encrypt the string.</param>
		/// <returns>Encrypted result without any encoding.</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="data"/> or <paramref name="password"/> is null.</exception>
		public static string Encrypt(string password, string data)
		{
			return Encrypt(data, password, Encoding.CharacterEncoding.None, false);
		}

		/// <summary>
		/// Use AES to encrypt data string and optionally encode it afterwards.
		/// The same <paramref name="password"/> and <see cref="Salt"/> must be used to decrypt the string.
		/// </summary>
		/// <param name="data">Clear string to encrypt.</param>
		/// <param name="password">Password used to encrypt the string.</param>
		/// <param name="characterEncoding">
		///		Determines what the post encryption encoding of <paramref name="data"/> is.
		///		Setting this value to anything but <see cref="CharacterEncoding.None"/> will encode the resulting
		///		encrypted value with the specified encoding.
		///		When encrypting, this value needs to be exactly the same as used for decryption.
		/// </param>
		/// <param name="applySalt">
		///		Determines whether the key is salted (transformed) before being used for extra security against brute force attacks. 
		///		When decrypting, this value needs to be exactly the same as used for encryption.
		/// </param>
		/// <returns>Encrypted result encode as specified in <paramref name="characterEncoding"/>.</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="data"/> or <paramref name="password"/> is null.</exception>
		public static string Encrypt(string password, string data, Encoding.CharacterEncoding characterEncoding, bool applySalt)
		{
			if (data == null)
				throw new ArgumentNullException("data");
			if (password == null)
				throw new ArgumentNullException("password");

			//In addition to applying a salt value, we're also changing the data by appending the datetime.  This will be truncated
			//when decrypting but just makes the encryption a bit more random.
			if (applySalt)
			{
				data += DateTime.Now.ToString("dd MM yyyy HH:mm:ss");
			}

			byte[] encBytes = EncryptData(System.Text.Encoding.GetEncoding(1252).GetBytes(data), password, PaddingMode.ISO10126, applySalt);

			string retVal = string.Empty;

			switch (characterEncoding)
			{
				case Encoding.CharacterEncoding.Base64:
					retVal = Convert.ToBase64String(encBytes);
					break;
				case Encoding.CharacterEncoding.Hex:
					retVal = HexUtil.ToString(encBytes);
					break;
				default:
					retVal = System.Text.Encoding.GetEncoding(1252).GetString(encBytes);
					break;
			}

			return retVal;
		}

		/// <summary>
		/// Decrypts a string that was encrypted using RijndalAES algorithm with no post-encryption encoding or salting.
		/// </summary>
		/// <param name="data">Encrypted data generated from EncryptData method.</param>
		/// <param name="password">Password used to decrypt the string.</param>
		/// <returns>Decrypted string.</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="data"/> or <paramref name="password"/> is null.</exception>
		/// <exception cref="Exception">If the decryption fails for any reason.</exception>
		public static string Decrypt(string password, string data)
		{
			return Decrypt(data, password, Encoding.CharacterEncoding.None, false);
		}

		/// <summary>
		/// Decrypts a string that was encrypted using RijndalAES algorithm with the specified post-encryption encoding.
		/// </summary>
		/// <param name="data">Encrypted data generated from EncryptData method.</param>
		/// <param name="password">Password used to decrypt the string.</param>
		/// <param name="characterEncoding">
		///		Determines what the post encryption encoding of <paramref name="data"/> is.
		///		When encrypting, this value needs to be exactly the same as used for decryption.
		/// </param>
		/// <param name="applySalt">
		///		Determines whether the key is salted (transformed) before being used for extra security against brute force attacks. 
		///		When encrypting, this value needs to be exactly the same as used for decryption.
		/// </param>
		/// <returns>Decrypted string.</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="data"/> or <paramref name="password"/> is null.</exception>
		/// <exception cref="Exception">If the decryption fails for any reason.</exception>
		public static string Decrypt(string password, string data, Encoding.CharacterEncoding characterEncoding, bool applySalt)
		{
			if (data == null)
				throw new ArgumentNullException("data");
			if (password == null)
				throw new ArgumentNullException("password");

			string decodedString = null;
			try
			{
				byte[] encBytes;
				switch (characterEncoding)
				{
					case Encoding.CharacterEncoding.Base64:
						encBytes = Convert.FromBase64String(data);
						break;
					case Encoding.CharacterEncoding.Hex:
						encBytes = HexUtil.GetBytes(data);
						break;
					default:
						encBytes = System.Text.Encoding.GetEncoding(1252).GetBytes(data);
						break;
				}

				byte[] decBytes = DecryptData(encBytes, password, PaddingMode.ISO10126, applySalt);
				decodedString = System.Text.Encoding.GetEncoding(1252).GetString(decBytes);

				//When salting, remove the datetime we added at the end of the string.
				if (applySalt)
				{
					decodedString = decodedString.Substring(0, decodedString.Length - 19);
				}
			}
			catch (Exception ex)
			{
				throw new Exception("The encrypted string was not in a valid format.", ex);
			}

			return decodedString;
		}

		/// <summary>
		/// Applies the RijndalAES encryption algorithm to a byte array.
		/// </summary>
		/// <param name="data">Byte array to encrypt.</param>
		/// <param name="password">Password used to encrypt data.</param>
		/// <param name="paddingMode">Padding Mode to apply.</param>
		/// <param name="applySalt">If set to false, the salt will be String.Empty.</param>
		/// <returns>
		/// A byte array transformed with the RijndalAES encryption algorithm.
		/// </returns>
		private static byte[] EncryptData(byte[] data, string password, PaddingMode paddingMode, bool applySalt)
		{
			if (data == null || data.Length == 0)
				throw new ArgumentNullException("data");
			if (password == null)
				throw new ArgumentNullException("password");
			PasswordDeriveBytes pdb = new PasswordDeriveBytes(password, System.Text.Encoding.GetEncoding(1252).GetBytes((applySalt ? Salt : "")));
			RijndaelManaged rm = new RijndaelManaged();
			rm.Padding = paddingMode;
			ICryptoTransform encryptor = rm.CreateEncryptor(pdb.GetBytes(16), pdb.GetBytes(16));
			using (MemoryStream msEncrypt = new MemoryStream())
			using (CryptoStream encStream = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
			{
				encStream.Write(data, 0, data.Length);
				encStream.FlushFinalBlock();
				return msEncrypt.ToArray();
			}
		}

		/// <summary>
		/// Applies the RijndalAES decryption algorithm to a byte array.
		/// </summary>
		/// <param name="data">Byte array to decrypt.</param>
		/// <param name="password">Password used to encrypt data.</param>
		/// <param name="paddingMode">Padding Mode to apply.</param>
		/// <param name="applySalt">If set to false, the salt will be String.Empty.</param>
		/// <returns>
		/// A byte array transformed with the RijndalAES decryption algorithm.
		/// </returns>		
		private static byte[] DecryptData(byte[] data, string password, PaddingMode paddingMode, bool applySalt)
		{
			if (data == null || data.Length == 0)
				throw new ArgumentNullException("data");
			if (password == null)
				throw new ArgumentNullException("password");
			PasswordDeriveBytes pdb = new PasswordDeriveBytes(password, System.Text.Encoding.GetEncoding(1252).GetBytes((applySalt ? Salt : "")));
			RijndaelManaged rm = new RijndaelManaged();
			rm.Padding = paddingMode;
			ICryptoTransform decryptor = rm.CreateDecryptor(pdb.GetBytes(16), pdb.GetBytes(16));
			using (MemoryStream msDecrypt = new MemoryStream(data))
			using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
			{
				// Decrypted bytes will always be less then encrypted bytes, so len of encrypted data will be big enough for buffer.
				byte[] fromEncrypt = new byte[data.Length];                // Read as many bytes as possible.
				int read = csDecrypt.Read(fromEncrypt, 0, fromEncrypt.Length);
				if (read < fromEncrypt.Length)
				{
					// Return a byte array of proper size.
					byte[] clearBytes = new byte[read];
					Buffer.BlockCopy(fromEncrypt, 0, clearBytes, 0, read);
					return clearBytes;
				}
				return fromEncrypt;
			}
		}
	}

}
