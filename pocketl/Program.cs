﻿namespace pocketl
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

            pass.Tokenizer.Tokenize(ctx, reporter, unit);
            pass.Parser.Parse(ctx, reporter, unit);

            reporter.PrintToConsole(ctx);
            ctx[ctx[unit].ast].PrintToConsole(ctx);
            System.Console.ReadKey();
        }


        static string testCode = @"
            fn hello(x: Int, y: Float)
            {
                
            }
        ";
    }
}
