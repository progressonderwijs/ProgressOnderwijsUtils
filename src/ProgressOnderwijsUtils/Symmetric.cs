using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;

///
///borrowed from http://www.4guysfromrolla.com/webtech/090501-1.shtml
///
///
///Thanks!
///

namespace progress.tools 
{
	public class Symmetric 
	{
		static private Byte[] m_Key = new Byte[8]; 
		static private Byte[] m_IV = new Byte[8]; 
		
		private const int CRYPTO_LENGTH = 512;
		//////////////////////////
		//Function to encrypt data
		public string EncryptData(String strKey, String strData)
		{
			string strResult;		//Return Result

			//1. String Length cannot exceed 90Kb. Otherwise, buffer will overflow. See point 3 for reasons
			if (strData.Length > 92160)
			{
				strResult="Error. Data String too large. Keep within 90Kb.";
				return strResult;
			}
		
			//2. Generate the Keys
			if (!InitKey(strKey))
			{
				strResult="Error. Fail to generate key for encryption";
				return strResult;
			}

			//3. Prepare the String
			//	The first 5 character of the string is formatted to store the actual length of the data.
			//	This is the simplest way to remember to original length of the data, without resorting to complicated computations.
			//	If anyone figure a good way to 'remember' the original length to facilite the decryption without having to use additional function parameters, pls let me know.
			strData = String.Format("{0,5:00000}"+strData, strData.Length);


			//4. Encrypt the Data
			byte[] rbData = new byte[strData.Length];
			ASCIIEncoding aEnc = new ASCIIEncoding();
			aEnc.GetBytes(strData, 0, strData.Length, rbData, 0);
			
			DESCryptoServiceProvider descsp = new DESCryptoServiceProvider(); 
			
			ICryptoTransform desEncrypt = descsp.CreateEncryptor(m_Key, m_IV); 


			//5. Perpare the streams:
			//	mOut is the output stream. 
			//	mStream is the input stream.
			//	cs is the transformation stream.
			MemoryStream mStream = new MemoryStream(rbData); 
			CryptoStream cs = new CryptoStream(mStream, desEncrypt, CryptoStreamMode.Read);        
			MemoryStream mOut = new MemoryStream();
			
			//6. Start performing the encryption
			int bytesRead; 
			byte[] output = new byte[CRYPTO_LENGTH]; 
			do 
			{ 
				bytesRead = cs.Read(output,0,CRYPTO_LENGTH);
				if (bytesRead != 0) 
					mOut.Write(output,0,bytesRead); 
			} while (bytesRead > 0); 
			
			//7. Returns the encrypted result after it is base64 encoded
			//	In this case, the actual result is converted to base64 so that it can be transported over the HTTP protocol without deformation.
			if (mOut.Length == 0)		
				strResult = "";
			else
				strResult = Convert.ToBase64String(mOut.GetBuffer(), 0, (int)mOut.Length);
		
			return strResult;
		}

		//////////////////////////
		//Function to decrypt data
		public string DecryptData(String strKey, String strData)
		{
			string strResult;

			//1. Generate the Key used for decrypting
			if (!InitKey(strKey))
			{
				strResult="Error. Fail to generate key for decryption";
				return strResult;
			}

			//2. Initialize the service provider
			int nReturn = 0;
			DESCryptoServiceProvider descsp = new DESCryptoServiceProvider(); 
			ICryptoTransform desDecrypt = descsp.CreateDecryptor(m_Key, m_IV); 
			
			//3. Prepare the streams:
			//	mOut is the output stream. 
			//	cs is the transformation stream.
			MemoryStream mOut = new MemoryStream();
			CryptoStream cs = new CryptoStream(mOut, desDecrypt, CryptoStreamMode.Write);        
			
			//4. Remember to revert the base64 encoding into a byte array to restore the original encrypted data stream
			byte[] bPlain = new byte[strData.Length];
			try 
			{
				bPlain = Convert.FromBase64CharArray(strData.ToCharArray(), 0, strData.Length);
			}
			catch (Exception) 
			{ 
				strResult = "Error. Input Data is not base64 encoded.";
				return strResult;
			}
			
			long lRead = 0;
			long lTotal = strData.Length;
			
			try
			{
				//5. Perform the actual decryption
				while (lTotal >= lRead)
				{ 
						cs.Write(bPlain,0,(int)bPlain.Length); 
						//descsp.BlockSize=64
						lRead = mOut.Length + Convert.ToUInt32(((bPlain.Length / descsp.BlockSize) * descsp.BlockSize));
				};
				
				ASCIIEncoding aEnc = new ASCIIEncoding();
				strResult = aEnc.GetString(mOut.GetBuffer(), 0, (int)mOut.Length);
				
				//6. Trim the string to return only the meaningful data
				//	Remember that in the encrypt function, the first 5 character holds the length of the actual data
				//	This is the simplest way to remember to original length of the data, without resorting to complicated computations.
				String strLen = strResult.Substring(0,5);
				int nLen = Convert.ToInt32(strLen);
				strResult = strResult.Substring(5, nLen);
				nReturn = (int)mOut.Length;
				
				return strResult;
			}
			catch (Exception e )
			{
				strResult = "Error. Decryption Failed. Possibly due to incorrect Key or corrupted data " + e.Message ;
				return strResult;
			}
			//Key or corrupted data Index and length must refer to a location within the string.Parameter name: length
		}

		/////////////////////////////////////////////////////////////
		//Private function to generate the keys into member variables
		static private bool InitKey(String strKey)
		{
			try
			{
				// Convert Key to byte array
				byte[] bp = new byte[strKey.Length];
				ASCIIEncoding aEnc = new ASCIIEncoding();
				aEnc.GetBytes(strKey, 0, strKey.Length, bp, 0);
				
				//Hash the key using SHA1
				SHA1CryptoServiceProvider sha = new SHA1CryptoServiceProvider();
				byte[] bpHash = sha.ComputeHash(bp);
				
				int i;
				// use the low 64-bits for the key value
				for (i=0; i<8; i++) 
					m_Key[i] = bpHash[i];
					
				for (i=8; i<16; i++) 
					m_IV[i-8] = bpHash[i];
				
				return true;
			}
			catch (Exception)
			{
				//Error Performing Operations
				return false;
			}
		}
	} // end symmetric class

	public class SimpleHash
	{
		/// <SUMMARY>
		/// Generates a hash for the given plain text value and returns a
		/// base64-encoded result. Before the hash is computed, a random salt
		/// is generated and appended to the plain text. This salt is stored at
		/// the end of the hash value, so it can be used later for hash
		/// verification.
		/// </SUMMARY>
		/// <PARAM name="plainText">
		/// Plaintext value to be hashed. The function does not check whether
		/// this parameter is null.
		/// </PARAM>
		/// <PARAM name="hashAlgorithm">
		/// Name of the hash algorithm. Allowed values are: "MD5", "SHA1",
		/// "SHA256", "SHA384", and "SHA512" (if any other value is specified
		/// MD5 hashing algorithm will be used). This value is case-insensitive.
		/// </PARAM>
		/// <PARAM name="saltBytes">
		/// Salt bytes. This parameter can be null, in which case a random salt
		/// value will be generated.
		/// </PARAM>
		/// <RETURNS>
		/// Hash value formatted as a base64-encoded string.
		/// </RETURNS>
		/// 
		public static string MD5ComputeHash(string   plainText)
		{
			return ComputeHash(plainText,"MD5",null);
		}

		public static string ComputeHash(string   plainText,			string   hashAlgorithm,			byte[]   saltBytes)
		{
			// If salt is not specified, generate it on the fly.
			if (saltBytes == null)
			{
				// Define min and max salt sizes.
				int minSaltSize = 4;
				int maxSaltSize = 8;

				// Generate a random number for the size of the salt.
				Random  random = new Random();
				int saltSize = random.Next(minSaltSize, maxSaltSize);

				// Allocate a byte array, which will hold the salt.
				saltBytes = new byte[saltSize];

				// Initialize a random number generator.
				RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();

				// Fill the salt with cryptographically strong byte values.
				rng.GetNonZeroBytes(saltBytes); 
			}
        
			// Convert plain text into a byte array.
			byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
        
			// Allocate array, which will hold plain text and salt.
			byte[] plainTextWithSaltBytes = 
				new byte[plainTextBytes.Length + saltBytes.Length];

			// Copy plain text bytes into resulting array.
			for (int i=0; i < plainTextBytes.Length; i++)
				plainTextWithSaltBytes[i] = plainTextBytes[i];
        
			// Append salt bytes to the resulting array.
			for (int i=0; i < saltBytes.Length; i++)
				plainTextWithSaltBytes[plainTextBytes.Length + i] = saltBytes[i];

			// Because we support multiple hashing algorithms, we must define
			// hash object as a common (abstract) base class. We will specify the
			// actual hashing algorithm class later during object creation.
			HashAlgorithm hash;
        
			// Make sure hashing algorithm name is specified.
			if (hashAlgorithm == null)
				hashAlgorithm = "";
        
			// Initialize appropriate hashing algorithm class.
			switch (hashAlgorithm.ToUpper())
			{
				case "SHA1":
					hash = new SHA1Managed();
					break;

				case "SHA256":
					hash = new SHA256Managed();
					break;

				case "SHA384":
					hash = new SHA384Managed();
					break;

				case "SHA512":
					hash = new SHA512Managed();
					break;

				default:
					hash = new MD5CryptoServiceProvider();
					break;
			}
        
			// Compute hash value of our plain text with appended salt.
			//byte[] hashBytes = hash.ComputeHash(plainTextWithSaltBytes);
			byte[] hashBytes = hash.ComputeHash(plainTextBytes);
        
			// Create array which will hold hash and original salt bytes.
			byte[] hashWithSaltBytes = new byte[hashBytes.Length + 
				saltBytes.Length];
        
			// Copy hash bytes into resulting array.
			for (int i=0; i < hashBytes.Length; i++)
				hashWithSaltBytes[i] = hashBytes[i];
          
			// Append salt bytes to the result.
			for (int i=0; i < saltBytes.Length; i++)
				hashWithSaltBytes[hashBytes.Length + i] = saltBytes[i];
            
			// Convert result into a base64-encoded string.
			//string hashString = Convert.ToBase64String(hashWithSaltBytes);
			
			// convert hash value to hex string
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			foreach(byte outputByte in hashBytes)
				// convert each byte to a Hexadecimal upper case string
				sb.Append(outputByte.ToString("x2"));


			string hashString = sb.ToString();
			// Return the result.
			return hashString;
		}

		/// <SUMMARY>
		/// Compares a hash of the specified plain text value to a given hash
		/// value. Plain text is hashed with the same salt value as the original
		/// hash.
		/// </SUMMARY>
		/// <PARAM name="plainText">
		/// Plain text to be verified against the specified hash. The function
		/// does not check whether this parameter is null.
		/// </PARAM>
		/// <PARAM name="hashAlgorithm">
		/// Name of the hash algorithm. Allowed values are: "MD5", "SHA1", 
		/// "SHA256", "SHA384", and "SHA512" (if any other value is specified,
		/// MD5 hashing algorithm will be used). This value is case-insensitive.
		/// </PARAM>
		/// <PARAM name="hashValue">
		/// Base64-encoded hash value produced by ComputeHash function. This value
		/// includes the original salt appended to it.
		/// </PARAM>
		/// <RETURNS>
		/// If computed hash mathes the specified hash the function the return
		/// value is true; otherwise, the function returns false.
		/// </RETURNS>
		/// 

		public static bool MD5VerifyHash(string   plainText,	string   hashString)
		{
			return VerifyHash(plainText,			"MD5"		,   hashString);
		}

		public static bool VerifyHash(string   plainText,			string   hashAlgorithm,			string   hashString)
		{
			// Convert base64-encoded hash value into a byte array.
			byte[] hashWithSaltBytes = Convert.FromBase64String(hashString);
        
			// We must know size of hash (without salt).
			int hashSizeInBits, hashSizeInBytes;
        
			// Make sure that hashing algorithm name is specified.
			if (hashAlgorithm == null)
				hashAlgorithm = "";
        
			// Size of hash is based on the specified algorithm.
			switch (hashAlgorithm.ToUpper())
			{
				case "SHA1":
					hashSizeInBits = 160;
					break;

				case "SHA256":
					hashSizeInBits = 256;
					break;

				case "SHA384":
					hashSizeInBits = 384;
					break;

				case "SHA512":
					hashSizeInBits = 512;
					break;

				default:
					hashSizeInBits = 128;
					break;
			}

			// Convert size of hash from bits to bytes.
			hashSizeInBytes = hashSizeInBits / 8;

			// Make sure that the specified hash value is long enough.
			if (hashWithSaltBytes.Length < hashSizeInBytes)
				return false;

			// Allocate array to hold original salt bytes retrieved from hash.
			byte[] saltBytes = new byte[hashWithSaltBytes.Length - 
				hashSizeInBytes];

			// Copy salt from the end of the hash to the new array.
			for (int i=0; i < saltBytes.Length; i++)
				saltBytes[i] = hashWithSaltBytes[hashSizeInBytes + i];

			// Compute a new hash string.
			string expectedHashString = 
				ComputeHash(plainText, hashAlgorithm, saltBytes);

			// If the computed hash matches the specified hash,
			// the plain text value must be correct.
			return (hashString == expectedHashString);
		}
	} // end SimpleHash

	public class DPAPI
	{

		#region dllimport
		[DllImport("Crypt32.dll", SetLastError=true,
			 CharSet=System.Runtime.InteropServices.CharSet.Auto)]
		private static extern bool CryptProtectData(
			ref DATA_BLOB pDataIn, 
			String szDataDescr, 
			ref DATA_BLOB pOptionalEntropy,
			IntPtr pvReserved, 
			ref CRYPTPROTECT_PROMPTSTRUCT 
			pPromptStruct, 
			int dwFlags, 
			ref DATA_BLOB pDataOut);
		[DllImport("Crypt32.dll", SetLastError=true, 
			 CharSet=System.Runtime.InteropServices.CharSet.Auto)]
		private static extern bool CryptUnprotectData(
			ref DATA_BLOB pDataIn, 
			String szDataDescr, 
			ref DATA_BLOB pOptionalEntropy, 
			IntPtr pvReserved, 
			ref CRYPTPROTECT_PROMPTSTRUCT 
			pPromptStruct, 
			int dwFlags, 
			ref DATA_BLOB pDataOut);
		[DllImport("kernel32.dll", 
			 CharSet=System.Runtime.InteropServices.CharSet.Auto)]
		private unsafe static extern int FormatMessage(int dwFlags, 
			ref IntPtr lpSource, 
			int dwMessageId,
			int dwLanguageId, 
			ref String lpBuffer, 
			int nSize,
			IntPtr *Arguments);

		#endregion

		[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
			internal struct DATA_BLOB
		{
			public int cbData;
			public IntPtr pbData;
		}

		[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
			internal struct CRYPTPROTECT_PROMPTSTRUCT
		{
			public int cbSize;
			public int dwPromptFlags;
			public IntPtr hwndApp;
			public String szPrompt;
		}
		static private IntPtr NullPtr = ((IntPtr)((int)(0)));
		private const int CRYPTPROTECT_UI_FORBIDDEN = 0x1;
		private const int CRYPTPROTECT_LOCAL_MACHINE = 0x4;


		public enum Store {USE_MACHINE_STORE = 1, USE_USER_STORE};

		private Store store;

		public DPAPI(Store tempStore)
		{
			store = tempStore;
		}

		public byte[] Encrypt(byte[] plainText, byte[] optionalEntropy)

		{
			bool retVal = false;
			DATA_BLOB plainTextBlob = new DATA_BLOB();
			DATA_BLOB cipherTextBlob = new DATA_BLOB();
			DATA_BLOB entropyBlob = new DATA_BLOB();
			CRYPTPROTECT_PROMPTSTRUCT prompt = new CRYPTPROTECT_PROMPTSTRUCT();
			InitPromptstruct(ref prompt);
			int dwFlags;
			try
			{
				try
				{
					int bytesSize = plainText.Length;
					plainTextBlob.pbData = Marshal.AllocHGlobal(bytesSize);
					if(IntPtr.Zero == plainTextBlob.pbData)
					{
						throw new Exception("Unable to allocate plaintext buffer.");
					}
					plainTextBlob.cbData = bytesSize;
					Marshal.Copy(plainText, 0, plainTextBlob.pbData, bytesSize);
				}
				catch(Exception ex)
				{
					throw new Exception("Exception marshalling data. " + ex.Message);
				}
				if(Store.USE_MACHINE_STORE == store)
				{//Using the machine store, should be providing entropy.
					dwFlags = CRYPTPROTECT_LOCAL_MACHINE|CRYPTPROTECT_UI_FORBIDDEN;
					//Check to see if the entropy is null
					if(null == optionalEntropy)
					{//Allocate something
						optionalEntropy = new byte[0];
					}
					try
					{
						int bytesSize = optionalEntropy.Length;
						entropyBlob.pbData = Marshal.AllocHGlobal(optionalEntropy.Length);;
						if(IntPtr.Zero == entropyBlob.pbData)
						{
							throw new Exception("Unable to allocate entropy data buffer.");
						}
						Marshal.Copy(optionalEntropy, 0, entropyBlob.pbData, bytesSize);
						entropyBlob.cbData = bytesSize;
					}
					catch(Exception ex)
					{
						throw new Exception("Exception entropy marshalling data. " + 
							ex.Message);
					}
				}
				else
				{//Using the user store
					dwFlags = CRYPTPROTECT_UI_FORBIDDEN;
				}
				retVal = CryptProtectData(ref plainTextBlob, "", ref entropyBlob, 
					IntPtr.Zero, ref prompt, dwFlags, 
					ref cipherTextBlob);
				if(false == retVal)
				{
					throw new Exception("Encryption failed. " + 
						GetErrorMessage(Marshal.GetLastWin32Error()));
				}
				//Free the blob and entropy.
				if(IntPtr.Zero != plainTextBlob.pbData)
				{
					Marshal.FreeHGlobal(plainTextBlob.pbData);
				}
				if(IntPtr.Zero != entropyBlob.pbData)
				{
					Marshal.FreeHGlobal(entropyBlob.pbData);
				}
			}
			catch(Exception ex)
			{
				throw new Exception("Exception encrypting. " + ex.Message);
			}
			byte[] cipherText = new byte[cipherTextBlob.cbData];
			Marshal.Copy(cipherTextBlob.pbData, cipherText, 0, cipherTextBlob.cbData);
			Marshal.FreeHGlobal(cipherTextBlob.pbData); 
			return cipherText;
		}

		public byte[] Decrypt(byte[] cipherText, byte[] optionalEntropy)
		{
			bool retVal = false;
			DATA_BLOB plainTextBlob = new DATA_BLOB();
			DATA_BLOB cipherBlob = new DATA_BLOB();
			CRYPTPROTECT_PROMPTSTRUCT prompt = new 
				CRYPTPROTECT_PROMPTSTRUCT();
			InitPromptstruct(ref prompt);
			try
			{
				try
				{
					int cipherTextSize = cipherText.Length;
					cipherBlob.pbData = Marshal.AllocHGlobal(cipherTextSize);
					if(IntPtr.Zero == cipherBlob.pbData)
					{
						throw new Exception("Unable to allocate cipherText buffer.");
					}
					cipherBlob.cbData = cipherTextSize;
					Marshal.Copy(cipherText, 0, cipherBlob.pbData, 
						cipherBlob.cbData);
				}
				catch(Exception ex)
				{
					throw new Exception("Exception marshalling data. " + 
						ex.Message);
				}
				DATA_BLOB entropyBlob = new DATA_BLOB();
				int dwFlags;
				if(Store.USE_MACHINE_STORE == store)
				{//Using the machine store, should be providing entropy.
					dwFlags = 
						CRYPTPROTECT_LOCAL_MACHINE|CRYPTPROTECT_UI_FORBIDDEN;
					//Check to see if the entropy is null
					if(null == optionalEntropy)
					{//Allocate something
						optionalEntropy = new byte[0];
					}
					try
					{
						int bytesSize = optionalEntropy.Length;
						entropyBlob.pbData = Marshal.AllocHGlobal(bytesSize);
						if(IntPtr.Zero == entropyBlob.pbData)
						{
							throw new Exception("Unable to allocate entropy buffer.");
						}
						entropyBlob.cbData = bytesSize;
						Marshal.Copy(optionalEntropy, 0, entropyBlob.pbData, 
							bytesSize);
					}
					catch(Exception ex)
					{
						throw new Exception("Exception entropy marshalling data. " + 
							ex.Message);
					}
				}
				else
				{//Using the user store
					dwFlags = CRYPTPROTECT_UI_FORBIDDEN;
				}
				retVal = CryptUnprotectData(ref cipherBlob, null, ref 
					entropyBlob, 
					IntPtr.Zero, ref prompt, dwFlags, 
					ref plainTextBlob);
				if(false == retVal)
				{
					throw new Exception("Decryption failed. " + 
						GetErrorMessage(Marshal.GetLastWin32Error()));
				}
				//Free the blob and entropy.
				if(IntPtr.Zero != cipherBlob.pbData)
				{
					Marshal.FreeHGlobal(cipherBlob.pbData);
				}
				if(IntPtr.Zero != entropyBlob.pbData)
				{
					Marshal.FreeHGlobal(entropyBlob.pbData);
				}
			}
			catch(Exception ex)
			{
				throw new Exception("Exception decrypting. " + ex.Message);
			}
			byte[] plainText = new byte[plainTextBlob.cbData];
			Marshal.Copy(plainTextBlob.pbData, plainText, 0, plainTextBlob.cbData);
			Marshal.FreeHGlobal(plainTextBlob.pbData); 
			return plainText;
		}

		private void InitPromptstruct(ref CRYPTPROTECT_PROMPTSTRUCT ps) 
		{
			ps.cbSize = Marshal.SizeOf(typeof(CRYPTPROTECT_PROMPTSTRUCT));
			ps.dwPromptFlags = 0;
			ps.hwndApp = NullPtr;
			ps.szPrompt = null;
		}

		private unsafe static String GetErrorMessage(int errorCode)
		{
			int FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x00000100;
			int FORMAT_MESSAGE_IGNORE_INSERTS = 0x00000200;
			int FORMAT_MESSAGE_FROM_SYSTEM  = 0x00001000;
			int messageSize = 255;
			String lpMsgBuf = "";
			int dwFlags = FORMAT_MESSAGE_ALLOCATE_BUFFER | 
				FORMAT_MESSAGE_FROM_SYSTEM | 
				FORMAT_MESSAGE_IGNORE_INSERTS;
			IntPtr ptrlpSource = new IntPtr();
			IntPtr prtArguments = new IntPtr();
			int retVal = FormatMessage(dwFlags, ref ptrlpSource, errorCode, 0, 
				ref lpMsgBuf, messageSize, 
				&prtArguments);
			if(0 == retVal)
			{
				throw new Exception("Failed to format message for error code " + 
					errorCode + ". ");
			}
			return lpMsgBuf;
		}

		public string DeCrypttoString(string decryptedstring)
		{
			return (Encoding.ASCII.GetString(Decrypt(Convert.FromBase64String(decryptedstring),null))) ;
		}
	} // end DPAPI



}
