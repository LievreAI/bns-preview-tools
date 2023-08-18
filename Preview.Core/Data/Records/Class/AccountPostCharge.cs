using Xylia.Preview.Common.Attribute;

namespace Xylia.Preview.Data.Record;

[AliasRecord]
public sealed class AccountPostCharge : BaseRecord
{
	[Signal("charge-money")]
	public int ChargeMoney;

	[Signal("charge-item"),Repeat(2)]
	public Item[] ChargeItem;

	[Signal("charge-item-amount"), Repeat(2)]
	public int[] ChargeItemAmount;
}