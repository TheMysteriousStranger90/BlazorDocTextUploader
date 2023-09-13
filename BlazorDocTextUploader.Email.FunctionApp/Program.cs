using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;


var host = new HostBuilder().ConfigureFunctionsWorkerDefaults()
    .Build();

host.Run();