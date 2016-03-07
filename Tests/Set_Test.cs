﻿using System;

namespace PML.Tests
{
    using NUnit.Framework;

    [TestFixture]
    public class FirstSet_Test
    {
        string Test_Source =
            @"%token ID;
            %start A;
            A:	S B;
            B:	'+' S B | /*EMPTY*/ ;
            S:	F T;
            T:	'*' F T | /*EMPTY*/ ;
            F:	'(' A ')' | ID ;";

        [Test]
        public void First()
        {
            Environment env = new Environment(new Logger());
            env.Parse(Test_Source);
            FirstSet.Setup(env);

            RuleGroup A = env.GroupByName("A");
            RuleGroup B = env.GroupByName("B");
            RuleGroup S = env.GroupByName("S");
            RuleGroup T = env.GroupByName("T");
            RuleGroup F = env.GroupByName("F");

            Assert.NotNull(A);
            Assert.NotNull(B);
            Assert.NotNull(S);
            Assert.NotNull(T);
            Assert.NotNull(F);

            Assert.NotNull(A.FirstSet);
            Assert.NotNull(B.FirstSet);
            Assert.NotNull(S.FirstSet);
            Assert.NotNull(T.FirstSet);
            Assert.NotNull(F.FirstSet);

            Assert.AreEqual(A.FirstSet.Count, 2);
            Assert.AreEqual(B.FirstSet.Count, 2);
            Assert.AreEqual(S.FirstSet.Count, 2);
            Assert.AreEqual(T.FirstSet.Count, 2);
            Assert.AreEqual(F.FirstSet.Count, 2);

            Assert.Contains("(", A.FirstSet);
            Assert.Contains("ID", A.FirstSet);

            Assert.Contains("+", B.FirstSet);
            Assert.Contains(null, B.FirstSet);

            Assert.Contains("(", S.FirstSet);
            Assert.Contains("ID", S.FirstSet);

            Assert.Contains("*", T.FirstSet);
            Assert.Contains(null, T.FirstSet);

            Assert.Contains("(", F.FirstSet);
            Assert.Contains("ID", F.FirstSet);
        }

        [Test]
        public void Follow()
        {
            Environment env = new Environment(new Logger());
            env.Parse(Test_Source);
            FirstSet.Setup(env);
            FollowSet.Setup(env);

            RuleGroup A = env.GroupByName("A");
            RuleGroup B = env.GroupByName("B");
            RuleGroup S = env.GroupByName("S");
            RuleGroup T = env.GroupByName("T");
            RuleGroup F = env.GroupByName("F");

            Assert.NotNull(A);
            Assert.NotNull(B);
            Assert.NotNull(S);
            Assert.NotNull(T);
            Assert.NotNull(F);

            Assert.NotNull(A.FollowSet);
            Assert.NotNull(B.FollowSet);
            Assert.NotNull(S.FollowSet);
            Assert.NotNull(T.FollowSet);
            Assert.NotNull(F.FollowSet);

            Assert.AreEqual(A.FollowSet.Count, 2);
            Assert.AreEqual(B.FollowSet.Count, 2);
            Assert.AreEqual(S.FollowSet.Count, 3);
            Assert.AreEqual(T.FollowSet.Count, 3);
            Assert.AreEqual(F.FollowSet.Count, 4);

            Assert.Contains(null, A.FollowSet);
            Assert.Contains(")", A.FollowSet);

            Assert.Contains(null, B.FollowSet);
            Assert.Contains(")", B.FollowSet);

            Assert.Contains(null, S.FollowSet);
            Assert.Contains("+", S.FollowSet);
            Assert.Contains(")", S.FollowSet);

            Assert.Contains(null, T.FollowSet);
            Assert.Contains("+", T.FollowSet);
            Assert.Contains(")", T.FollowSet);

            Assert.Contains(null, F.FollowSet);
            Assert.Contains("+", F.FollowSet);
            Assert.Contains("*", F.FollowSet);
            Assert.Contains(")", F.FollowSet);
        }
    }
}