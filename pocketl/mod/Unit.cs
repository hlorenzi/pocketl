using System.Collections.Generic;


namespace pocketl.mod
{
    public class Unit
    {
        public string name;
        public H<Package> package;
        public List<syn.Token> tokens;
        public syn.Node ast;
        public sema.Map semanticMap = new sema.Map();


        public string ReadSource(Context ctx)
        {
            return ctx[this.package].filesystem.Read(this.name);
        }
    }
}
