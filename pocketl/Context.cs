using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace pocketl
{
    public class Context
    {
        public HandleManager<mod.Package> packages = new HandleManager<mod.Package>();
        public HandleManager<mod.Unit> units = new HandleManager<mod.Unit>();
        public HandleManager<syn.Token> tokens = new HandleManager<syn.Token>();
        public HandleManager<syn.Node> nodes = new HandleManager<syn.Node>();


        public mod.Package this[H<mod.Package> handle]
        {
            get { return this.packages[handle]; }
            set { this.packages[handle] = value; }
        }


        public mod.Unit this[H<mod.Unit> handle]
        {
            get { return this.units[handle]; }
            set { this.units[handle] = value; }
        }


        public syn.Token this[H<syn.Token> handle]
        {
            get { return this.tokens[handle]; }
            set { this.tokens[handle] = value; }
        }


        public syn.Node this[H<syn.Node> handle]
        {
            get { return this.nodes[handle]; }
            set { this.nodes[handle] = value; }
        }


        public H<mod.Package> AddPackage(string name, util.FileSystem filesystem)
        {
            return this.packages.Add(new mod.Package
            {
                name = name,
                filesystem = filesystem
            });
        }


        public H<mod.Unit> AddUnit(H<mod.Package> hPackage, string name)
        {
            var hUnit = this.units.Add(new mod.Unit
            {
                name = name,
                package = hPackage
            });

            this.packages[hPackage].units.Add(hUnit);
            return hUnit;
        }
    }
}
