using Xylia.Preview.Data.Helper;
using Xylia.Preview.Data.Models.DatData.DatDetect;

namespace Xylia.Preview.Common.Struct;
public struct Time64
{
	const long epoch = 621355968000000000;
	public long Ticks;

	public Time64(long Ticks) => this.Ticks = Ticks;


	public DateTime Time => new(epoch + Ticks * 10000000);

	public DateTime LocalTime => TimeZoneInfo.ConvertTimeFromUtc(Time, ZoneInfo());

	public static Time64 Parse(string s) => (TimeZoneInfo.ConvertTimeToUtc(DateTime.Parse(s), ZoneInfo()).Ticks - epoch) / 10000000;


	public static implicit operator Time64(long Ticks) => new(Ticks);

	private static TimeZoneInfo ZoneInfo()
	{
		var publisher = FileCache.Data.Provider?.Locale?.Publisher;
		var offset = publisher switch
		{
			Publisher.Default => new TimeSpan(9, 0, 0),   // Korea Standard Time
			Publisher.Tencent => new TimeSpan(8, 0, 0),   // China Standard Time
			Publisher.Innova => new TimeSpan(0, 0, 0),    //
			Publisher.NcJapan => new TimeSpan(9, 0, 0),   // Tokyo Standard Time
			Publisher.Sea => new TimeSpan(0, 0, 0),        //
			Publisher.NcTaiwan => new TimeSpan(8, 0, 0),  // Taipei Standard Time
			Publisher.NcWest => new TimeSpan(-5, 0, 0),   // Eastern Standard Time
			Publisher.Garena => new TimeSpan(7, 0, 0),    // SE Asia Standard Time
			_ => TimeZoneInfo.Local.BaseUtcOffset,
		};

		return TimeZoneInfo.CreateCustomTimeZone("BnsZoneInfo", offset, publisher.ToString(), publisher.ToString());
	}
}