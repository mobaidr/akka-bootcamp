using System;
using System.Collections.Generic;
using System.Text;
using Akka.Actor;

namespace WinTail
{
    public class TailCoordinatorActor : UntypedActor
    {
        #region Messages
        public class StartTail
        {
            public string FilePath { get; private set; }
            public IActorRef ReporterActor { get; private set; }

            public StartTail(string filePath, IActorRef reporterActor)
            {
                ReporterActor = reporterActor;
                FilePath = filePath;
            }
        }

        public class StopTail
        {
            public string FilePath { get; private set; }

            public StopTail(string filePath)
            {
                FilePath = filePath;
            }
        }

        #endregion
        protected override void OnReceive(object message)
        {
            if (message is StartTail)
            {
                var msg = message as StartTail;

                Context.ActorOf(Props.Create(() => new TailActor(msg.ReporterActor, msg.FilePath)));
            }
        }

        // TailCoordinatorActor.cs
        protected override SupervisorStrategy SupervisorStrategy()
        {
            return new OneForOneStrategy(
                10, // maxNumberOfRetries
                TimeSpan.FromSeconds(30), // withinTimeRange
                x => // localOnlyDecider
                    {
                        //Maybe we consider ArithmeticException to not be application critical
                        //so we just ignore the error and keep going.
                        if (x is ArithmeticException) return Directive.Resume;

                        //Error that we cannot recover from, stop the failing actor
                        else if (x is NotSupportedException) return Directive.Stop;

                        //In all other cases, just restart the failing actor
                        else return Directive.Restart;
                    });
        }

    }
}
