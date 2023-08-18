using BnsBinTool.Core.DataStructs;

using Xylia.Preview.Data.Models.DatData;

namespace Xylia.Preview.Data.Helper;
public sealed class TableSet_Local : TableSet
{
	private readonly string datpath;
	public TableSet_Local(string DatPath) : base() => this.datpath = DatPath;


	public override void LoadData(bool UseDB, string Folder)
	{
		if (Tables is not null) return;

		var local = new BNSDat(datpath).ExtractBin();

		this.Tables = local.Tables.ToArray();
		detect.Read(this.Tables, null);

		this.LoadConverter();
	}
}

public sealed class TableSet_Test : TableSet
{
	public override void LoadData(bool UseDB, string Folder)
	{
		detect.Read(defs);
		this.LoadConverter();

		foreach (var table in defs)
		{
			var byRef = new Dictionary<Ref, string>();
			var byAlias = new Dictionary<string, Ref>();

			converter._tablesAliases.ByRef[table.Type] = byRef;
			converter._tablesAliases.ByAlias[table.Type] = byAlias;
		}
	}
}