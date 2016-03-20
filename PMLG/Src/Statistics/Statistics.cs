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
        public RStatistics R;

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
            public RuleLookahead Lookahead;

            public ConflictEntry(ConflictType type, BU.RuleState state, RuleLookahead lookahead = null)
            {
                Type = type;
                State = state;
                Lookahead = lookahead;
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
            public RuleLookahead Lookahead;

            public ConflictEntry(ConflictType type, RuleGroup grp, RuleLookahead lookahead, Rule rule)
            {
                Type = type;
                Group = grp;
                Rule = rule;
                Lookahead = lookahead;
            }
        }

        List<ConflictEntry> _Conflicts = new List<ConflictEntry>();
        public List<ConflictEntry> Conflicts { get { return _Conflicts; } }
    }

    public class RStatistics
    {
        public enum ConflictType
        {
            Decision,
            Internal
        }

        public class ConflictEntry
        {
            public ConflictType Type;
            public R.RState State;

            public ConflictEntry(ConflictType type, R.RState state)
            {
                Type = type;
                State = state;
            }
        }

        List<ConflictEntry> _Conflicts = new List<ConflictEntry>();
        public List<ConflictEntry> Conflicts { get { return _Conflicts; } }
    }
}
