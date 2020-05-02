using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace App.Web
{
    public interface IServerService
    {
        string MapPath(string path);
    }

    /// <summary>
    /// services.AddSingleton<IServerProvider, ServerProvider>();
    /// </summary>
    public class ServerService : IServerService
    {
        private IHostingEnvironment _host;
        public ServerService(IHostingEnvironment host)
        {
            _host = host;
        }

        public string MapPath(string path)
        {
            return Path.Combine(_host.ContentRootPath, path);
        }
    }
}
