using Xylia.Preview.Data.Models.DatData.DatDetect;

namespace Xylia.Preview.Data.Models.DatData.DataProvider;
public class FileProvider : IDataProvider
{
	#region Constructor
	private readonly List<FileInfo> files;

	public FileProvider(params FileInfo[] file) => files = file.ToList();
	#endregion


	FileInfo[] IDataProvider.GetFiles(string pattern) => files.ToArray();
}