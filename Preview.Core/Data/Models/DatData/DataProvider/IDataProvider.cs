namespace Xylia.Preview.Data.Models.DatData.DataProvider;
public interface IDataProvider
{
	FileInfo[] GetFiles(string pattern);

	bool is64Bit() => true;
}