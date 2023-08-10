using System.Xml;

using Xylia.Xml;

namespace Xylia.Preview.Data.Models.DatData.DataProvider;
public class FolderProvider : IDataProvider
{
	#region Constructor
	private readonly DirectoryInfo directory;

	public FolderProvider(string path) => directory = new(path);
	#endregion


	public FileInfo[] GetFiles(string pattern) => directory.GetFiles(pattern, SearchOption.AllDirectories);

	public IEnumerable<XmlDocument> GetFiles(string pattern, string type) => 
		GetFiles(pattern).Select(o => o.FullName.GetXmlDocument());
}