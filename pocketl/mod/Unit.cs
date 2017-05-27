using System.Collections.Generic;


namespace pocketl.mod
{
    public class Unit
    {
        public string name;
        public H<Package> package;
        public List<H<syn.Token>> tokens;
        public H<syn.Node> ast;


        public string ReadSource(Context ctx)
        {
            return ctx[this.package].filesystem.Read(this.name);
        }
    }
}
