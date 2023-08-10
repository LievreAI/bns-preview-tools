using Xylia.Preview.Common.Attribute;

namespace Xylia.Preview.Data.Record;
[AliasRecord]
public sealed class ItemEvent : BaseRecord
{
	public DateTime EventExpirationTime;

	public Text Name2;


	#region Functions
	public bool IsExpiration => this.EventExpirationTime < DateTime.Now;
	#endregion
}