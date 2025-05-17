using System;
using System.Threading.Tasks;
using Piranha.Workflow;

namespace WorkflowDemo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Piranha CMS Workflow Demo");
            Console.WriteLine("-------------------------");
            Console.WriteLine();

            await TestWorkflow.RunWorkflowDemoAsync();

            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
} 