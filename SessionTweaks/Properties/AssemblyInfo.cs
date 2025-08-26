using System.Reflection;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: ComVisible(false)]
[assembly: AssemblyTitle(SessionTweaks.BuildInfo.Name)]
[assembly: AssemblyDescription(SessionTweaks.BuildInfo.Description)]
[assembly: AssemblyCompany("net.kazu0617")]
[assembly: AssemblyProduct(SessionTweaks.BuildInfo.GUID)]
[assembly: AssemblyVersion(SessionTweaks.BuildInfo.Version)]

namespace SessionTweaks
{
	public static class BuildInfo
	{
		public const string Version = "5.0.0";

		public const string Name = "SessionTweaks";
		public const string Description = "Add buttons for session orb / join without focusing / copy session uri.";

		public const string Author = "kazu0617";

		public const string Link = "https://github.com/kazu0617/SessionTweaks";

		public const string GUID = "net.kazu0617.SessionTweaks";
	}
}
