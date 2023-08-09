﻿using System.Security.Cryptography;
using System.Text;

using Ionic.Zlib;

using Xylia.Extension;
using Xylia.Preview.Data.Models.DatData.DatDetect;

using static Xylia.Preview.Data.Models.DatData.BXML_CONTENT;
using static Xylia.Preview.Data.Models.DatData.Third.MySpport;

namespace Xylia.Preview.Data.Models.DatData;
public sealed class BNSDat
{
	#region Fields
	public string Path;

	public bool Bit64;

	public KeyInfo KeyInfo = new();
	#endregion

	#region Constructor
	public BNSDat(string DatPath, bool? Is64 = null, byte[] AES = null)
	{
		if (AES != null) KeyInfo.AES_KEY.Add(AES);

		Bit64 = Is64 ?? DatPath.Judge64Bit();
		Path = DatPath;

	}


	public static implicit operator BNSDat(FileInfo file) => file != null ? new(file.FullName) : null;

	public static implicit operator BNSDat(string path) => File.Exists(path) ? new(path) : null;
	#endregion


	#region DatInfo
	/// <summary>
	/// 2020年5月引入的认证字符
	/// </summary>
	public byte[] Auth;

	public byte[] Signature;
	public static byte[] Magic => "UOSEDALB"u8.ToArray();


	public uint Version;

	public byte[] Unknown_001;
	public byte[] Unknown_002;

	public int FileDataSizePacked;

	public bool IsCompressed;
	public bool IsEncrypted;

	public int FileTableSizePacked;
	public int FileTableSizeUnpacked;
	public byte[] FileTableUnpacked;

	public int OffsetGlobal;


	private List<BPKG_FTE> _files;

	public List<BPKG_FTE> FileTable
	{
		get
		{
			if (_files is null)
				this.Read();

			return _files;

		}
	}

	public IEnumerable<BPKG_FTE> EnumerateFiles(string searchPattern)
	{
		searchPattern = searchPattern
			.Replace("/", "\\")
			.Replace("\\", "\\\\")
			.Replace("*", ".*?");

		return FileTable.Where(o => o.FilePath.RegexMatch(searchPattern));
	}
	#endregion

	#region Functions
	public static byte[] Decrypt(byte[] buffer, int size, byte[] AES)
	{
		// AES requires buffer to consist of blocks with 16 bytes (each)
		// expand last block by padding zeros if required...
		// -> the encrypted data in BnS seems already to be aligned to blocks

		int AES_BLOCK_SIZE = AES.Length;
		int sizePadded = size + AES_BLOCK_SIZE;

		byte[] output = new byte[sizePadded];
		byte[] tmp = new byte[sizePadded];
		buffer.CopyTo(tmp, 0);
		buffer = null;


		var aes = Aes.Create();
		aes.Mode = CipherMode.ECB;

		ICryptoTransform decrypt = aes.CreateDecryptor(AES, new byte[16]);
		decrypt.TransformBlock(tmp, 0, sizePadded, output, 0);

		tmp = output;
		output = new byte[size];
		Array.Copy(tmp, 0, output, 0, size);
		tmp = null;

		return output;
	}

	public static byte[] Encrypt(byte[] buffer, int size, out int sizePadded, byte[] AESKey)
	{
		int AES_BLOCK_SIZE = AESKey.Length;
		sizePadded = size + (AES_BLOCK_SIZE - size % AES_BLOCK_SIZE);

		byte[] output = new byte[sizePadded];
		byte[] temp = new byte[sizePadded];


		Array.Copy(buffer, 0, temp, 0, buffer.Length);
		buffer = null;

		var aes = Aes.Create();
		aes.Mode = CipherMode.ECB;

		ICryptoTransform encrypt = aes.CreateEncryptor(AESKey, new byte[16]);
		encrypt.TransformBlock(temp, 0, sizePadded, output, 0);

		temp = null;
		return output;
	}

	public static byte[] Deflate(byte[] buffer, int sizeDecompressed)
	{
		byte[] tmp = ZlibStream.UncompressBuffer(buffer);
		if (tmp.Length != sizeDecompressed)
		{
			byte[] tmp2 = new byte[sizeDecompressed];
			Array.Copy(tmp, 0, tmp2, 0, Math.Min(sizeDecompressed, tmp.Length));

			tmp = tmp2;
			tmp2 = null;
		}

		return tmp;
	}

	public static byte[] Inflate(byte[] buffer, int sizeDecompressed, byte? compressionLevel = null)
	{
		if (sizeDecompressed == 0)
			sizeDecompressed = buffer.Length;

		var output = new MemoryStream();

		ZlibStream zs = new(output, CompressionMode.Compress, (CompressionLevel)(compressionLevel ?? 6), true);
		zs.Write(buffer, 0, sizeDecompressed);
		zs.Flush();
		zs.Close();

		return output.ToArray();
	}




	public static byte[] Unpack(byte[] buffer, int sizeStored, int sizeSheared, int sizeUnpacked, bool isEncrypted, bool isCompressed, KeyInfo KeyInfo)
	{
		byte[] output = buffer;

		if (KeyInfo.Correct != null) Callback(buffer, sizeStored, sizeSheared, sizeUnpacked, isEncrypted, isCompressed, KeyInfo.Correct, out output);
		else
		{
			foreach (byte[] AES in KeyInfo.AES_KEY)
			{
				if (Callback(buffer, sizeStored, sizeSheared, sizeUnpacked, isEncrypted, isCompressed, AES, out output))
				{
					KeyInfo.Correct = AES;
					break;
				}
			}
		}


		return output;
	}

	public static bool Callback(byte[] buffer, int sizeStored, int sizeSheared, int sizeUnpacked, bool isEncrypted, bool isCompressed, byte[] key, out byte[] output)
	{
		output = buffer;

		try
		{
			if (isEncrypted) output = Decrypt(buffer, sizeStored, key);
			if (isCompressed) output = Deflate(output, sizeUnpacked);

			//既不加密，也不压缩 -> 返回原始副本	
			if (!isEncrypted && !isCompressed)
			{
				output = new byte[sizeUnpacked];
				Array.Copy(buffer, 0, output, 0, Math.Min(sizeSheared, sizeUnpacked));
			}

			return true;
		}
		catch
		{
			return false;
		}
	}

	public static byte[] Pack(byte[] buffer, int sizeUnpacked, out int sizeSheared, out int sizeStored, bool encrypt, bool compress, byte compressionLevel, byte[] Key)
	{
		byte[] output = buffer;
		buffer = null;

		sizeStored = sizeSheared = sizeUnpacked;

		//如果是压缩过的
		if (compress)
		{
			output = Inflate(output, sizeUnpacked, compressionLevel);

			sizeSheared = output.Length;
			sizeStored = output.Length;
		}

		if (encrypt)
		{
			output = Encrypt(output, output.Length, out sizeStored, Key ?? new KeyInfo().AES_KEY.First());
		}

		return output;
	}
	#endregion


	#region Functions
	private void Read()
	{
		if (!File.Exists(Path))
			return;

		#region head info
		using var br = new BinaryReader(new FileStream(Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
		Signature = br.ReadBytes(8);
		Version = br.ReadUInt32();
		Unknown_001 = br.ReadBytes(5);
		FileDataSizePacked = Bit64 ? (int)br.ReadInt64() : br.ReadInt32();
		var FileCount = Bit64 ? (int)br.ReadInt64() : br.ReadInt32();
		IsCompressed = br.ReadBoolean();
		IsEncrypted = br.ReadBoolean();

		// Update 200429
		if (Version == 3)
		{
			Auth = br.ReadBytes(128);
		}


		Unknown_002 = br.ReadBytes(62);

		FileTableSizePacked = Bit64 ? (int)br.ReadInt64() : br.ReadInt32();
		FileTableSizeUnpacked = Bit64 ? (int)br.ReadInt64() : br.ReadInt32();

		byte[] FileTablePacked = br.ReadBytes(FileTableSizePacked);

		//不要相信数值，请读取当前流位置
		OffsetGlobal = Bit64 ? (int)br.ReadInt64() : br.ReadInt32();
		OffsetGlobal = (int)br.BaseStream.Position;
		#endregion

		#region files
		byte[] FileTableUnpacked = Unpack(FileTablePacked, FileTableSizePacked, FileTableSizePacked, FileTableSizeUnpacked, IsEncrypted, IsCompressed, KeyInfo);
		FileTablePacked = null;

		using BinaryReader br2 = new(new MemoryStream(FileTableUnpacked));
		FileTableUnpacked = null;

		//file info
		_files = new List<BPKG_FTE>();
		for (int i = 0; i < FileCount; i++)
			_files.Add(new BPKG_FTE(br2, Bit64, OffsetGlobal) { KeyInfo = KeyInfo });

		//file data
		Parallel.ForEach(_files, BPKG_FTE =>
		{
			lock (br)
			{
				br.BaseStream.Position = BPKG_FTE.FileDataOffset;
				BPKG_FTE.Data = br.ReadBytes(BPKG_FTE.FileDataSizeStored);
			}
		});
		#endregion
	}

	public static void Pack(PackParam param)
	{
		ArgumentNullException.ThrowIfNull(param.Aes);
		var Level = (byte)(3 * (byte)param.CompressionLevel);


		#region 获取文件数据
		List<BPKG_FTE> list = new();
		foreach (string File in Directory.EnumerateFiles(param.FolderPath, "*", SearchOption.AllDirectories))
		{
			list.Add(new BPKG_FTE()
			{
				FilePath = File,
				Unknown_001 = 2,
				IsCompressed = true,
				IsEncrypted = true,
				Unknown_002 = 0,
				FileDataSizeUnpacked = 0,
				Padding = new byte[60],
			});
		}

		Parallel.ForEach(list, _file =>
		{
			using FileStream fis = new(_file.FilePath, FileMode.Open);

			var tmp = new MemoryStream();
			if (_file.FilePath.EndsWith(".xml") || _file.FilePath.EndsWith(".x16")) tmp = Convert(fis, BXML_TYPE.BXML_BINARY);
			else fis.CopyTo(tmp);

			// 原始数据
			var originalData = tmp.ToArray();
			_file.FileDataSizeUnpacked = originalData.Length;
			tmp.Close();
			tmp = null;

			// 转换数据
			_file.FilePath = _file.FilePath.Replace(param.FolderPath, "").TrimStart('\\');
			_file.Data = Pack(originalData, _file.FileDataSizeUnpacked, out _file.FileDataSizeSheared, out _file.FileDataSizeStored, _file.IsEncrypted, _file.IsCompressed, Level, param.Aes);

			originalData = null;
		});
		#endregion


		#region 生成头部数据
		var is64 = param.Bit64;

		BinaryWriter bw = new(new MemoryStream());
		bw.Write(Magic);

		int Version = 3;
		bw.Write(Version);

		byte[] Unknown_001 = new byte[5] { 0, 0, 0, 0, 0 };
		bw.Write(Unknown_001);

		// size
		int FileDataSizePacked = 0;
		if (is64) bw.Write((long)FileDataSizePacked);
		else bw.Write(FileDataSizePacked);

		// count
		if (is64) bw.Write((long)list.Count);
		else bw.Write(list.Count);


		bool IsCompressed = true;
		bw.Write(IsCompressed);
		bool IsEncrypted = true;
		bw.Write(IsEncrypted);


		if (Version == 3)
		{
			var Auth = RSA3;
			bw.Write(new byte[128]);
		}

		byte[] Unknown_002 = new byte[62];
		bw.Write(Unknown_002);
		#endregion

		#region 生成BKG数据
		int FileDataOffset = 0;
		BinaryWriter mosTable = new(new MemoryStream());
		foreach (BPKG_FTE item in list)
		{
			item.FileDataOffset = FileDataOffset;
			byte[] FilePath = Encoding.Unicode.GetBytes(item.FilePath);


			if (is64) mosTable.Write((long)item.FilePath.Length);
			else mosTable.Write(item.FilePath.Length);

			mosTable.Write(FilePath);
			mosTable.Write(item.Unknown_001);
			mosTable.Write(item.IsCompressed);
			mosTable.Write(item.IsEncrypted);
			mosTable.Write(item.Unknown_002);

			if (is64) mosTable.Write((long)item.FileDataSizeUnpacked);
			else mosTable.Write(item.FileDataSizeUnpacked);

			if (is64) mosTable.Write((long)item.FileDataSizeSheared);
			else mosTable.Write(item.FileDataSizeSheared);

			if (is64) mosTable.Write((long)item.FileDataSizeStored);
			else mosTable.Write(item.FileDataSizeStored);

			if (is64) mosTable.Write((long)item.FileDataOffset);
			else mosTable.Write(item.FileDataOffset);

			FileDataOffset += item.FileDataSizeStored;


			mosTable.Write(item.Padding);
		}

		int FileTableSizeUnpacked = (int)mosTable.BaseStream.Length;
		int FileTableSizeSheared = FileTableSizeUnpacked;
		int FileTableSizePacked = FileTableSizeUnpacked;

		var buffer_unpacked = mosTable.BaseStream.ToBytes();
		mosTable.Dispose();
		mosTable = null;

		var buffer_packed = Pack(buffer_unpacked, FileTableSizeUnpacked, out FileTableSizeSheared, out FileTableSizePacked, IsEncrypted, IsCompressed, Level, param.Aes);
		buffer_unpacked = null;


		if (is64) bw.Write((long)FileTableSizePacked);
		else bw.Write(FileTableSizePacked);

		if (is64) bw.Write((long)FileTableSizeUnpacked);
		else bw.Write(FileTableSizeUnpacked);

		long Pos = bw.BaseStream.Position;

		bw.Write(buffer_packed);
		buffer_packed = null;
		#endregion

		#region 生成内容数据
		int OffsetGlobal = (int)bw.BaseStream.Position + (is64 ? 8 : 4);
		if (is64) bw.Write((long)OffsetGlobal);
		else bw.Write(OffsetGlobal);

		foreach (BPKG_FTE item in list)
		{
			bw.Write(item.Data);
			item.Data = null;
		}

		FileDataSizePacked = (int)(bw.BaseStream.Length - Pos);
		bw.BaseStream.Position = 17;
		bw.Write((long)FileDataSizePacked);
		#endregion


		#region 最后处理
		FileStream fileStream = null;

		try
		{
			fileStream = new(param.PackagePath, FileMode.Open);
		}
		catch
		{
			fileStream = new(param.PackagePath + ".tmp", FileMode.Open);
			Console.WriteLine("由于文件目前正在占用，已变更为创建临时文件。待退出游戏后将.tmp后缀删除即可。\n");
		}

		bw.BaseStream.Position = 0;
		bw.BaseStream.CopyTo(fileStream);
		bw.Dispose();
		bw = null;

		fileStream.Close();
		fileStream = null;

		Console.WriteLine("完成");
		#endregion
	}
	#endregion
}