namespace pocketl
{
    class Program
    {
        static void Main(string[] args)
        {
            var filesystem = new util.FileSystemMock();
            filesystem.Add("main.p", testCode);

            var ctx = new Context();
            var pkg = ctx.AddPackage("test", filesystem);
            var unit = ctx.AddUnit(pkg, "main.p");

            var reporter = new diagn.ReporterDefault();

            ctx.AddPrimitives();

            pass.Tokenizer.Tokenize(ctx, reporter, unit);
            pass.Parser.Parse(ctx, reporter, unit);
            pass.Collector.Collect(ctx, reporter, unit);
            pass.NameResolver.Resolve(ctx, reporter, unit);
            pass.StructureResolver.Resolve(ctx, reporter, unit);

            ctx[unit].ast.PrintToConsole(ctx, ctx[unit].semanticMap);
            ctx.names.PrintToConsole(ctx, true);
            reporter.PrintToConsole(ctx);
            System.Console.ReadKey();
        }


        static string testCode = @"
            type Test
            {
                x: Int,
                y: UInt64,
                z: FooBar,
                w: hello,
                a: Test,
                x: Float32
            }

            fn hello(x: Int, y: Float)
            {
                a = Test { };
                a = hello;

                a;
                a = b + c + d;
                a = b = c + d * e + f;
                a = b + !!!!c;
                a = b!!!! + c;
                a = (-!-!b!!!!);
                a.b.c = d.e!;

                a = 123;
                a = ();
                a = (0,);
                a = (0);
                a = (0, 1, 2);

                a = b;
                a = b {};
                a = b { c = 0 };
                a = b.x.y { c = 0, d = 1 };

                if x { a };
                if x { a } else { b };
                while x { a };
                loop { a };
                break;
                continue;
                return;
                return a;
            }
        ";
    }
}
