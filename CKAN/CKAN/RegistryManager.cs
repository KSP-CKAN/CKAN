using System;
using Newtonsoft.Json;

namespace CKAN
{
	public class RegistryManager
	{
		string path;

		public RegistryManager (string path)
		{
			this.path = path;
		}

		public Registry load() {
			string json = System.IO.File.ReadAllText(path);
			return JsonConvert.DeserializeObject<Registry>(json);
		}

		public Registry load_or_create() {
			try {
				return load ();
			}
			catch (System.IO.FileNotFoundException) {
				create ();
				return load ();
			}
		}

		void create() {
			save (Registry.empty ());
		}

		public string serialise (Registry registry) {
			return JsonConvert.SerializeObject (registry);
		}

		public void save (Registry registry) {
			System.IO.File.WriteAllText(path, serialise (registry));
		}
	}
}
