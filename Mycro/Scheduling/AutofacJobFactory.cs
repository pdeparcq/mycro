using System;
using Autofac;
using Quartz;
using Quartz.Simpl;
using Quartz.Spi;

namespace Televic.Mycro.Scheduling
{
    public class AutofacJobFactory : SimpleJobFactory
    {
        private readonly IContainer _container;

        public AutofacJobFactory(IContainer container)
        {
            _container = container;
        }

        public override IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            try
            {
                // this will inject dependencies that the job requires
                return (IJob)this._container.Resolve(bundle.JobDetail.JobType);
            }
            catch (Exception e)
            {
                throw new SchedulerException(
                    $"Problem while instantiating job '{bundle.JobDetail.Key}' from the NinjectJobFactory.", e);
            }
        }
    }
}
