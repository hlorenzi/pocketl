namespace pocketl.pass
{
    public class StructureResolver
    {
        public static void Resolve(Context ctx, diagn.Reporter reporter, H<mod.Unit> hUnit)
        {
            var unit = ctx[hUnit];
            ResolveNode(ctx, reporter, unit, unit.ast);
        }


        private static void ResolveNode(Context ctx, diagn.Reporter reporter, mod.Unit unit, syn.Node node)
        {
            TryCollectStructureDef(ctx, reporter, unit, node as syn.Node.StructureDef);

            foreach (var child in node.Children())
                ResolveNode(ctx, reporter, unit, child);
        }


        private static void TryCollectStructureDef(Context ctx, diagn.Reporter reporter, mod.Unit unit, syn.Node.StructureDef node)
        {
            if (node == null)
                return;

            var hDef = unit.semanticMap.def[node];
            if (hDef == null)
                reporter.InternalError("def not found for node", new diagn.Caret(node.span));

            else
            {
                var structureDef = ctx[hDef] as sema.Def.Structure;
                
                foreach (var child in node.fields)
                {
                    var nodeField = child as syn.Node.StructureDefField;
                    var nodeIdentifier = nodeField.identifier as syn.Node.Identifier;

                    var fieldName = nodeIdentifier.token.excerpt;
                    var duplicateField = structureDef.fields.Find(f => f.name == fieldName);
                    if (duplicateField != null)
                        reporter.Error("duplicate field `" + fieldName + "`",
                            new diagn.Caret(duplicateField.defNameSpan, false),
                            new diagn.Caret(nodeIdentifier.span));

                    var fieldType = TypeResolver.Resolve(ctx, reporter, nodeField.type);

                    structureDef.fields.Add(new sema.Def.Structure.Field
                    {
                        defSpan = nodeField.span,
                        defNameSpan = nodeIdentifier.span,
                        name = fieldName,
                        type = fieldType
                    });
                }
            }
        }
    }
}
