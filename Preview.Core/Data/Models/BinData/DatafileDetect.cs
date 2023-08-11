using System.Text;

using BnsBinTool.Core.Definitions;
using BnsBinTool.Core.Models;

using Xylia.Extension;
using Xylia.Preview.Data.Models.BinData.AliasTable;
using Xylia.Preview.Data.Models.BinData.Table.Config;

using RecordModel = BnsBinTool.Core.Models.Record;
using TableModel = BnsBinTool.Core.Models.Table;

namespace Xylia.Preview.Data.Models.BinData;

/// <summary>
/// datafile auto detect
/// Actually, it is directly defined in the game program, but we cannot get it.
/// </summary>
public sealed class DatafileDetect
{
	#region Datafile Helper
	readonly Dictionary<int, string> by_id = new();

	readonly Dictionary<string, short> by_name = new(StringComparer.OrdinalIgnoreCase);


	private void AddList(string Name, int Type)
	{
		by_id[Type] = Name;

		if (Name == "account-post-charge")
		{
			AddList("account-level", Type - 1);
		}
		else if (Name == "board-gacha") AddList("board-gacha-reward", Type + 1);
		else if (Name == "challengelistreward") AddList("challengelist", Type - 1);
		else if (Name == "collecting")
		{
			AddList("closet-collecting-grade", Type - 2);
			AddList("closet-group", Type - 1);
		}
		else if (Name == "faction") AddList("faction-level", Type + 1);
		else if (Name == "glyph") AddList("glyph-page", Type + 1);
		else if (Name == "item-graph-seed-group") AddList("item-graph", Type - 1);
		else if (Name == "item-brand") AddList("item-brand-tooltip", Type + 1);
		else if (Name == "item-improve-option")
		{
			AddList("item-improve", Type - 1);
			AddList("item-improve-option-list", Type + 1);
		}
		else if (Name == "job-style")
		{
			AddList("job", Type - 2);
			AddList("jobskillset", Type - 1);
		}
		else if (Name == "jumpingcharacter") AddList("jumpingcharacter2", Type - 1);
		else if (Name == "linkmoveanim") AddList("level", Type - 1);
		else if (Name == "mapinfo")
		{
			AddList("mapoverlay", Type + 1);
			AddList("mapunit", Type + 2);
		}
		else if (Name == "map-group-1") AddList("map-group-1-expedition", Type + 1);
		else if (Name == "mentoring")
		{
			AddList("mastery-level", Type - 3);
			AddList("mastery-stat-point", Type - 2);
			AddList("mastery-stat-point-pick", Type - 1);
		}
		else if (Name == "npc")
		{
			AddList("npccombatmoveanim", Type - 1);

			AddList("npcindicatormoveanim", Type + 1);
			AddList("npcmoveanim", Type + 2);
		}
		else if (Name == "questbonusrewardsetting") AddList("questbonusreward", Type - 1);
		else if (Name == "random-store-item") AddList("random-store-item-display", Type + 1);
		else if (Name == "skillskin")
		{
			AddList("skillshow3", Type - 1);
			AddList("skillskineffect", Type + 1);
		}
		else if (Name == "skilltooltipattribute") AddList("skilltooltip", Type + 1);
		else if (Name == "skill-train-combo-action") AddList("skill-train-category", Type - 1);
		else if (Name == "slatestone")
		{
			AddList("slatescroll", Type - 2);
			AddList("slatescrollstone", Type - 1);
		}
		else if (Name == "unlocated-store") AddList("unlocated-store-ui", Type + 1);
		else if (Name == "vehicle" && by_id.GetValueOrDefault(Type + 1, null) == "vehicle-appearance")
		{
			by_id[Type] = "vehicle-appearance";
			by_id[Type + 1] = "vehicle";
		}
		else if (Name == "vehicle-appearance" && by_id.GetValueOrDefault(Type - 1, null) == "vehicle")
		{
			by_id[Type - 1] = "vehicle-appearance";
			by_id[Type] = "vehicle";
		}
	}

	private void CreateNameMap()
	{
		by_name.Clear();
		foreach (var o in by_id)
		{
			if (o.Value is null)
				continue;

			by_name[o.Value.Replace("-", null)] = (short)o.Key;
		}
	}



	public bool TryGetName(short key, out string name) => by_id.TryGetValue(key, out name);

	public bool TryGetKey(string name, out short key) => by_name.TryGetValue(name.Replace("-", null), out key);
	#endregion


	#region Load
	/// <summary>
	/// create map by detect data
	/// </summary>
	/// <param name="data"></param>
	public void Load(Datafile data) => Load(data.Tables, data.NameTable.CreateTable());

	/// <summary>
	/// create map by detect data
	/// </summary>
	/// <param name="tables"></param>
	/// <param name="AliasTable"></param>
	public void Load(IEnumerable<TableModel> tables, IReadOnlyDictionary<string, AliasCollection> AliasTable)
	{
		tables.ForEach(table => by_id[table.Type] = null);
		Parallel.ForEach(tables, table =>
		{
			if (table.Records.Count == 0) return;

			HashSet<string> GetLookup(RecordModel record) => Encoding.Unicode.GetString(record.StringLookup.Data)
				.Split('\0', StringSplitOptions.RemoveEmptyEntries)
				.ToHashSet(StringComparer.OrdinalIgnoreCase);
			var str1 = GetLookup(table.Records[0]);
			var str2 = table.IsCompressed ? GetLookup(table.Records[^1]) : str1;


			#region common
			if (AliasTable != null)
			{
				bool HasCheck = false;
				foreach (var alist in AliasTable.Where(lst => !lst.Value.HasCheck))
				{
					if (table.Records.Count == 0) continue;
					if (alist.Key != "npctalkmessage" && alist.Key != "effect" &&
						 table.Records.Count != alist.Value.Count &&
						 table.Records.Count != alist.Value.Count + 1) continue;


					if (!str1.Contains(alist.Value.First().Alias)) continue;
					if (!str2.Contains(alist.Value.Last().Alias)) continue;


					HasCheck = alist.Value.HasCheck = true;
					AddList(alist.Key, table.Type);
					break;
				}

				if (HasCheck) return;
			}
			#endregion

			#region else
			if (table.IsCompressed)
			{
				if (table.Size > 5000000)
				{
					var FieldSize = table.Records[0].DataSize;
					if (FieldSize > 2000)
					{
						AddList("item", table.Type);
						return;
					}
					else if (FieldSize == 28 || FieldSize == 36)
					{
						AddList("text", table.Type);
						return;
					}
				}
			}
			else
			{
				if (str1.Contains("00047888.BordGacha_Disable")) AddList("board-gacha", table.Type);
				else if (str1.Contains("ShopSale-1")) AddList("content-quota", table.Type);
				else if (str1.Contains("00008603.Indicator.CN_BlueDiamond"))
				{
					AddList("goodsicon", table.Type - 1);
					AddList("gradebenefits", table.Type);
				}
				else if (str1.Contains("DropItem_Anim")) AddList("itempouchmesh2", table.Type);
				else if (str1.Contains("S,DOWN"))
				{
					AddList("key-cap", table.Type - 1);
					AddList("key-command", table.Type);
				}
				else if (str1.Contains("CharPos_JinM")) AddList("lobby-pc", table.Type);
				else if (str1.Contains("00055945.Thunderer_JinM_Animset"))
				{
					AddList("pc-appearance", table.Type);
					AddList("pc", table.Type + 1);
				}
				else if (str1.Contains("00007975.PC.MaleChild01_BladeMaster"))
				{
					AddList("pc-voice", table.Type - 1);
					AddList("pc-voice-set", table.Type);
				}
				else if (str1.Contains("00009076.Race_Gun")) AddList("race", table.Type);
				else if (str1.Contains("76_PCSpawnPoint_1")) AddList("zonepcspawn", table.Type);
			}
			#endregion
		});

		this.CreateNameMap();
	}

	/// <summary>
	/// create temp map
	/// </summary>
	/// <param name="definitions"></param>
	public void Load(List<TableDefinition> definitions)
	{
		short idx = 0;
		definitions.ForEach(def => AddList(def.Name, ++idx));

		CreateNameMap();
	}
	#endregion



	/// <summary>
	/// convert ref table name to key
	/// </summary>
	public void ConvertTableName(List<TableDefinition> definitions)
	{
		foreach (var tableDef in definitions)
		{
			{
				if (tableDef.Type == 0 && this.TryGetKey(tableDef.Name, out var type))
					tableDef.Type = type;
			}

			foreach (AttributeDef attr in tableDef.ExpandedAttributes.Where(o => o is AttributeDef))
			{
				var TypeName = attr.ReferedTableName;
				if (TypeName != null && this.TryGetKey(TypeName, out var type))
					attr.ReferedTable = type;
			}

			foreach (var subtable in tableDef.Subtables)
			{
				foreach (AttributeDef attr in subtable.ExpandedAttributes.Where(o => o is AttributeDef))
				{
					var TypeName = attr.ReferedTableName;
					if (TypeName != null && this.TryGetKey(TypeName, out var type))
						attr.ReferedTable = type;
				}
			}
		}
	}
}