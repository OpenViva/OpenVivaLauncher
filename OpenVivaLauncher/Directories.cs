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
	
		public static string GameInstallLocation { get { return $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\OpenVivaLauncher\\Game\\Openviva"; } }
    }
}
