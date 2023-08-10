using System.Xml;

using Xylia.Preview.Data.Models.DatData.DatDetect;

namespace Xylia.Preview.Data.Models.DatData.DataProvider;
public class DefaultProvider : IDataProvider
{
	#region Fields
	public BNSDat XmlData;
	public BNSDat LocalData;
	public BNSDat ConfigData;
	public bool is64Bit = true;

	public static Lazy<IDatSelect> select;

	public Locale Locale { get; private set; }
	#endregion


	#region Methods
	bool IDataProvider.is64Bit() => this.is64Bit;

	IEnumerable<XmlDocument> IDataProvider.GetFiles(string pattern, string type)
	{
		return this.XmlData.EnumerateFiles(pattern).Select(o => o.Xml.Nodes);
	}


	public static DefaultProvider Load(string FolderPath, ResultMode mode = ResultMode.SelectDat)
	{
		if (string.IsNullOrWhiteSpace(FolderPath) || !Directory.Exists(FolderPath))
			throw new Exception("invalid game folder, please to set.");

		//get all
		var datas = new DataCollection(FolderPath);
		var xmls = datas.GetFiles(DatType.xml, mode);
		var locals = datas.GetFiles(DatType.local, mode);
		var configs = datas.GetFiles(DatType.config, mode);

		//get target
		DefaultProvider provider;
		if (xmls.Count > 1 || locals.Count > 1) provider = select.Value.Show(xmls, locals);
		else
		{
			provider = new DefaultProvider()
			{
				XmlData = xmls.FirstOrDefault(),
				LocalData = locals.FirstOrDefault(),
				ConfigData = configs.FirstOrDefault(),
			};
			provider.is64Bit = provider.XmlData.Bit64;
		}

		provider.Locale = new Locale(new DirectoryInfo(FolderPath));
		return provider;
	}
	#endregion
}

public interface IDatSelect
{
	DefaultProvider Show(IEnumerable<FileInfo> Xml, IEnumerable<FileInfo> Local);
}