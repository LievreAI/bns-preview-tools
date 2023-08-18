using Xylia.Configure;

namespace Xylia.Preview.Properties;
public static class CommonPath
{
	public static string OutputFolder
	{
		get => Ini.Instance.ReadValue("Folder", "Output");
		set
		{
			if (Directory.Exists(value))
				Ini.Instance.WriteValue("Folder", "Output", value);
		}
	}

	public static string GameFolder
	{
		get => Ini.Instance.ReadValue("Folder", "Game_Bns");
		set
		{
			if (Directory.Exists(value))
				Ini.Instance.WriteValue("Folder", "Game_Bns", value);
		}
	}



	public static string DataFiles => Path.Combine(OutputFolder, "data");

	public static string OutputFolder_Resource
	{
		get => Ini.Instance.ReadValue("Folder", "Output_Resource") ?? (OutputFolder + @"\Pak");
		set => Ini.Instance.WriteValue("Folder", "Output_Resource", value);
	}
}