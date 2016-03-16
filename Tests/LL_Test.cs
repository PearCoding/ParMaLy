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

        string Test_Source_2 =
            @"%start A;
            A:	'a' B;
            B:	'a' B | /* EMPTY */;";

        [Test]
        public void LL1()
        {
            Logger logger = new Logger(false);
            Environment env = new Environment(logger);
            env.Parse(Test_Source_2);

            // A LL(0) parser would fail in the above CFG case.
            Parser.LL0 ll0 = new Parser.LL0();
            ll0.Generate(env, logger);
            Assert.AreNotEqual(ll0.Statistics.TD.Conflicts.Count, 0);

            Parser.LLK ll1 = new Parser.LLK(1);
            ll1.Generate(env, logger);
            Assert.AreEqual(ll1.Statistics.TD.Conflicts.Count, 0);
            var lookup = ll1.Lookup;

            // TODO
        }

        string Test_Source_3 =
            @"%start A;
            A:	'a' B;
            B:	B 'a' | C 'a';
            C:  B 'b';";

        [Test]
        public void LL1_LeftRecursion()
        {
            Logger logger = new Logger(false);
            Environment env = new Environment(logger);
            env.Parse(Test_Source_3);

            // A LL(0) parser would fail in the above CFG case.
            Parser.LL0 ll0 = new Parser.LL0();
            ll0.Generate(env, logger);
            Assert.AreNotEqual(ll0.Statistics.TD.Conflicts.Count, 0);

            // Due to the Left-Recursion case, even a LL(1) (or LL(k)) parser would fail in the above CFG case.
            Parser.LLK ll1 = new Parser.LLK(1);
            ll1.Generate(env, logger);
            Assert.AreNotEqual(ll1.Statistics.TD.Conflicts.Count, 0);
        }

        string Test_Source_4 =
            @"%start A;
            A:	B | C ;
            B:	'a' 'b' ;
            C:  'a' 'a' ;";

        [Test]
        public void LL2()
        {
            Logger logger = new Logger(false);
            Environment env = new Environment(logger);
            env.Parse(Test_Source_4);

            // A LL(0) parser would fail in the above CFG case.
            Parser.LL0 ll0 = new Parser.LL0();
            ll0.Generate(env, logger);
            Assert.AreNotEqual(ll0.Statistics.TD.Conflicts.Count, 0);

            Parser.LLK ll1 = new Parser.LLK(1);
            ll1.Generate(env, logger);
            Assert.AreNotEqual(ll1.Statistics.TD.Conflicts.Count, 0);

            Parser.LLK ll2 = new Parser.LLK(2);
            ll2.Generate(env, logger);
            Assert.AreEqual(ll2.Statistics.TD.Conflicts.Count, 0);
            var lookup = ll2.Lookup;

            // TODO
        }
    }
}