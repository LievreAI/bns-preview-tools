namespace Xylia.Preview.Common.Struct;
public struct FVersion
{
	public FVersion(ushort[] data)
	{
		this.Major = data[0];
		this.Minor = data[1];
		this.Build = data[2];
		this.Revision = data[3];
	}

	public FVersion(string data)
	{
		var strings = data.Split('.');

		this.Major = ushort.Parse(strings[0]);
		this.Minor = ushort.Parse(strings[1]);
		this.Build = ushort.Parse(strings[2]);
		this.Revision = ushort.Parse(strings[3]);
	}


	public ushort Major { get; }
	public ushort Minor { get; }
	public ushort Build { get; }
	public ushort Revision { get; }


	public override string ToString() => $"{Major}.{Minor}.{Build}.{Revision}";
}