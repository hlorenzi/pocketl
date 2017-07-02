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
            pass.FunctionHeaderResolver.Resolve(ctx, reporter, unit);
            pass.FunctionBodyResolver.Resolve(ctx, reporter, unit);

            var output = new util.OutputConsole();
            ctx[unit].ast.PrintDebug(output, ctx, ctx[unit].semanticMap);
            ctx.names.PrintDebug(output, ctx, true);
            reporter.Print(output, ctx);

            System.Console.ReadKey();
        }


        static string testCode = @"
            type Test
            {
                x: Int,
                y: (UInt64, Float, $Int8, *mut UInt16),
                z: fn(FooBar, ***Bool, $$mut$Int32),
                w: fn(hello) -> Bool,
                a: *Test,
                b: *mut Float32,
                c: $Int,
                d: $mut Int
            }

            fn hello(x: Int, y: Float)
            {
                {};
                {};
                {{}; {}; {}};

                let z = x;

                x;
                y;
                z;

                x = y;
                x = y = z;

                x = (y, z, w);
            }
        ";
    }
}
