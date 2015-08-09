using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace CommonTypes
{
	public class SettingsManager
	{
		public static string GetSettingsPath(string brandName, string productName)
		{
			return Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
				brandName,
				productName);
		}

		public static string GetSettingsFilePath(string brandName, string productName)
		{
			string result = GetSettingsPath(brandName, productName);
			string filename = string.Format("{0}-settings.dat", productName);

			result = Path.Combine(result, filename);

			return result;
		}

		public static void AssureFolderExists(string brandName, string productName)
		{
			string folder = GetSettingsPath(brandName, productName);
			if (!Directory.Exists(folder))
				Directory.CreateDirectory(folder);
		}

		public static bool StoreSettings(string brandName, string productName, object settingsObject)
		{
			bool result = false;

			if (!string.IsNullOrWhiteSpace(brandName)
				&& !string.IsNullOrWhiteSpace(productName)
				&& settingsObject != null)
			{
				BinaryFormatter formatter = new BinaryFormatter();

				AssureFolderExists(brandName, productName);

				string path = GetSettingsFilePath(brandName, productName);

				using (FileStream writeStream = new FileStream(path, FileMode.Create, FileAccess.Write))
					formatter.Serialize(writeStream, settingsObject);

				result = true;
			}
			return result;
		}

		public static object LoadSettings(string brandName, string productName)
		{
			object result = null;

			BinaryFormatter formatter = new BinaryFormatter();

			string path = GetSettingsFilePath(brandName, productName);

			try
			{
				AssureFolderExists(brandName, productName);
				using (FileStream readStream = new FileStream(path, FileMode.Open, FileAccess.Read))
					result = formatter.Deserialize(readStream);
			}
			catch (FileNotFoundException) { }
			catch (SerializationException) { }

			return result;
		}
	}
}
