using System;
using System.Collections.Generic;
using System.Text;

namespace OGA.Sequence.Model
{
    public enum eStepState
    {
        Unknown,
        Ready,
        Running,
        Aborted,
        Cancelled,
        Completed
    }

    public enum eTransitionState
    {
        Unknown,
        Ready,
        Error
    }

    /// <summary>
    /// Assigns a reporting entry to a particular phase of executing:
    /// Loading config, validating config, running step, reporting, etc...
    /// </summary>
    public enum eResultPhase
    {
        NA,
        Loading,
        Validation,
        Running,
        Reporting
    }

    public enum eEntryType
    {
        Base,
        StateChange,
        Transition,
        Start,
        End,
        Error,
        Cancellation,
        Disposition,
        OverallDisposition
    }

    public enum eObjectType
    {
        TransitionConfig,
        StepConfig,
        SequenceConfig,
        Sequence,
        Step,
        Transition,
        Result,
        Phase
    }

    public enum eDisposition
    {
        Unknown,
        Skipped,
        Cancelled,
        Aborted,
        Completed,
        Pass,
        PasswException,
        Fail
    }
}
