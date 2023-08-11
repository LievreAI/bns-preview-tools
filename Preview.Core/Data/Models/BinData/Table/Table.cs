using System.Collections;
using System.Configuration;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;

using BnsBinTool.Core;
using BnsBinTool.Core.DataStructs;
using BnsBinTool.Core.Definitions;
using BnsBinTool.Core.Helpers;

using Xylia.Extension;
using Xylia.Preview.Common.Attribute;
using Xylia.Preview.Data.Helper;
using Xylia.Preview.Data.Models.BinData.Table.Record.Attributes;
using Xylia.Preview.Data.Models.DatData.DataProvider;
using Xylia.Preview.Data.Record;
using Xylia.Preview.Properties;
using Xylia.Windows.CustomException;
using Xylia.Xml;

using RecordModel = BnsBinTool.Core.Models.Record;
using TableModel = BnsBinTool.Core.Models.Table;


namespace Xylia.Preview.Data.Models.BinData.Table;
public sealed class Table<T> : IEnumerable<T>, ITable where T : BaseRecord, new()
{
	#region Constructor
	public string Name { get; set; }
	public TableSet Owner { get; set; }
	public TableDefinition TableDef { get; set; }


	public IDataProvider DataProvider;

	public string XmlDataPath;
	#endregion

	#region Data
	private readonly Dictionary<Ref, Lazy<T>> ByRef = new();

	private readonly Dictionary<string, Ref> ByAlias = new(StringComparer.OrdinalIgnoreCase);

	private void AddAlias(string Alias, Ref Ref)
	{
		if (Alias is not null && !this.ByAlias.ContainsKey(Alias))
			this.ByAlias[Alias] = Ref;
	}

	private Lazy<T>[] _records;

	public Lazy<T>[] Records
	{
		private set => this._records = value;
		get
		{
			this.Load();
			return this._records;
		}
	}
	#endregion

	#region Type
	readonly Dictionary<short, Type> Types = new();
	readonly Dictionary<string, short> Types_Name = new(StringComparer.OrdinalIgnoreCase);

	private void GetSubType()
	{
		short typeIndex = -1;
		foreach (var instance in Assembly.GetExecutingAssembly().GetTypes())
		{
			if (!instance.IsAbstract && typeof(T).IsAssignableFrom(instance) && instance != typeof(T))
			{
				typeIndex++;

				Types[typeIndex] = instance;
				Types_Name[instance.Name.TitleLowerCase()] = typeIndex;
			}
		}
	}

	private T CreateInstance(string Type, out short? type) => CreateInstance(type = string.IsNullOrWhiteSpace(Type) ? null : Types_Name.GetValueOrDefault(Type));

	private T CreateInstance(short? type)
	{
		if (type.HasValue && Types.TryGetValue(type.Value, out var instance))
			return Activator.CreateInstance(instance) as T;
		return Activator.CreateInstance(typeof(T)) as T;
	}
	#endregion



	#region Load Methods
	private bool InLoading = false;

	public void Load()
	{
		#region Initialize
		if (this.InLoading)
		{
			while (this.InLoading) Thread.Sleep(100);
			return;
		}

		if (this._records != null) return;
		if (Program.IsDesignMode) return;


		this.InLoading = true;
		this._records = null;
		#endregion


		ArgumentNullException.ThrowIfNull(TableDef);
		this.GetSubType();

		lock (this)
		{
			var task = new Task(() =>
			{
				try
				{
					if (DataProvider is null && Owner.Provider is DefaultProvider or null) this.LoadData();
					else this.LoadXml();

					Trace.WriteLine($"[{DateTime.Now}] load table `{Name}` successful ({this._records.Length})");
				}
				catch (Exception ex)
				{
					if (ex is ConfigurationErrorsException or UserExitException)
						throw;

					this._records = Array.Empty<Lazy<T>>();
					Trace.WriteLine($"[{DateTime.Now}] load table `{Name}` failed: {ex}");
				}
				finally
				{
					InLoading = false;
				}
			});

			task.Start();
			task.Wait();
		}
	}

	private void LoadXml()
	{
		//get table
		var Files = (DataProvider ?? this.Owner.Provider).GetFiles(XmlDataPath ?? $"{Name}Data*");
		if (Files is null || !Files.Any()) throw new FileNotFoundException();

		LoadXml(Files.Select(o => o.FullName.GetXmlDocument()));
	}

	private void LoadXml(IEnumerable<XmlDocument> xmls)
	{
		var keys = TableDef.ExpandedAttributes.Where(attr => attr.IsKey);
		var builder = new RecordBuilder(null, null);

		var aliasAttrDef = TableDef["alias"];



		List<XmlElement> tables = new();
		foreach (var xml in xmls)
		{
			var root = xml.DocumentElement;
			tables.AddRange(root.SelectNodes($"./" + TableDef.OriginalName).OfType<XmlElement>());
		}

		this._records = new Lazy<T>[tables.Count];
		for (var x = 0; x < this._records.Length; x++)
		{
			var element = tables[x];

			// build ref
			var record = new RecordModel { Data = new byte[16] };
			if (TableDef.AutoKey) record.RecordId = x + 1;
			else keys.ForEach(attr => builder.SetAttribute(record, attr, element.Attributes[attr.Name]?.Value));


			// add
			var Ref = record.RecordRef;
			if (aliasAttrDef != null) AddAlias(element.Attributes["alias"]?.Value, Ref);

			this.ByRef[Ref] = this._records[x] = new(() =>
			{
				var Object = CreateInstance(element.Attributes["type"]?.Value, out var type);
				Object.Ref = Ref;
				Object.Type = type ?? -1;
				Object.LoadData(element);

				return Object;
			});
		}
	}

	private void LoadData()
	{
		lock (Owner) Owner.LoadData(XmlDataPath is null);

		if (XmlDataPath is not null)
		{
			LoadXml(Owner.Provider.GetFiles(XmlDataPath, "Xml"));
			return;
		}

		//get table
		if (this.TableDef.Type == 0) throw new FileNotFoundException();
		var table = Owner.Tables.FirstOrDefault(table => table.Type == this.TableDef.Type);
		TableDefinitionEx.CheckVersion(TableDef, table);
		TableDefinitionEx.CheckSize(TableDef, table, (msg) => Debug.WriteLine(msg));

		if (Settings.TestMode == DumpMode.Used && this.Owner?.GetType() == typeof(TableSet))
			ProcessTable(CommonPath.DataFiles);


		var aliasAttrDef = TableDef["alias"];

		this._records = new Lazy<T>[table.Records.Count];
		for (var x = 0; x < this._records.Length; x++)
		{
			var record = table.Records[x];
			if (aliasAttrDef != null) AddAlias(record.StringLookup.GetString(record.Get<int>(aliasAttrDef.Offset)), record.RecordRef);

			this.ByRef[record.RecordRef] = this._records[x] = new(() =>
			{
				var Object = CreateInstance(record.SubclassType);
				Object.Ref = record.RecordRef;
				Object.Type = record.SubclassType;
				Object.LoadData(new DbData(Owner.converter, this.TableDef, record));

				return Object;
			});
		}
	}
	#endregion

	#region Get Methods
	public T this[string Alias] => this.GetLazyInfo(Alias)?.Value;

	public T this[int Id, int Variant = 0] => this.GetLazyInfo(new Ref(Id, Variant))?.Value;

	public T this[BaseRecord resolvedRecord] => this[resolvedRecord?.alias];


	private Lazy<T> GetLazyInfo(string Alias)
	{
		this.Load();

		if (string.IsNullOrWhiteSpace(Alias)) return null;
		else if (this.ByAlias.TryGetValue(Alias, out var item)) return GetLazyInfo(item);
		else if (int.TryParse(Alias, out var MainID)) return GetLazyInfo(new Ref(MainID));
		else if (Alias.Contains('.'))
		{
			var o = Alias.Split('.');
			if (o.Length == 2 && int.TryParse(o[0], out var id) && int.TryParse(o[1], out var variant))
				return GetLazyInfo(new Ref(id, variant));
		}

		if (this._records != null && this._records.Length != 0
			&& typeof(T) != typeof(Text))
			Debug.WriteLine($"[{Name}] get failed, alias: {Alias}");
		return null;
	}

	private Lazy<T> GetLazyInfo(Ref Ref)
	{
		this.Load();

		if (Ref.Id <= 0) return null;
		else if (this.ByRef.TryGetValue(Ref, out var item)) return item;
		else if (this._records != null && this._records.Length != 0)
			Debug.WriteLine($"[{Name}] get failed, id: {Ref.Id} variation: {Ref.Variant}");

		return null;
	}
	#endregion


	#region Process Methods
	public void ProcessTable(string outputPath)
	{
		var table = Owner.Tables.FirstOrDefault(table => table.Type == this.TableDef.Type);
		if (table is null) return;

		Owner.converter.ProcessTable(table, this.TableDef, outputPath);
	}
	#endregion

	#region Serialize Methods
	public TableModel Serialize()
	{
		var table = new TableModel()
		{
			ElementCount = 1,
			Type = 14,
			MajorVersion = 0,
			MinorVersion = 1,
			IsCompressed = false,

			RecordCountOffset = 1,
			Padding  = null,
		};


		var builder = Owner.converter.Builder;
		builder.InitializeTable(table.IsCompressed);

		// Clear old records
		table.Records.Clear();

		foreach (var record in this.Records)
		{
			table.Records.Add(record.Value.Serialize(this));
		}

		builder.FinalizeTable();
		return table;
	}

	public XDocument Serialize(ReleaseSide side = default)
	{
		var doc = new XDocument();

		var table = new XElement("table");
		doc.Add(table);

		table.SetAttributeValue("release-module", "");
		table.SetAttributeValue("release-side", side.ToString().ToLower());
		table.SetAttributeValue("type", null);
		table.SetAttributeValue("version", $"");

		foreach (var record in this.Records)
		{
			table.Add(record.Value.Serialize(side));
		}


		return doc;
	}
	#endregion


	#region Interface
	object ITable.this[string Alias] => this[Alias];

	public void Clear()
	{
		this._records = null;

		this.ByRef.Clear();
		this.ByAlias.Clear();
	}

	public IEnumerator<T> GetEnumerator()
	{
		if (this.Records != null)
			foreach (var info in this.Records)
				yield return info.Value;

		yield break;
	}

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	#endregion
}

public interface ITable : IEnumerable
{
	/// <summary>
	/// table name
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	/// bns data provider
	/// </summary>
	public TableSet Owner { get; set; }

	/// <summary>
	/// data struct definition
	/// </summary>
	public TableDefinition TableDef { get; set; }



	object this[string Alias] { get; }

	void Clear();
}