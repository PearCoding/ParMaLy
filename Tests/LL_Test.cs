using System;
using System.Collections.Generic;
using System.Linq;

namespace PML.Tests
{
    using NUnit.Framework;

    [TestFixture]
    public class LL_Test
    {
        string Test_Source_1 =
            @"%start A;
            A:	'a' B;
            B:	'a' B;";
        
        [Test]
        public void LL0()
        {
            Logger logger = new Logger(false);
            Environment env = new Environment(logger);
            env.Parse(Test_Source_1);

            Parser.LL0 ll0 = new Parser.LL0();
            ll0.Generate(env, logger);

            Assert.AreEqual(ll0.Statistics.TD.Conflicts.Count, 0);
            var lookup = ll0.Lookup;

            Assert.AreEqual(lookup.Get(env.GroupByName("A"), new RuleLookahead(new RuleToken(RuleTokenType.Token, "a"))).Rule,
                env.GroupByName("A").Rules.First());
            Assert.AreEqual(lookup.Get(env.GroupByName("A"), null).Rule,
                env.GroupByName("A").Rules.First());

            Assert.AreEqual(lookup.Get(env.GroupByName("B"), new RuleLookahead(new RuleToken(RuleTokenType.Token, "a"))).Rule,
                env.GroupByName("B").Rules.First());
            Assert.AreEqual(lookup.Get(env.GroupByName("B"), null).Rule,
                env.GroupByName("B").Rules.First());
        }
    }
}