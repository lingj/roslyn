﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.RemoveUnnecessaryImports;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Editor.CSharp.UnitTests.Diagnostics;
using Roslyn.Test.Utilities;
using Xunit;

namespace Microsoft.CodeAnalysis.Editor.CSharp.UnitTests.RemoveUnnecessaryImports
{
    public partial class RemoveUnnecessaryImportsTests : AbstractCSharpDiagnosticProviderBasedUserDiagnosticTest
    {
        internal override (DiagnosticAnalyzer, CodeFixProvider) CreateDiagnosticProviderAndFixer(Workspace workspace)
            => (new CSharpRemoveUnnecessaryImportsDiagnosticAnalyzer(),
                new CSharpRemoveUnnecessaryImportsCodeFixProvider());

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)]
        public async Task TestNoReferences()
        {
            await TestInRegularAndScriptAsync(
@"[|using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
    }
}|]",
@"class Program
{
    static void Main(string[] args)
    {
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)]
        public async Task TestIdentifierReferenceInTypeContext()
        {
            await TestInRegularAndScriptAsync(
@"[|using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
        DateTime d;
    }
}|]",
@"using System;

class Program
{
    static void Main(string[] args)
    {
        DateTime d;
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)]
        public async Task TestGenericReferenceInTypeContext()
        {
            await TestInRegularAndScriptAsync(
@"[|using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
        List<int> list;
    }
}|]",
@"using System.Collections.Generic;

class Program
{
    static void Main(string[] args)
    {
        List<int> list;
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)]
        public async Task TestMultipleReferences()
        {
            await TestInRegularAndScriptAsync(
@"[|using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
        List<int> list;
        DateTime d;
    }
}|]",
@"using System;
using System.Collections.Generic;

class Program
{
    static void Main(string[] args)
    {
        List<int> list;
        DateTime d;
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)]
        public async Task TestExtensionMethodReference()
        {
            await TestInRegularAndScriptAsync(
@"[|using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
        args.Where(a => a.Length > 10);
    }
}|]",
@"using System.Linq;

class Program
{
    static void Main(string[] args)
    {
        args.Where(a => a.Length > 10);
    }
}");
        }

        [WorkItem(541827, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/541827")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)]
        public async Task TestExtensionMethodLinq()
        {
            // NOTE: Intentionally not running this test with Script options, because in Script,
            // NOTE: class "Goo" is placed inside the script class, and can't be seen by the extension
            // NOTE: method Select, which is not inside the script class.
            await TestMissingInRegularAndScriptAsync(
@"[|using System;
using System.Collections;
using SomeNS;

class Program
{
    static void Main()
    {
        Goo qq = new Goo();
        IEnumerable x = from q in qq
                        select q;
    }
}

public class Goo
{
    public Goo()
    {
    }
}

namespace SomeNS
{
    public static class SomeClass
    {
        public static IEnumerable Select(this Goo o, Func<object, object> f)
        {
            return null;
        }
    }
}|]");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)]
        public async Task TestAliasQualifiedAliasReference()
        {
            await TestInRegularAndScriptAsync(
@"[|using System;
using G = System.Collections.Generic;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
        G::List<int> list;
    }
}|]",
@"using G = System.Collections.Generic;

class Program
{
    static void Main(string[] args)
    {
        G::List<int> list;
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)]
        public async Task TestQualifiedAliasReference()
        {
            await TestInRegularAndScriptAsync(
@"[|using System;
using G = System.Collections.Generic;

class Program
{
    static void Main(string[] args)
    {
        G.List<int> list;
    }
}|]",
@"using G = System.Collections.Generic;

class Program
{
    static void Main(string[] args)
    {
        G.List<int> list;
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)]
        public async Task TestNestedUnusedUsings()
        {
            await TestInRegularAndScriptAsync(
@"[|using System;
using System.Collections.Generic;
using System.Linq;

namespace N
{
    using System;

    class Program
    {
        static void Main(string[] args)
        {
            DateTime d;
        }
    }
}|]",
@"namespace N
{
    using System;

    class Program
    {
        static void Main(string[] args)
        {
            DateTime d;
        }
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)]
        public async Task TestNestedUsedUsings()
        {
            await TestInRegularAndScriptAsync(
@"[|using System;
using System.Collections.Generic;
using System.Linq;

namespace N
{
    using System;

    class Program
    {
        static void Main(string[] args)
        {
            DateTime d;
        }
    }
}

class F
{
    DateTime d;
}|]",
@"using System;

namespace N
{
    using System;

    class Program
    {
        static void Main(string[] args)
        {
            DateTime d;
        }
    }
}

class F
{
    DateTime d;
}");
        }

        [WorkItem(712656, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/712656")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)]
        public async Task TestNestedUsedUsings2()
        {
            await TestInRegularAndScriptAsync(
@"using System;
using System.Collections.Generic;
using System.Linq;

namespace N
{
    [|using System;|]
    using System.Collections.Generic;

    class Program
    {
        static void Main(string[] args)
        {
            DateTime d;
        }
    }
}

class F
{
    DateTime d;
}",
@"using System;

namespace N
{
    using System;

    class Program
    {
        static void Main(string[] args)
        {
            DateTime d;
        }
    }
}

class F
{
    DateTime d;
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)]
        public async Task TestAttribute()
        {
            await TestMissingInRegularAndScriptAsync(
@"[|using SomeNamespace;

[SomeAttr]
class Goo
{
}

namespace SomeNamespace
{
    public class SomeAttrAttribute : System.Attribute
    {
    }
}|]");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)]
        public async Task TestAttributeArgument()
        {
            await TestMissingInRegularAndScriptAsync(
@"[|using goo;

[SomeAttribute(typeof(SomeClass))]
class Program
{
    static void Main()
    {
    }
}

public class SomeAttribute : System.Attribute
{
    public SomeAttribute(object f)
    {
    }
}

namespace goo
{
    public class SomeClass
    {
    }
}|]");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)]
        public async Task TestRemoveAllWithSurroundingPreprocessor()
        {
            await TestInRegularAndScriptAsync(
@"#if true

[|using System;
using System.Collections.Generic;

#endif

class Program
{
    static void Main(string[] args)
    {
    }
}|]",
@"#if true


#endif

class Program
{
    static void Main(string[] args)
    {
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)]
        public async Task TestRemoveFirstWithSurroundingPreprocessor()
        {
            await TestInRegularAndScriptAsync(
@"#if true

[|using System;
using System.Collections.Generic;

#endif

class Program
{
    static void Main(string[] args)
    {
        List<int> list;
    }
}|]",
@"#if true

using System.Collections.Generic;

#endif

class Program
{
    static void Main(string[] args)
    {
        List<int> list;
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)]
        public async Task TestRemoveAllWithSurroundingPreprocessor2()
        {
            await TestInRegularAndScriptAsync(
@"[|namespace N
{
#if true

    using System;
    using System.Collections.Generic;

#endif

    class Program
    {
        static void Main(string[] args)
        {
        }
    }
}|]",
@"namespace N
{
#if true


#endif

    class Program
    {
        static void Main(string[] args)
        {
        }
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)]
        public async Task TestRemoveOneWithSurroundingPreprocessor2()
        {
            await TestInRegularAndScriptAsync(
@"[|namespace N
{
#if true

    using System;
    using System.Collections.Generic;

#endif

    class Program
    {
        static void Main(string[] args)
        {
            List<int> list;
        }
    }
}|]",
@"namespace N
{
#if true

    using System.Collections.Generic;

#endif

    class Program
    {
        static void Main(string[] args)
        {
            List<int> list;
        }
    }
}");
        }

        [WorkItem(541817, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/541817")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)]
        public async Task TestComments8718()
        {
            await TestInRegularAndScriptAsync(
@"[|using Goo; using System.Collections.Generic; /*comment*/ using Goo2;

class Program
{
    static void Main(string[] args)
    {
        Bar q;
        Bar2 qq;
    }
}

namespace Goo
{
    public class Bar
    {
    }
}

namespace Goo2
{
    public class Bar2
    {
    }
}|]",
@"using Goo;
using Goo2;

class Program
{
    static void Main(string[] args)
    {
        Bar q;
        Bar2 qq;
    }
}

namespace Goo
{
    public class Bar
    {
    }
}

namespace Goo2
{
    public class Bar2
    {
    }
}");
        }

        [WorkItem(528609, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/528609")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)]
        public async Task TestComments()
        {
            await TestInRegularAndScriptAsync(
@"//c1
/*c2*/
[|using/*c3*/ System/*c4*/; //c5
//c6

class Program
{
}
|]",
@"//c1
/*c2*/
//c6

class Program
{
}
");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)]
        public async Task TestUnusedUsing()
        {
            await TestInRegularAndScriptAsync(
@"[|using System.Collections.Generic;

class Program
{
    static void Main()
    {
    }
}|]",
@"class Program
{
    static void Main()
    {
    }
}");
        }

        [WorkItem(541827, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/541827")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)]
        public async Task TestSimpleQuery()
        {
            await TestInRegularAndScriptAsync(
@"[|using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
        var q = from a in args
                where a.Length > 21
                select a;
    }
}|]",
@"using System.Linq;

class Program
{
    static void Main(string[] args)
    {
        var q = from a in args
                where a.Length > 21
                select a;
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)]
        public async Task TestUsingStaticClassAccessField1()
        {
            await TestAsync(
@"[|using SomeNS.Goo;

class Program
{
    static void Main()
    {
        var q = x;
    }
}

namespace SomeNS
{
    static class Goo
    {
        public static int x;
    }
}|]",
@"class Program
{
    static void Main()
    {
        var q = x;
    }
}

namespace SomeNS
{
    static class Goo
    {
        public static int x;
    }
}",
                CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp5));
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)]
        public async Task TestUsingStaticClassAccessField2()
        {
            await TestMissingInRegularAndScriptAsync(
@"[|using static SomeNS.Goo;

class Program
{
    static void Main()
    {
        var q = x;
    }
}

namespace SomeNS
{
    static class Goo
    {
        public static int x;
    }
}|]");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)]
        public async Task TestUsingStaticClassAccessMethod1()
        {
            await TestAsync(
@"[|using SomeNS.Goo;

class Program
{
    static void Main()
    {
        var q = X();
    }
}

namespace SomeNS
{
    static class Goo
    {
        public static int X()
        {
            return 42;
        }
    }
}|]",
@"[|class Program
{
    static void Main()
    {
        var q = X();
    }
}

namespace SomeNS
{
    static class Goo
    {
        public static int X()
        {
            return 42;
        }
    }
}|]",
                CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp5));
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)]
        public async Task TestUsingStaticClassAccessMethod2()
        {
            await TestMissingInRegularAndScriptAsync(
@"[|using static SomeNS.Goo;

class Program
{
    static void Main()
    {
        var q = X();
    }
}

namespace SomeNS
{
    static class Goo
    {
        public static int X()
        {
            return 42;
        }
    }
}|]");
        }

        [WorkItem(8846, "DevDiv_Projects/Roslyn")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)]
        public async Task TestUnusedTypeImportIsRemoved()
        {
            await TestInRegularAndScriptAsync(
@"[|using SomeNS.Goo;

class Program
{
    static void Main()
    {
    }
}

namespace SomeNS
{
    static class Goo
    {
    }
}|]",
@"class Program
{
    static void Main()
    {
    }
}

namespace SomeNS
{
    static class Goo
    {
    }
}");
        }

        [WorkItem(541817, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/541817")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)]
        public async Task TestRemoveTrailingComment()
        {
            await TestInRegularAndScriptAsync(
@"[|using System.Collections.Generic; // comment

class Program
{
    static void Main(string[] args)
    {
    }
}

|]",
@"class Program
{
    static void Main(string[] args)
    {
    }
}

");
        }

        [WorkItem(541914, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/541914")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)]
        public async Task TestRemovingUnbindableUsing()
        {
            await TestInRegularAndScriptAsync(
@"[|using gibberish;

public static class Program
{
}|]",
@"public static class Program
{
}");
        }

        [WorkItem(541937, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/541937")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)]
        public async Task TestAliasInUse()
        {
            await TestMissingInRegularAndScriptAsync(
@"[|using GIBBERISH = Goo.Bar;

class Program
{
    static void Main(string[] args)
    {
        GIBBERISH x;
    }
}

namespace Goo
{
    public class Bar
    {
    }
}|]");
        }

        [WorkItem(541914, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/541914")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)]
        public async Task TestRemoveUnboundUsing()
        {
            await TestInRegularAndScriptAsync(
@"[|using gibberish;

public static class Program
{
}|]",
@"public static class Program
{
}");
        }

        [WorkItem(542016, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/542016")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)]
        public async Task TestLeadingNewlines1()
        {
            await TestInRegularAndScriptAsync(
@"[|using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {

    }
}|]",
@"class Program
{
    static void Main(string[] args)
    {

    }
}");
        }

        [WorkItem(542016, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/542016")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)]
        public async Task TestRemoveLeadingNewLines2()
        {
            await TestInRegularAndScriptAsync(
@"[|namespace N
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    class Program
    {
        static void Main(string[] args)
        {

        }
    }
}|]",
@"namespace N
{
    class Program
    {
        static void Main(string[] args)
        {

        }
    }
}");
        }

        [WorkItem(542134, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/542134")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)]
        public async Task TestImportedTypeUsedAsGenericTypeArgument()
        {
            await TestMissingInRegularAndScriptAsync(
@"[|using GenericThingie;

public class GenericType<T>
{
}

namespace GenericThingie
{
    public class Something
    {
    }
}

public class Program
{
    void goo()
    {
        GenericType<Something> type;
    }
}|]");
        }

        [WorkItem(542723, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/542723")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)]
        public async Task TestRemoveCorrectUsing1()
        {
            await TestAsync(
@"[|using System.Collections.Generic;

namespace Goo
{
    using Bar = Dictionary<string, string>;
}|]",
@"using System.Collections.Generic;

namespace Goo
{
}",
parseOptions: null);
        }

        [WorkItem(542723, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/542723")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)]
        public async Task TestRemoveCorrectUsing2()
        {
            await TestMissingAsync(
@"[|using System.Collections.Generic;

namespace Goo
{
    using Bar = Dictionary<string, string>;

    class C
    {
        Bar b;
    }
}|]");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)]
        public async Task TestSpan()
        {
            await TestSpansAsync(
@"namespace N
{
    [|using System;|]
}");
        }

        [WorkItem(543000, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/543000")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)]
        public async Task TestMissingWhenErrorsWouldBeGenerated()
        {
            await TestMissingInRegularAndScriptAsync(
@"[|using System;
using X;
using Y;

class B
{
    static void Main()
    {
        Bar(x => x.Goo());
    }

    static void Bar(Action<int> x)
    {
    }

    static void Bar(Action<string> x)
    {
    }
}

namespace X
{
    public static class A
    {
        public static void Goo(this int x)
        {
        }

        public static void Goo(this string x)
        {
        }
    }
}

namespace Y
{
    public static class B
    {
        public static void Goo(this int x)
        {
        }
    }
}|]");
        }

        [WorkItem(544976, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/544976")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)]
        public async Task TestMissingWhenMeaningWouldChangeInLambda()
        {
            await TestMissingInRegularAndScriptAsync(
@"[|using System;
using X;
using Y;

class B
{
    static void Main()
    {
        Bar(x => x.Goo(), null); // Prints 1
    }

    static void Bar(Action<string> x, object y)
    {
        Console.WriteLine(1);
    }

    static void Bar(Action<int> x, string y)
    {
        Console.WriteLine(2);
    }
}

namespace X
{
    public static class A
    {
        public static void Goo(this int x)
        {
        }

        public static void Goo(this string x)
        {
        }
    }
}

namespace Y
{
    public static class B
    {
        public static void Goo(this int x)
        {
        }
    }
}|]");
        }

        [WorkItem(544976, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/544976")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)]
        public async Task TestCasesWithLambdas1()
        {
            // NOTE: Y is used when speculatively binding "x => x.Goo()".  As such, it is marked as
            // used even though it isn't in the final bind, and could be removed.  However, as we do
            // not know if it was necessary to eliminate a speculative lambda bind, we must leave
            // it.
            await TestMissingInRegularAndScriptAsync(
@"[|using System;
using X;
using Y;

class B
{
    static void Main()
    {
        Bar(x => x.Goo(), null); // Prints 1
    }

    static void Bar(Action<string> x, object y)
    {
    }
}

namespace X
{
    public static class A
    {
        public static void Goo(this string x)
        {
        }
    }
}

namespace Y
{
    public static class B
    {
        public static void Goo(this int x)
        {
        }
    }
}|]");
        }

        [WorkItem(545646, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545646")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)]
        public async Task TestCasesWithLambdas2()
        {
            await TestMissingInRegularAndScriptAsync(
@"[|using System;
using N; // Falsely claimed as unnecessary

static class C
{
    static void Ex(this string x)
    {
    }

    static void Inner(Action<string> x, string y)
    {
    }

    static void Inner(Action<string> x, int y)
    {
    }

    static void Inner(Action<int> x, int y)
    {
    }

    static void Outer(Action<string> x, object y)
    {
        Console.WriteLine(1);
    }

    static void Outer(Action<int> x, string y)
    {
        Console.WriteLine(2);
    }

    static void Main()
    {
        Outer(y => Inner(x => x.Ex(), y), null);
    }
}

namespace N
{
    static class E
    {
        public static void Ex(this int x)
        {
        }
    }
}|]");
        }

        [WorkItem(545741, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545741")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)]
        public async Task TestMissingOnAliasedVar()
        {
            await TestMissingInRegularAndScriptAsync(
@"[|using var = var;

class var
{
}

class B
{
    static void Main()
    {
        var a = 1;
    }
}|]");
        }

        [WorkItem(546115, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/546115")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)]
        public async Task TestBrokenCode()
        {
            await TestMissingInRegularAndScriptAsync(
@"[|using System.Linq;

public class QueryExpressionTest
{
    public static void Main()
    {
        var expr1 = new[] { };
        var expr2 = new[] { };
        var query8 = from int i in expr1
                     join int fixed in expr2 on i equals fixed select new { i, fixed };

    var query9 = from object i in expr1
                 join object fixed in expr2 on i equals fixed select new { i, fixed };
  }
}|]");
        }

        [WorkItem(530980, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/530980")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)]
        public async Task TestReferenceInCref()
        {
            // parsing doc comments as simple trivia; System is unnecessary
            await TestInRegularAndScriptAsync(
@"[|using System;
/// <summary><see cref=""String"" /></summary>
class C
{
}|]",
@"/// <summary><see cref=""String"" /></summary>
class C
{
}");

            // fully parsing doc comments; System is necessary
            await TestMissingAsync(
@"[|using System;
/// <summary><see cref=""String"" /></summary>
class C
{
}|]", new TestParameters(Options.Regular.WithDocumentationMode(DocumentationMode.Diagnose)));
        }

        [WorkItem(751283, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/751283")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)]
        public async Task TestUnusedUsingOverLinq()
        {
            await TestInRegularAndScriptAsync(
@"using System;
[|using System.Linq;
using System.Threading.Tasks;|]

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine();
    }
}",
@"using System;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine();
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)]
        [WorkItem(20377, "https://github.com/dotnet/roslyn/issues/20377")]
        public async Task TestWarningLevel0()
        {
            // Ideally we would also want features to work with /warn:0, but this causes a compatibility conflict since
            // /warn:0 can be used to suppress warnings with /warnaserror enabled.
            await TestMissingInRegularAndScriptAsync(
@"[|using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
    }
}|]", new TestParameters(compilationOptions: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, warningLevel: 0)));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [Trait(Traits.Feature, Traits.Features.CodeActionsRemoveUnnecessaryImports)]
        [WorkItem(20377, "https://github.com/dotnet/roslyn/issues/20377")]
        public async Task TestWarningLevel(int warningLevel)
        {
            await TestInRegularAndScriptAsync(
@"[|using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
    }
}|]",
@"class Program
{
    static void Main(string[] args)
    {
    }
}", compilationOptions: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, warningLevel: warningLevel));
        }
    }
}
