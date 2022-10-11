using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenVivaLauncher
{
	public class Directories
	{		
		public static string Appdata { get{ return $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}"; } }

        public static string LocalAppdata { get { return $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}"; } }

        public static string DefaultInstallLocation { get { return Appdata + "\\OpenVivaLauncher\\Game\\"; } }

		public static string ConfigLocation { get { return LocalAppdata + "\\OpenVivaLauncher\\Config.json"; } }
    }
}
