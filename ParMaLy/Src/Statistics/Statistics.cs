using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PML.Statistics
{
    public class Statistics
    {
        public BUStatistics BU;
        public TDStatistics TD;

        public long TimeElapsed = 0;//ms
    }

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
            public BU.RuleState State;
            public RuleToken Token;

            public ConflictEntry(ConflictType type, BU.RuleState state, RuleToken token = null)
            {
                Type = type;
                State = state;
                Token = token;
            }
        }

        List<ConflictEntry> _Conflicts = new List<ConflictEntry>();
        public List<ConflictEntry> Conflicts { get { return _Conflicts; } }

        public class ProcessEntry
        {
            public long TimeElapsed;
            public int TotalStages;
            public int QueueJobs;
            public BU.RuleState Job;

            public ProcessEntry(BU.RuleState job, int stages, int jobs)
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

    public class TDStatistics
    {
        public enum ConflictType
        {
            Lookup,
            Internal
        }

        public class ConflictEntry
        {
            public ConflictType Type;
            public RuleGroup Group;
            public Rule Rule;
            public RuleToken Token;

            public ConflictEntry(ConflictType type, RuleGroup grp, RuleToken token, Rule rule)
            {
                Type = type;
                Group = grp;
                Rule = rule;
                Token = token;
            }
        }

        List<ConflictEntry> _Conflicts = new List<ConflictEntry>();
        public List<ConflictEntry> Conflicts { get { return _Conflicts; } }
    }
}
