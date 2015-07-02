using System;
using System.Configuration;
using SsisUp.Builders;
using SsisUp.Builders.References;

namespace Ssis.Deploy
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["MasterDb"];
            var fileDestination = ConfigurationManager.AppSettings["FileDestination"];

            var jobConfiguration = JobConfiguration.Create()
                .WithName("Ssis Up - Sample Data Load")
                .WithDescription("This is a sample data load")
                .WithSsisOwner("sa")
                .WithSsisServer(Environment.MachineName)
                .WithSchedule(
                    ScheduleConfiguration.Create()
                        .WithName("Saturday Load")
                        .RunOn(FrequencyDay.Saturday)
                        .StartingAt(new TimeSpan(10, 0, 0))
                        .ActivatedFrom(DateTime.Parse("1 Jan 2010"))
                        .ActivatedUntil(DateTime.Parse("31 Dec 2020")))
                .WithStep(
                    StepConfiguration.Create()
                        .WithId(1)
                        .WithName("Sample Package")
                        .WithSubSystem(SsisSubSystem.IntegrationServices)
                        .WithDtsxFile(@".\Packages\Package.dtsx")
                        .WithDtsxFileDestination(fileDestination)
                        .ExecuteCommand(
                            string.Format(
                                @"/FILE ""{0}\Package.dtsx"" /CHECKPOINTING OFF /REPORTING E /X86",
                                fileDestination))
                        .OnSuccess(JobAction.QuitWithSuccess)
                        .OnFailure(JobAction.QuitWithFailure));


            var result = DeploymentConfiguration
                .Create()
                .ToDatabase(connectionString.ConnectionString)
                .WithJobConfiguration(jobConfiguration)
                .Deploy();

            Console.ReadLine();
            if (!result.Successful)
            {
                return -1;
            }

            return 0;
        }
    }
}
