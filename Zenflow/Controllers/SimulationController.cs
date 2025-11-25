using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using static FintechStatsPlatform.Program;

namespace Zenflow.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SimulationController : ControllerBase
    {
        [HttpGet("run-complex-process")]
        public async Task<IActionResult> RunComplexProcess()
        {
            var currentActivity = Activity.Current;

            if (currentActivity != null)
            {
                currentActivity.AddTag("labwork.student", "Stashenko, Androshuk, Ruban, Sinchuk");
                currentActivity.AddTag("labwork.requirement", "3a_Additional_Fields");
                currentActivity.AddTag("transaction.priority", "High");
            }

            using (var activity = AppDiagnostics.ActivitySource.StartActivity("Manual_Heavy_Calculation"))
            {
                activity?.AddTag("calculation.method", "MonteCarlo");

                await Task.Delay(2000);

                activity?.AddEvent(new ActivityEvent("Calculation reached 50%"));

                await Task.Delay(500);
            }

            return Ok(new { status = "Success", message = "Trace generated with custom fields and child span." });
        }
    }
}