﻿using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace Xylia.Preview.Data.Models.DatData.Third;
public static class BnsCompression
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void Emit(byte[] rsaBlob, ref int offset, byte[] value)
	{
		Buffer.BlockCopy(value, 0, rsaBlob, offset, value.Length);
		offset += value.Length;
	}

	public unsafe static byte[] GetRSAKeyBlob(byte[] exp, byte[] mod, byte[] p, byte[] q)
	{
		if (exp == null || mod == null || p == null || q == null)
		{
			throw new CryptographicException();
		}

		byte[] array2;
		byte[] array = array2 = new byte[Marshal.SizeOf<BCRYPT_RSAKEY_BLOB>() + exp.Length + mod.Length + p.Length + q.Length];
		fixed (byte* ptr = array2) {
			BCRYPT_RSAKEY_BLOB* ptr2 = (BCRYPT_RSAKEY_BLOB*)ptr;
			ptr2->Magic = KeyBlobMagicNumber.BCRYPT_RSAPRIVATE_MAGIC;
			ptr2->BitLength = mod.Length * 8;
			ptr2->cbPublicExp = exp.Length;
			ptr2->cbModulus = mod.Length;
			ptr2->cbPrime1 = p.Length;
			ptr2->cbPrime2 = q.Length;
			int num = Marshal.SizeOf<BCRYPT_RSAKEY_BLOB>();
			Emit(array, ref num, exp);
			Emit(array, ref num, mod);
			Emit(array, ref num, p);
			Emit(array, ref num, q);
		}
		array2 = null;
		return array;
	}

	[DllImport("bnscompression.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
	public static extern bool GetCreateParams([MarshalAs(UnmanagedType.LPWStr)] string fileName, [MarshalAs(UnmanagedType.U1)] out bool use64Bit, out CompressionLevel compressionLevel, out IntPtr encryptionKey, out uint encryptionKeySize, out IntPtr privateKeyBlob, out uint privateKeyBlobSize, out BinaryXmlVersion binaryXmlVersion);

	[DllImport("bnscompression.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
	public static extern double ExtractToDirectory([MarshalAs(UnmanagedType.LPWStr)] string sourceFileName, [MarshalAs(UnmanagedType.LPWStr)] string destinationDirectoryName, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U1, SizeParamIndex = 3)] byte[] encryptionKey, uint encryptionKeySize, Delegate d);

	[DllImport("bnscompression.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
	public static extern double CreateFromDirectory([MarshalAs(UnmanagedType.LPWStr)] string sourceDirectoryName, [MarshalAs(UnmanagedType.LPWStr)] string destinationFileName, [MarshalAs(UnmanagedType.U1)] bool use64Bit, CompressionLevel compressionLevel, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U1, SizeParamIndex = 5)] byte[] encryptionKey, uint encryptionKeySize, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U1, SizeParamIndex = 7)] byte[] privateKeyBlob, uint privateKeyBlobSize, BinaryXmlVersion binaryXmlVersion, Delegate d);

	public enum CompressionLevel
	{
		None = -1,
		Fastest,
		Fast,
		Normal,
		Maximum
	}

	public enum BinaryXmlVersion
	{
		None = -1,
		Version3,
		Version4
	}

	public enum DelegateResult
	{
		Continue,
		Skip,
		Cancel
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
	public delegate DelegateResult Delegate([MarshalAs(UnmanagedType.LPWStr)] string fileName, ulong fileSize);
}
