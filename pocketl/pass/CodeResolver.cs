using System.Collections.Generic;


namespace pocketl.pass
{
    public class CodeResolver
    {
        public static void ResolveFunctionBody(Context ctx, diagn.Reporter reporter, sema.Code.Body body, syn.Node node)
        {
            var resolver = new CodeResolver(ctx, reporter, body);

            var segment = body.CreateSegment();
            var destination = new sema.Code.Lvalue.Register { index = 0 };

            resolver.ResolveExpr(ref segment, destination, node);
        }


        private Context ctx;
        private diagn.Reporter reporter;
        private sema.Code.Body code;
        private List<bool> registersInScope = new List<bool>();
        private Stack<int> breakSegments = new Stack<int>();
        private Stack<int> continueSegments = new Stack<int>();


        private CodeResolver(Context ctx, diagn.Reporter reporter, sema.Code.Body code)
        {
            this.ctx = ctx;
            this.reporter = reporter;
            this.code = code;

            for (var i = 0; i < code.registers.Count; i++)
                this.registersInScope.Add(true);
        }


        private void ResolveExpr(ref int segment, sema.Code.Lvalue destination, syn.Node node)
        {
            var nodeParen = node as syn.Node.Parenthesized;
            var nodeBlock = node as syn.Node.Block;
            var nodeLet = node as syn.Node.Let;
            var nodeIf = node as syn.Node.If;
            var nodeWhile = node as syn.Node.While;
            var nodeLoop = node as syn.Node.Loop;
            var nodeBreak = node as syn.Node.Break;
            var nodeContinue = node as syn.Node.Continue;
            var nodeReturn = node as syn.Node.Return;
            var nodeIdentifier = node as syn.Node.Identifier;
            var nodeNumber = node as syn.Node.Number;
            var nodeBool = node as syn.Node.LiteralBool;
            var nodeTuple = node as syn.Node.LiteralTuple;
            var nodeBinaryOp = node as syn.Node.BinaryOperation;

            if (nodeParen != null)
                this.ResolveExpr(ref segment, destination, nodeParen.inner);
            
            else if (nodeBlock != null)
                this.ResolveExprBlock(ref segment, destination, nodeBlock);

            else if (nodeLet != null)
                this.ResolveExprLet(ref segment, destination, nodeLet);

            else if (nodeIf != null)
                this.ResolveExprIf(ref segment, destination, nodeIf);

            else if (nodeWhile != null)
                this.ResolveExprWhile(ref segment, destination, nodeWhile);

            else if (nodeLoop != null)
                this.ResolveExprLoop(ref segment, destination, nodeLoop);

            else if (nodeBreak != null)
                this.ResolveExprBreak(ref segment, destination, nodeBreak);

            else if (nodeContinue != null)
                this.ResolveExprContinue(ref segment, destination, nodeContinue);

            else if (nodeReturn != null)
                this.ResolveExprReturn(ref segment, destination, nodeReturn);

            else if (nodeIdentifier != null)
                this.ResolveExprName(ref segment, destination, nodeIdentifier);

            else if (nodeNumber != null)
                this.ResolveExprNumber(ref segment, destination, nodeNumber);

            else if (nodeBool != null)
                this.ResolveExprBool(ref segment, destination, nodeBool);

            else if (nodeTuple != null)
                this.ResolveExprTuple(ref segment, destination, nodeTuple);

            else if (nodeBinaryOp != null)
                this.ResolveExprBinaryOp(ref segment, destination, nodeBinaryOp);

            else
                this.reporter.InternalError("unimplemented", new diagn.Caret(node.span));
        }


        private sema.Code.Lvalue ResolveLvalue(ref int segment, syn.Node node)
        {
            var nodeIdentifier = node as syn.Node.Identifier;
            if (nodeIdentifier != null)
                return this.ResolveLvalueName(ref segment, nodeIdentifier);

            else
            {
                this.reporter.InternalError("unimplemented", new diagn.Caret(node.span));
                return new sema.Code.Lvalue.Discard { span = node.span };
            }
        }


        private void ResolveExprBlock(ref int segment, sema.Code.Lvalue destination, syn.Node.Block node)
        {
            if (node.exprs.Count == 0)
            {
                this.AddVoidInstruction(segment, destination, node.span);
                return;
            }

            var scopeIndexBefore = this.registersInScope.Count;

            for (var i = 0; i < node.exprs.Count; i++)
            {
                var exprDestination = destination;
                if (i < node.exprs.Count - 1)
                    exprDestination = new sema.Code.Lvalue.Discard();

                this.ResolveExpr(ref segment, exprDestination, node.exprs[i]);
            }

            for (var i = scopeIndexBefore; i < this.registersInScope.Count; i++)
                this.registersInScope[i] = false;
        }


        private void ResolveExprLet(ref int segment, sema.Code.Lvalue destination, syn.Node.Let node)
        {
            var registerIndex = this.CreateRegister();
            var register = this.code.registers[registerIndex];

            register.spanDef = node.span;
            register.spanDefName = node.identifier.span;
            register.name = (node.identifier as syn.Node.Identifier).token.excerpt;

            if (node.type != null)
                register.type = TypeResolver.Resolve(this.ctx, this.reporter, node.type);
            else
                register.type = new sema.Type.Placeholder();

            if (node.expr != null)
            {
                var assignDestination = new sema.Code.Lvalue.Register
                {
                    span = node.identifier.span,
                    index = registerIndex
                };

                this.ResolveExpr(ref segment, assignDestination, node.expr);
            }

            this.AddVoidInstruction(segment, destination, node.span);
        }


        private void ResolveExprIf(ref int segment, sema.Code.Lvalue destination, syn.Node.If node)
        {
            var conditionRegIndex = this.ResolveIntoNewRegister(
                ref segment, node.condition, new sema.Type.Structure { def = ctx.primitiveBool });

            var conditionLvalue = new sema.Code.Lvalue.Register { index = conditionRegIndex, span = node.condition.span };
            this.ResolveExpr(ref segment, conditionLvalue, node.condition);

            var branch = new sema.Code.Terminator.Branch { conditionRegisterIndex = conditionRegIndex };
            this.code.SetTerminator(segment, branch);

            var trueSegment = this.code.CreateSegment();
            branch.trueSegmentIndex = trueSegment;
            this.ResolveExpr(ref trueSegment, new sema.Code.Lvalue.Discard { span = node.trueBlock.span }, node.trueBlock);

            if (node.falseBlock != null)
            {
                var falseSegment = this.code.CreateSegment();
                branch.falseSegmentIndex = falseSegment;
                this.ResolveExpr(ref falseSegment, new sema.Code.Lvalue.Discard { span = node.falseBlock.span }, node.falseBlock);
                var afterSegment = this.code.CreateSegment();
                this.code.SetTerminator(trueSegment, new sema.Code.Terminator.Goto { segmentIndex = afterSegment });
                this.code.SetTerminator(falseSegment, new sema.Code.Terminator.Goto { segmentIndex = afterSegment });
                segment = afterSegment;
            }
            else
            {
                var afterSegment = this.code.CreateSegment();
                branch.falseSegmentIndex = afterSegment;
                this.code.SetTerminator(trueSegment, new sema.Code.Terminator.Goto { segmentIndex = afterSegment });
                segment = afterSegment;
            }

            this.AddVoidInstruction(segment, destination, node.span);
        }


        private void ResolveExprWhile(ref int segment, sema.Code.Lvalue destination, syn.Node.While node)
        {
            var conditionSegment = this.code.CreateSegment();
            var conditionSegmentEnd = conditionSegment;

            this.code.SetTerminator(segment, new sema.Code.Terminator.Goto { segmentIndex = conditionSegment });

            var conditionRegIndex = this.ResolveIntoNewRegister(
                ref conditionSegmentEnd, node.condition, new sema.Type.Structure { def = ctx.primitiveBool });

            var conditionLvalue = new sema.Code.Lvalue.Register { index = conditionRegIndex, span = node.condition.span };
            this.ResolveExpr(ref conditionSegmentEnd, conditionLvalue, node.condition);

            var branch = new sema.Code.Terminator.Branch { conditionRegisterIndex = conditionRegIndex };
            this.code.SetTerminator(conditionSegmentEnd, branch);

            var bodySegment = this.code.CreateSegment();
            var afterSegment = this.code.CreateSegment();
            branch.trueSegmentIndex = bodySegment;

            this.breakSegments.Push(afterSegment);
            this.continueSegments.Push(conditionSegment);

            this.ResolveExpr(ref bodySegment, new sema.Code.Lvalue.Discard { span = node.block.span }, node.block);
            this.code.SetTerminator(bodySegment, new sema.Code.Terminator.Goto { segmentIndex = conditionSegment });

            this.breakSegments.Pop();
            this.continueSegments.Pop();

            branch.falseSegmentIndex = afterSegment;
            segment = afterSegment;

            this.AddVoidInstruction(segment, destination, node.span);
        }


        private void ResolveExprLoop(ref int segment, sema.Code.Lvalue destination, syn.Node.Loop node)
        {
            var bodySegment = this.code.CreateSegment();
            var bodySegmentEnd = bodySegment;

            var afterSegment = this.code.CreateSegment();
            segment = afterSegment;

            this.code.SetTerminator(segment, new sema.Code.Terminator.Goto { segmentIndex = bodySegment });

            this.breakSegments.Push(afterSegment);
            this.continueSegments.Push(bodySegment);

            this.ResolveExpr(ref bodySegmentEnd, new sema.Code.Lvalue.Discard { span = node.block.span }, node.block);
            this.code.SetTerminator(bodySegmentEnd, new sema.Code.Terminator.Goto { segmentIndex = bodySegment });

            this.breakSegments.Pop();
            this.continueSegments.Pop();

            this.AddVoidInstruction(segment, destination, node.span);
        }


        private void ResolveExprBreak(ref int segment, sema.Code.Lvalue destination, syn.Node.Break node)
        {
            if (this.breakSegments.Count == 0)
            {
                this.reporter.Error("`break` not inside a loop", new diagn.Caret(node.span));
                this.code.SetTerminator(segment, new sema.Code.Terminator.Error { });
            }
            else
            {
                this.code.SetTerminator(segment, new sema.Code.Terminator.Goto { segmentIndex = this.breakSegments.Peek() });

                segment = this.code.CreateSegment();
                this.AddVoidInstruction(segment, destination, node.span);
            }
        }


        private void ResolveExprContinue(ref int segment, sema.Code.Lvalue destination, syn.Node.Continue node)
        {
            if (this.continueSegments.Count == 0)
            {
                this.reporter.Error("`continue` not inside a loop", new diagn.Caret(node.span));
                this.code.SetTerminator(segment, new sema.Code.Terminator.Error { });
            }
            else
            {
                this.code.SetTerminator(segment, new sema.Code.Terminator.Goto { segmentIndex = this.continueSegments.Peek() });

                segment = this.code.CreateSegment();
                this.AddVoidInstruction(segment, destination, node.span);
            }
        }


        private void ResolveExprReturn(ref int segment, sema.Code.Lvalue destination, syn.Node.Return node)
        {
            if (node.expr != null)
                this.ResolveExpr(ref segment, new sema.Code.Lvalue.Register { index = 0 }, node.expr);
            else
                this.AddVoidInstruction(segment, new sema.Code.Lvalue.Register { index = 0 }, node.span);

            this.code.SetTerminator(segment, new sema.Code.Terminator.Return { });

            segment = this.code.CreateSegment();
            this.AddVoidInstruction(segment, destination, node.span);
        }


        private void ResolveExprName(ref int segment, sema.Code.Lvalue destination, syn.Node.Identifier node)
        {
            this.code.AddInstruction(segment, new sema.Code.Instruction.CopyLvalue
            {
                span = node.span,
                destination = destination,
                source = this.ResolveLvalue(ref segment, node)
            });
        }


        private void ResolveExprNumber(ref int segment, sema.Code.Lvalue destination, syn.Node.Number node)
        {
            this.code.AddInstruction(segment, new sema.Code.Instruction.CopyLiteralNumber
            {
                span = node.span,
                destination = destination,
                excerpt = node.token.excerpt
            });
        }


        private void ResolveExprBool(ref int segment, sema.Code.Lvalue destination, syn.Node.LiteralBool node)
        {
            this.code.AddInstruction(segment, new sema.Code.Instruction.CopyLiteralBool
            {
                span = node.span,
                destination = destination,
                value = node.value
            });
        }


        private void ResolveExprTuple(ref int segment, sema.Code.Lvalue destination, syn.Node.LiteralTuple node)
        {
            var elems = new List<int>();
            foreach (var elem in node.elems)
                elems.Add(this.ResolveIntoNewRegister(ref segment, elem));

            this.code.AddInstruction(segment, new sema.Code.Instruction.CopyLiteralTuple
            {
                span = node.span,
                destination = destination,
                sources = elems
            });
        }


        private void ResolveExprBinaryOp(ref int segment, sema.Code.Lvalue destination, syn.Node.BinaryOperation node)
        {
            if (node.op == syn.Node.BinaryOperator.Assign)
            {
                var assignDestination = this.ResolveLvalue(ref segment, node.lhs);
                this.ResolveExpr(ref segment, assignDestination, node.rhs);
                this.AddVoidInstruction(segment, destination, node.span);
            }
            else
                this.reporter.InternalError("unimplemented", new diagn.Caret(node.span));
        }


        private sema.Code.Lvalue ResolveLvalueName(ref int segment, syn.Node.Identifier node)
        {
            var registerIndex = this.FindRegisterByName(node.token.excerpt);
            if (registerIndex >= 0)
                return new sema.Code.Lvalue.Register { span = node.span, index = registerIndex };

            else
            {
                this.reporter.Error("unknown `" + node.token.excerpt + "`", new diagn.Caret(node.span));
                return new sema.Code.Lvalue.Error { span = node.span };
            }
        }


        private void AddVoidInstruction(int segment, sema.Code.Lvalue destination, diagn.Span span)
        {
            if (destination is sema.Code.Lvalue.Discard)
                return;

            this.code.AddInstruction(segment, new sema.Code.Instruction.CopyLiteralTuple
            {
                destination = destination,
                span = span
            });
        }


        private int CreateRegister()
        {
            var newRegisterIndex = this.code.registers.Count;
            this.registersInScope.Add(true);
            this.code.registers.Add(new sema.Code.Register());
            return newRegisterIndex;
        }


        private int ResolveIntoNewRegister(ref int segment, syn.Node node, sema.Type type = null)
        {
            var newRegisterIndex = this.code.registers.Count;
            this.registersInScope.Add(true);
            this.code.registers.Add(new sema.Code.Register
            {
                spanDef = node.span,
                type = type ?? new sema.Type.Placeholder()
            });

            var destination = new sema.Code.Lvalue.Register { span = node.span, index = newRegisterIndex };
            this.ResolveExpr(ref segment, destination, node);
            return newRegisterIndex;
        }


        private int FindRegisterByName(string name)
        {
            for (var i = this.code.registers.Count - 1; i >= 0; i--)
            {
                if (!this.registersInScope[i])
                    continue;

                if (this.code.registers[i].name != name)
                    continue;

                return i;
            }

            return -1;
        }
    }
}
