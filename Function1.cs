using System;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace FuncionEncolar040123
{
    public class Function1
    {
        private readonly IServicio _servicio;
        public Function1(IServicio servicio)
        {
            _servicio = servicio;
        }

        [FunctionName("Function1")]
        public async Task Run([TimerTrigger("0 */1 * * * *", RunOnStartup = true)] TimerInfo myTimer, ILogger log, CancellationToken stoppingToken)
        {
            log.LogInformation($"C# Timer trigger executed at: a {DateTime.Now}");
            await _servicio.GetNewData(stoppingToken);
        }
    }
}
