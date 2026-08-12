using System.Windows;
using FreelanceFlow.Data;

namespace FreelanceFlow
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            using var context = new FreelanceFlowDbContext();
            context.Database.EnsureCreated();
        }
    }
}