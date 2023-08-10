using System.Xml;

namespace Xylia.Preview.Data.Models.DatData.DataProvider;
public interface IDataProvider
{
	bool is64Bit() => true;

	FileInfo[] GetFiles(string pattern) => throw new NotImplementedException();

	IEnumerable<XmlDocument> GetFiles(string pattern , string type) => throw new NotImplementedException();


	DatDetect.Locale Locale
	{ 
		get => throw new NotImplementedException(); 
		private set => throw new NotImplementedException();
	}
}