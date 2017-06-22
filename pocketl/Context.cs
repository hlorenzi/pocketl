namespace pocketl
{
    public class Context
    {
        public HandleManager<mod.Package> packages = new HandleManager<mod.Package>();
        public HandleManager<mod.Unit> units = new HandleManager<mod.Unit>();
        public sema.Namespace names = new sema.Namespace();
        public HandleManager<sema.Def> defs = new HandleManager<sema.Def>();

        public H<sema.Def> primitiveBool;
        public H<sema.Def> primitiveInt;
        public H<sema.Def> primitiveInt8;
        public H<sema.Def> primitiveInt16;
        public H<sema.Def> primitiveInt32;
        public H<sema.Def> primitiveInt64;
        public H<sema.Def> primitiveUInt;
        public H<sema.Def> primitiveUInt8;
        public H<sema.Def> primitiveUInt16;
        public H<sema.Def> primitiveUInt32;
        public H<sema.Def> primitiveUInt64;
        public H<sema.Def> primitiveFloat;
        public H<sema.Def> primitiveFloat32;
        public H<sema.Def> primitiveFloat64;


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


        public sema.Def this[H<sema.Def> handle]
        {
            get { return this.defs[handle]; }
            set { this.defs[handle] = value; }
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


        public void AddPrimitives()
        {
            System.Func<string, H<sema.Def>> add = (name) =>
            {
                var hDef = this.defs.Reserve();
                var hNamespaceNode = this.names.FindOrReserve(name);

                this[hDef] = new sema.Def.Structure { namespaceNode = hNamespaceNode };
                hNamespaceNode.item = new sema.Namespace.Item.Def { def = hDef };
                return hDef;
            };

            this.primitiveBool = add("Bool");
            this.primitiveInt = add("Int");
            this.primitiveInt8 = add("Int8");
            this.primitiveInt16 = add("Int16");
            this.primitiveInt32 = add("Int32");
            this.primitiveInt64 = add("Int64");
            this.primitiveUInt = add("UInt");
            this.primitiveUInt8 = add("UInt8");
            this.primitiveUInt16 = add("UInt16");
            this.primitiveUInt32 = add("UInt32");
            this.primitiveUInt64 = add("UInt64");
            this.primitiveFloat = add("Float");
            this.primitiveFloat32 = add("Float32");
            this.primitiveFloat64 = add("Float64");
        }
    }
}
