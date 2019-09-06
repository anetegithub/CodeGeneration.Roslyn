using System;
using System.Collections.Generic;
using System.Text;

namespace Sample.Consumer
{
    public class StepStatus
    {
        public bool MoveNext => !Completed;

        internal bool Completed { get; set; }

        public StepStatus()
        {
        }

        public bool Proceed { get; set; }

        public object OutcomeValue { get; set; }

        public StepStatus(object outcome)
        {
            Proceed = true;
            OutcomeValue = outcome;
        }
    }
}
