using Xylia.Preview.Common.Attribute;
using Xylia.Preview.Common.Interface;

namespace Xylia.Preview.Data.Record;
[AliasRecord]
public sealed class Dungeon : BaseRecord, IAttraction
{	 
	public sbyte UiTextGrade;
	public Text DungeonName2;
	public Text DungeonDesc;


	#region Interface
	public string GetName() => this.DungeonName2.GetText();

	public string GetDescribe() => this.DungeonDesc.GetText();
	#endregion
}