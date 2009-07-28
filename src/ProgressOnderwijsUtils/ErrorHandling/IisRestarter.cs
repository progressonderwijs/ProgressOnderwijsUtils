using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceProcess;
using System.IO;

namespace ProgressOnderwijsUtils.ErrorHandling
{
	public static class IisRestarter
	{
		public static void RestartNow() { //require administrative rights; Iis cannot do this itself.
			ServiceController w3svc = ServiceController.GetServices().First(service => service.ServiceName.ToUpperInvariant() == "W3SVC");
			Console.Write("Stopping");
			w3svc.Stop();
			Console.WriteLine("...");
			w3svc.WaitForStatus(ServiceControllerStatus.Stopped);
			Console.Write("Restarting");
			w3svc.Start();
			Console.WriteLine("...");
			w3svc.WaitForStatus(ServiceControllerStatus.Running);
			Console.WriteLine("Restarted.");
		}
	}
}
