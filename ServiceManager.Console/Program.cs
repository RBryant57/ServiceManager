using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Management;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace ServiceManager.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var impersonator = new Impersonator.Impersonator();
            var userName = ConfigurationManager.AppSettings.Get("UserName");
            var password = ConfigurationManager.AppSettings.Get("Password");
            var domain = ConfigurationManager.AppSettings.Get("Domain");
            var services = ConfigurationManager.AppSettings.Get("Service").Split(',');
            var actions = ConfigurationManager.AppSettings.Get("Action").Split(',');
            var startupTypes = ConfigurationManager.AppSettings.Get("StartupType").Split(',');
            var computerName = ConfigurationManager.AppSettings.Get("ComputerName");

            using (impersonator.Impersonate(domain, userName, password))
            {
                 for(int i = 0; i < services.Length; i++)
                {
                    var service = new ServiceController(services[i]);
                    var startupType = new ServiceStartMode();

                    switch (startupTypes[i].ToLower())
                    {
                        case "automatic":
                            startupType = ServiceStartMode.Automatic;
                            break;
                        case "boot":
                            startupType = ServiceStartMode.Boot;
                            break;
                        case "disabled":
                            startupType = ServiceStartMode.Disabled;
                            break;
                        case "manual":
                            startupType = ServiceStartMode.Manual;
                            break;
                        case "system":
                            startupType = ServiceStartMode.System;
                            break;
                    }

                    //this query could also be: ("select * from Win32_Service where name = '" + serviceName + "'");
                    //ManagementScope scope = new ManagementScope(@"\\" + computerName + @"\root\cimv2");
                    //SelectQuery query = new SelectQuery("select * from Win32_Service where name = '" + services[i] + "'");
                    //using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query))
                    //{
                    //    ManagementObjectCollection collection = searcher.Get();

                    //    foreach (ManagementObject mobj in collection)
                    //    {

                    //    }
                    //}
                    ManagementObject classInstance =
                        new ManagementObject(@"\\" + computerName + @"\root\cimv2",
                        "Win32_Service.Name='" + services[i] + "'",
                        null);

                    // Obtain in-parameters for the method
                    ManagementBaseObject inParams =
                        classInstance.GetMethodParameters("ChangeStartMode");

                    // Add the input parameters.
                    inParams["StartMode"] = startupTypes[i];

                    // Execute the method and obtain the return values.
                    ManagementBaseObject outParams =
                        classInstance.InvokeMethod("ChangeStartMode", inParams, null);

                    if (actions[i].ToLower() == "start")
                        service.Start();
                    else
                        service.Stop();
                }
            }
        }
    }
}
