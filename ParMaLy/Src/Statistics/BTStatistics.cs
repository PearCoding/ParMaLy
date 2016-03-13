using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PML.Statistics
{
    public class BTStatistics
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
    }
}
