using System;
using System.Collections.Generic;
using System.Linq;

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
            Environment env = new Environment(new Logger(false));
            env.Parse(Test_Source);
            env.FirstCache.Setup(env, 1);
            
            var A = env.FirstCache.Get(env.GroupByName("A"), 1);
            var B = env.FirstCache.Get(env.GroupByName("B"), 1);
            var S = env.FirstCache.Get(env.GroupByName("S"), 1);
            var T = env.FirstCache.Get(env.GroupByName("T"), 1);
            var F = env.FirstCache.Get(env.GroupByName("F"), 1);

            Assert.AreEqual(A.Count(), 2);
            Assert.AreEqual(B.Count(), 2);
            Assert.AreEqual(S.Count(), 2);
            Assert.AreEqual(T.Count(), 2);
            Assert.AreEqual(F.Count(), 2);

            Assert.AreEqual(A[0].Count(), 1);
            Assert.AreEqual(A[1].Count(), 1);
            Assert.AreEqual(B[0].Count(), 1);
            Assert.IsNull(B[1]);
            Assert.AreEqual(S[0].Count(), 1);
            Assert.AreEqual(S[1].Count(), 1);
            Assert.AreEqual(T[0].Count(), 1);
            Assert.IsNull(T[1]);
            Assert.AreEqual(F[0].Count(), 1);
            Assert.AreEqual(F[1].Count(), 1);

            Assert.AreEqual(new RuleToken(RuleTokenType.Token, "("), A[0][0]);
            Assert.AreEqual(new RuleToken(RuleTokenType.Token, "ID"), A[1][0]);

            Assert.AreEqual(new RuleToken(RuleTokenType.Token, "+"), B[0][0]);
        
            Assert.AreEqual(new RuleToken(RuleTokenType.Token, "("), S[0][0]);
            Assert.AreEqual(new RuleToken(RuleTokenType.Token, "ID"), S[1][0]);

            Assert.AreEqual(new RuleToken(RuleTokenType.Token, "*"), T[0][0]);

            Assert.AreEqual(new RuleToken(RuleTokenType.Token, "("), F[0][0]);
            Assert.AreEqual(new RuleToken(RuleTokenType.Token, "ID"), F[1][0]);
        }

        [Test]
        public void Follow()
        {
            //TODO

            //Environment env = new Environment(new Logger(false));
            //env.Parse(Test_Source);
            //env.FirstCache.Setup(env, 1);
            //FollowSet.Setup(env);

            //RuleGroup A = env.GroupByName("A");
            //RuleGroup B = env.GroupByName("B");
            //RuleGroup S = env.GroupByName("S");
            //RuleGroup T = env.GroupByName("T");
            //RuleGroup F = env.GroupByName("F");

            //Assert.NotNull(A);
            //Assert.NotNull(B);
            //Assert.NotNull(S);
            //Assert.NotNull(T);
            //Assert.NotNull(F);

            //Assert.NotNull(A.FollowSet);
            //Assert.NotNull(B.FollowSet);
            //Assert.NotNull(S.FollowSet);
            //Assert.NotNull(T.FollowSet);
            //Assert.NotNull(F.FollowSet);

            //Assert.AreEqual(A.FollowSet.Count, 2);
            //Assert.AreEqual(B.FollowSet.Count, 2);
            //Assert.AreEqual(S.FollowSet.Count, 3);
            //Assert.AreEqual(T.FollowSet.Count, 3);
            //Assert.AreEqual(F.FollowSet.Count, 4);

            //Assert.Contains(null, A.FollowSet);
            //Assert.Contains(new RuleToken(RuleTokenType.Token, ")"), A.FollowSet);

            //Assert.Contains(null, B.FollowSet);
            //Assert.Contains(new RuleToken(RuleTokenType.Token, ")"), B.FollowSet);

            //Assert.Contains(null, S.FollowSet);
            //Assert.Contains(new RuleToken(RuleTokenType.Token, "+"), S.FollowSet);
            //Assert.Contains(new RuleToken(RuleTokenType.Token, ")"), S.FollowSet);

            //Assert.Contains(null, T.FollowSet);
            //Assert.Contains(new RuleToken(RuleTokenType.Token, "+"), T.FollowSet);
            //Assert.Contains(new RuleToken(RuleTokenType.Token, ")"), T.FollowSet);

            //Assert.Contains(null, F.FollowSet);
            //Assert.Contains(new RuleToken(RuleTokenType.Token, "+"), F.FollowSet);
            //Assert.Contains(new RuleToken(RuleTokenType.Token, "*"), F.FollowSet);
            //Assert.Contains(new RuleToken(RuleTokenType.Token, ")"), F.FollowSet);
        }

        string Test_Source_2 =
            @"%start A;
            A:	'a' B 'a' 'a' | 'b' C 'b' 'a' ;
            B:	'b' | /*EMPTY*/ ;
            C:	'b' | /*EMPTY*/ ;";

        [Test]
        public void First2()
        {
            Environment env = new Environment(new Logger(false));
            env.Parse(Test_Source_2);
            env.FirstCache.Setup(env, 2);

            var A = env.FirstCache.Get(env.GroupByName("A"), 2);
            var B = env.FirstCache.Get(env.GroupByName("B"), 2);
            var C = env.FirstCache.Get(env.GroupByName("C"), 2);
            Assert.IsNotNull(A);
            Assert.IsNotNull(B);
            Assert.IsNotNull(C);

            Assert.AreEqual(A.Count(), 3);
            Assert.AreEqual(B.Count(), 2);
            Assert.AreEqual(C.Count(), 2);

            Assert.AreEqual(A[0].Count(), 2);
            Assert.AreEqual(A[1].Count(), 2);
            Assert.AreEqual(A[2].Count(), 2);

            Assert.AreEqual(B[0].Count(), 1);
            Assert.IsNull(B[1]);

            Assert.AreEqual(C[0].Count(), 1);
            Assert.IsNull(C[1]);

            Assert.AreEqual(new RuleToken(RuleTokenType.Token, "a"), A[0][0]);
            Assert.AreEqual(new RuleToken(RuleTokenType.Token, "b"), A[0][1]);
            Assert.AreEqual(new RuleToken(RuleTokenType.Token, "a"), A[1][0]);
            Assert.AreEqual(new RuleToken(RuleTokenType.Token, "a"), A[1][1]);
            Assert.AreEqual(new RuleToken(RuleTokenType.Token, "b"), A[2][0]);
            Assert.AreEqual(new RuleToken(RuleTokenType.Token, "b"), A[2][1]);

            Assert.AreEqual(new RuleToken(RuleTokenType.Token, "b"), B[0][0]);
            Assert.AreEqual(new RuleToken(RuleTokenType.Token, "b"), C[0][0]);
        }
        
        string Test_Source_3 =
            @"%start A;
            A:	B 'b' 'a' | 'b' B 'a' 'a' ;
            B:	'b' C | /*EMPTY*/ ;
            C:	'a' 'b' | 'b' C | /*EMPTY*/ ;";
        [Test]
        public void First3()
        {
            Environment env = new Environment(new Logger(false));
            env.Parse(Test_Source_3);
            env.FirstCache.Setup(env, 3);

            var A = env.FirstCache.Get(env.GroupByName("A"), 3);
            var B = env.FirstCache.Get(env.GroupByName("B"), 3);
            var C = env.FirstCache.Get(env.GroupByName("C"), 3);

            Assert.IsNotNull(A);
            Assert.IsNotNull(B);
            Assert.IsNotNull(C);

            Assert.AreEqual(A.Count(), 5);
            Assert.AreEqual(B.Count(), 4);
            Assert.AreEqual(C.Count(), 6);

            Assert.AreEqual(A[0].Count(), 3);
            Assert.AreEqual(A[1].Count(), 3);
            Assert.AreEqual(A[2].Count(), 3);
            Assert.AreEqual(A[3].Count(), 2);
            Assert.AreEqual(A[4].Count(), 3);

            Assert.AreEqual(B[0].Count(), 3);
            Assert.AreEqual(B[1].Count(), 3);
            Assert.AreEqual(B[2].Count(), 3);
            Assert.IsNull(B[3]);

            Assert.AreEqual(C[0].Count(), 2);
            Assert.AreEqual(C[1].Count(), 3);
            Assert.AreEqual(C[2].Count(), 3);
            Assert.AreEqual(C[3].Count(), 3);
            Assert.AreEqual(C[4].Count(), 2);
            Assert.IsNull(C[5]);

            Assert.AreEqual(new RuleToken(RuleTokenType.Token, "b"), A[0][0]);
            Assert.AreEqual(new RuleToken(RuleTokenType.Token, "a"), A[0][1]);
            Assert.AreEqual(new RuleToken(RuleTokenType.Token, "b"), A[0][2]);

            Assert.AreEqual(new RuleToken(RuleTokenType.Token, "b"), A[1][0]);
            Assert.AreEqual(new RuleToken(RuleTokenType.Token, "b"), A[1][1]);
            Assert.AreEqual(new RuleToken(RuleTokenType.Token, "a"), A[1][2]);

            Assert.AreEqual(new RuleToken(RuleTokenType.Token, "b"), A[2][0]);
            Assert.AreEqual(new RuleToken(RuleTokenType.Token, "b"), A[2][1]);
            Assert.AreEqual(new RuleToken(RuleTokenType.Token, "b"), A[2][2]);

            Assert.AreEqual(new RuleToken(RuleTokenType.Token, "b"), A[3][0]);
            Assert.AreEqual(new RuleToken(RuleTokenType.Token, "a"), A[3][1]);

            Assert.AreEqual(new RuleToken(RuleTokenType.Token, "b"), A[4][0]);
            Assert.AreEqual(new RuleToken(RuleTokenType.Token, "a"), A[4][1]);
            Assert.AreEqual(new RuleToken(RuleTokenType.Token, "a"), A[4][2]);

            Assert.AreEqual(new RuleToken(RuleTokenType.Token, "b"), B[0][0]);
            Assert.AreEqual(new RuleToken(RuleTokenType.Token, "a"), B[0][1]);
            Assert.AreEqual(new RuleToken(RuleTokenType.Token, "b"), B[0][2]);

            Assert.AreEqual(new RuleToken(RuleTokenType.Token, "b"), B[1][0]);
            Assert.AreEqual(new RuleToken(RuleTokenType.Token, "b"), B[1][1]);
            Assert.AreEqual(new RuleToken(RuleTokenType.Token, "a"), B[1][2]);

            Assert.AreEqual(new RuleToken(RuleTokenType.Token, "b"), B[2][0]);
            Assert.AreEqual(new RuleToken(RuleTokenType.Token, "b"), B[2][1]);
            Assert.AreEqual(new RuleToken(RuleTokenType.Token, "b"), B[2][2]);

            Assert.AreEqual(new RuleToken(RuleTokenType.Token, "a"), C[0][0]);
            Assert.AreEqual(new RuleToken(RuleTokenType.Token, "b"), C[0][1]);

            Assert.AreEqual(new RuleToken(RuleTokenType.Token, "b"), C[1][0]);
            Assert.AreEqual(new RuleToken(RuleTokenType.Token, "a"), C[1][1]);
            Assert.AreEqual(new RuleToken(RuleTokenType.Token, "b"), C[1][2]);

            Assert.AreEqual(new RuleToken(RuleTokenType.Token, "b"), C[2][0]);
            Assert.AreEqual(new RuleToken(RuleTokenType.Token, "b"), C[2][1]);
            Assert.AreEqual(new RuleToken(RuleTokenType.Token, "b"), C[2][2]);

            Assert.AreEqual(new RuleToken(RuleTokenType.Token, "b"), C[3][0]);
            Assert.AreEqual(new RuleToken(RuleTokenType.Token, "b"), C[3][1]);
            Assert.AreEqual(new RuleToken(RuleTokenType.Token, "a"), C[3][2]);

            Assert.AreEqual(new RuleToken(RuleTokenType.Token, "b"), C[4][0]);
            Assert.AreEqual(new RuleToken(RuleTokenType.Token, "b"), C[4][1]);
        }
    }
}