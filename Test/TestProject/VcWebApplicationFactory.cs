using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vc.DAL.Mongo;
using VcGrpcService;
using Microsoft.Extensions.DependencyInjection;
using TestProject.Fixtures;
using Microsoft.Extensions.Options;

namespace TestProject
{
    public class VcWebApplicationFactory : WebApplicationFactory<Startup>
    {
        public VcWebApplicationFactory()
        {

        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
