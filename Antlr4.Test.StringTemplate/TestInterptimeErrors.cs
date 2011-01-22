/*
 * [The "BSD licence"]
 * Copyright (c) 2011 Terence Parr
 * All rights reserved.
 *
 * Conversion to C#:
 * Copyright (c) 2011 Sam Harwell, Tunnel Vision Laboratories, LLC
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 * 3. The name of the author may not be used to endorse or promote products
 *    derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
 * IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
 * NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
 * DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

namespace Antlr4.Test.StringTemplate
{
    using Antlr4.StringTemplate;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Antlr4.StringTemplate.Misc;
    using Environment = System.Environment;

    [TestClass]
    public class TestInterptimeErrors : BaseTest
    {
        public class UserHiddenName
        {
            protected string name;
            public UserHiddenName(string name)
            {
                this.name = name;
            }
            protected string getName()
            {
                return name;
            }
        }

        public class UserHiddenNameField
        {
            protected string name;
            public UserHiddenNameField(string name)
            {
                this.name = name;
            }
        }

        [TestMethod]
        public void TestMissingEmbeddedTemplate()
        {
            ErrorBuffer errors = new ErrorBuffer();

            string templates =
                "t() ::= \"<foo()>\"" + Environment.NewLine;

            writeFile(tmpdir, "t.stg", templates);
            STGroup group = new STGroupFile(tmpdir + "/" + "t.stg");
            group.setListener(errors);
            ST st = group.getInstanceOf("t");
            st.render();
            string expected = "context [t] 1:0 no such template: foo" + newline;
            string result = errors.ToString();
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void TestMissingSuperTemplate()
        {
            ErrorBuffer errors = new ErrorBuffer();

            string templates =
                "t() ::= \"<super.t()>\"" + Environment.NewLine;

            writeFile(tmpdir, "t.stg", templates);
            STGroup group = new STGroupFile(tmpdir + "/" + "t.stg");
            group.setListener(errors);
            string templates2 =
                "u() ::= \"blech\"" + Environment.NewLine;

            writeFile(tmpdir, "t2.stg", templates2);
            STGroup group2 = new STGroupFile(tmpdir + "/" + "t2.stg");
            group.importTemplates(group2);
            ST st = group.getInstanceOf("t");
            st.render();
            string expected = "context [t] 1:1 no such template: super.t" + newline;
            string result = errors.ToString();
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void TestNoPropertyNotError()
        {
            ErrorBuffer errors = new ErrorBuffer();

            string templates =
                "t(u) ::= \"<u.x>\"" + Environment.NewLine;

            writeFile(tmpdir, "t.stg", templates);
            STGroup group = new STGroupFile(tmpdir + "/" + "t.stg");
            group.setListener(errors);
            ST st = group.getInstanceOf("t");
            st.add("u", new User(32, "parrt"));
            st.render();
            string expected = "";
            string result = errors.ToString();
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void TestHiddenPropertyNotError()
        {
            ErrorBuffer errors = new ErrorBuffer();

            string templates =
                "t(u) ::= \"<u.name>\"" + Environment.NewLine;

            writeFile(tmpdir, "t.stg", templates);
            STGroup group = new STGroupFile(tmpdir + "/" + "t.stg");
            group.setListener(errors);
            ST st = group.getInstanceOf("t");
            st.add("u", new UserHiddenName("parrt"));
            st.render();
            string expected = "";
            string result = errors.ToString();
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void TestHiddenFieldNotError()
        {
            ErrorBuffer errors = new ErrorBuffer();

            string templates =
                "t(u) ::= \"<u.name>\"" + Environment.NewLine;

            writeFile(tmpdir, "t.stg", templates);
            STGroup group = new STGroupFile(tmpdir + "/" + "t.stg");
            group.setListener(errors);
            ST st = group.getInstanceOf("t");
            st.add("u", new UserHiddenNameField("parrt"));
            st.render();
            string expected = "";
            string result = errors.ToString();
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void TestSoleArg()
        {
            ErrorBuffer errors = new ErrorBuffer();

            string templates =
                "t() ::= \"<u({9})>\"\n" +
                "u(x,y) ::= \"<x>\"\n";

            writeFile(tmpdir, "t.stg", templates);
            STGroup group = new STGroupFile(tmpdir + "/" + "t.stg");
            group.setListener(errors);
            ST st = group.getInstanceOf("t");
            st.render();
            string expected = "context [t] 1:1 passed 1 arg(s) to template u with 2 declared arg(s)" + newline;
            string result = errors.ToString();
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void TestSoleArgUsingApplySyntax()
        {
            ErrorBuffer errors = new ErrorBuffer();

            string templates =
                "t() ::= \"<{9}:u()>\"\n" +
                "u(x,y) ::= \"<x>\"\n";

            writeFile(tmpdir, "t.stg", templates);
            STGroup group = new STGroupFile(tmpdir + "/" + "t.stg");
            group.setListener(errors);
            ST st = group.getInstanceOf("t");
            string expected = "9";
            string result = st.render();
            Assert.AreEqual(expected, result);

            expected = "context [t] 1:5 passed 1 arg(s) to template u with 2 declared arg(s)" + newline;
            result = errors.ToString();
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void TestUndefinedAttr()
        {
            ErrorBuffer errors = new ErrorBuffer();

            string templates =
                "t() ::= \"<u()>\"\n" +
                "u() ::= \"<x>\"\n";

            writeFile(tmpdir, "t.stg", templates);
            STGroup group = new STGroupFile(tmpdir + "/" + "t.stg");
            group.setListener(errors);
            STGroup.debug = true;
            ST st = group.getInstanceOf("t");
            st.render();
            string expected = "context [t u] 1:1 attribute x isn't defined" + newline;
            string result = errors.ToString();
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void TestParallelAttributeIterationWithMissingArgs()
        {
            ErrorBuffer errors = new ErrorBuffer();
            STGroup group = new STGroup();
            group.setListener(errors);
            ST e = new ST(group,
                    "<names,phones,salaries:{n,p | <n>@<p>}; separator=\", \">"
                );
            e.add("names", "Ter");
            e.add("names", "Tom");
            e.add("phones", "1");
            e.add("phones", "2");
            e.add("salaries", "big");
            e.render();
            string errorExpecting =
                "1:23: anonymous template has 2 arg(s) but mapped across 3 value(s)" + newline +
                "context [anonymous] 1:23 passed 3 arg(s) to template _sub1 with 2 declared arg(s)" + newline +
                "context [anonymous] 1:1 iterating through 3 values in zip map but template has 2 declared arguments" + newline;
            Assert.AreEqual(errorExpecting, errors.ToString());
            string expecting = "Ter@1, Tom@2";
            Assert.AreEqual(expecting, e.render());
        }

        [TestMethod]
        public void TestStringTypeMismatch()
        {
            ErrorBuffer errors = new ErrorBuffer();
            STGroup group = new STGroup();
            group.setListener(errors);
            ST e = new ST(group, "<trim(s)>");
            e.add("s", 34);
            e.render(); // generate the error
            string errorExpecting = "context [anonymous] 1:1 function trim expects a string not System.Int32" + newline;
            Assert.AreEqual(errorExpecting, errors.ToString());
        }

        [TestMethod]
        public void TestStringTypeMismatch2()
        {
            ErrorBuffer errors = new ErrorBuffer();
            STGroup group = new STGroup();
            group.setListener(errors);
            ST e = new ST(group, "<strlen(s)>");
            e.add("s", 34);
            e.render(); // generate the error
            string errorExpecting = "context [anonymous] 1:1 function strlen expects a string not System.Int32" + newline;
            Assert.AreEqual(errorExpecting, errors.ToString());
        }
    }
}