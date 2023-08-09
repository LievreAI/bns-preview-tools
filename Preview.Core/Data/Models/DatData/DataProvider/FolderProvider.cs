namespace Xylia.Preview.Data.Models.DatData.DataProvider;
public class FolderProvider : IDataProvider
{
	#region Constructor
	private readonly DirectoryInfo directory;

	public FolderProvider(string path) => directory = new(path);
	#endregion


	FileInfo[] IDataProvider.GetFiles(string pattern) => directory.GetFiles(pattern, SearchOption.AllDirectories);
}