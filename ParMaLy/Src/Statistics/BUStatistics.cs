using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PML.Statistics
{
    public class BUStatistics
    {
        public enum ConflictType
        {
            ShiftReduce,
            ReduceReduce,
            ShiftShift,// Not a real conflict
            Accept,
            Internal
        }

        public class ConflictEntry
        {
            public ConflictType Type;
            public RuleState State;
            public string Token;

            public ConflictEntry(ConflictType type, RuleState state, string token = null)
            {
                Type = type;
                State = state;
                Token = token;
            }
        }

        List<ConflictEntry> _Conflicts = new List<ConflictEntry>();
        public List<ConflictEntry> Conflicts { get { return _Conflicts; } }

        public long TimeElapsed = 0;//ms

        public class ProcessEntry
        {
            public long TimeElapsed;
            public int TotalStages;
            public int QueueJobs;
            public RuleState Job;

            public ProcessEntry(RuleState job, int stages, int jobs)
            {
                Job = job;
                TotalStages = stages;
                QueueJobs = jobs;
                TimeElapsed = 0;
            }
        }

        List<ProcessEntry> _Proceedings = new List<ProcessEntry>();
        public List<ProcessEntry> Proceedings { get { return _Proceedings; } }
    }
}
