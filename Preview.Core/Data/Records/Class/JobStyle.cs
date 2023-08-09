using Xylia.Preview.Common.Attribute;
using Xylia.Preview.Common.Seq;

namespace Xylia.Preview.Data.Record;

[AliasRecord]
public sealed class JobStyle : BaseRecord
{
	public JobSeq Job;

	[Signal("job-style")]
	public JobStyleSeq jobStyle;


	public string IntroduceJobStyleIcon;

	public Text IntroduceJobStyleName;

	public Text IntroduceJobStylePlayDesc;

}