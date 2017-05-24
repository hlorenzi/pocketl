namespace pocketl
{
    class Program
    {
        static void Main(string[] args)
        {
            var filesystem = new util.FileSystemMock();
            filesystem.Add("main.p", "Hello, world!");

            var ctx = new Context();
            var pkg = ctx.AddPackage("test", filesystem, "main.p");
        }
    }
}
