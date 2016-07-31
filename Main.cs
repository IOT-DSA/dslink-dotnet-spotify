using System.Threading;
using DSLink.NET;

namespace DSLink.Spotify
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			NETPlatform.Initialize();
			var dslink = new SpotifyDSLink(new Configuration(args, "Spotify", responder: true));
			dslink.Connect();

			while (true)
			{
				Thread.Sleep(60 * 1000);
			}
		}
	}
}
