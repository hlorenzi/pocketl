using System;
using System.Collections.Generic;


namespace pocketl.sema
{
    public class Code
    {
        public class Body
        {
            public int parameterCount;
            public List<Register> registers = new List<Register>();
            public List<Segment> segments = new List<Segment>();


            public int CreateSegment()
            {
                var segment = new Segment { terminator = new Terminator.Return() };
                this.segments.Add(segment);
                return this.segments.Count - 1;
            }


            public void AddInstruction(int segmentIndex, Instruction instr)
            {
                this.segments[segmentIndex].instrs.Add(instr);
            }


            public void PrintDebug(util.Output output, Context ctx)
            {
                for (var i = 0; i < this.registers.Count; i++)
                {
                    var reg = this.registers[i];
                    output.Write("r#" + i);

                    if (reg.name != null)
                        output.Write(" `" + reg.name + "`");

                    output.Write(": ");
                    output.WriteLine(reg.type.PrintableName(ctx));
                }

                if (this.registers.Count > 0 && this.segments.Count > 0)
                    output.WriteLine();

                for (var i = 0; i < this.segments.Count; i++)
                {
                    output.WriteLine("s#" + i + ":");
                    this.segments[i].PrintDebug(output.Indented, ctx);
                    output.WriteLine();
                }
            }
        }


        public class Register
        {
            public diagn.Span spanDef;
            public diagn.Span spanDefName;
            public string name;
            public Type type;
        }


        public class Segment
        {
            public List<Instruction> instrs = new List<Instruction>();
            public Terminator terminator;

            
            public void PrintDebug(util.Output output, Context ctx)
            {
                foreach (var instr in this.instrs)
                {
                    instr.PrintDebug(output, ctx);
                    output.WriteLine();
                }

                terminator.PrintDebug(output, ctx);
                output.WriteLine();
            }
        }


        public abstract class Terminator
        {
            public virtual void PrintDebug(util.Output output, Context ctx)
            {

            }


            public class Return : Terminator
            {
                public override void PrintDebug(util.Output output, Context ctx)
                {
                    output.Write("return");
                }
            }


            public class Goto : Terminator
            {
                public int segmentIndex;


                public override void PrintDebug(util.Output output, Context ctx)
                {
                    output.Write("goto s#" + segmentIndex);
                }
            }


            public class Branch : Terminator
            {
                public int conditionRegisterIndex;
                public int trueSegmentIndex;
                public int falseSegmentIndex;


                public override void PrintDebug(util.Output output, Context ctx)
                {
                    output.Write("branch r#" + conditionRegisterIndex);
                    output.Write(" ? s#" + trueSegmentIndex);
                    output.Write(" : s#" + falseSegmentIndex);
                }
            }
        }


        public class PrimitiveNumber
        {
            public enum Type
            {
                Int8, Int16, Int32, Int64,
                UInt8, UInt16, UInt32, UInt64,
                Float32, Float64
            }

            public ulong unsignedValue;
            public long signedValue;
            public double floatValue;
        }


        public abstract class Instruction
        {
            public diagn.Span span;
            public Lvalue destination;


            public virtual void PrintDebug(util.Output output, Context ctx)
            {

            }


            protected void PrintDebugExcerpt(util.Output output, Context ctx)
            {
                output.AlignColumn(40);
                output.Write("# " + this.span.Excerpt(ctx));
            }


            public class CopyLiteralNumber : Instruction
            {
                public PrimitiveNumber number;
            }


            public class CopyLiteralBool : Instruction
            {
                public bool value;
            }


            public class CopyLiteralStructure : Instruction
            {
                public H<Def> hDef;
                public List<int> sourceFields = new List<int>();
            }


            public class CopyLiteralTuple : Instruction
            {
                public List<int> sources = new List<int>();


                public override void PrintDebug(util.Output output, Context ctx)
                {
                    this.destination.PrintDebug(output, ctx);
                    output.Write(" = (");
                    for (var i = 0; i < this.sources.Count; i++)
                    {
                        if (i > 0)
                            output.Write(", ");

                        output.Write("r#" + this.sources[i]);
                    }
                    output.Write(")");
                    this.PrintDebugExcerpt(output, ctx);
                }
            }


            public class CopyFunction : Instruction
            {
                public H<Def> hDef;
            }


            public class CopyCallResult : Instruction
            {
                public int sourceTarget;
                public List<int> sourceArguments = new List<int>();
            }


            public class CopyLvalue : Instruction
            {
                public Lvalue source;


                public override void PrintDebug(util.Output output, Context ctx)
                {
                    this.destination.PrintDebug(output, ctx);
                    output.Write(" = ");
                    this.source.PrintDebug(output, ctx);
                    this.PrintDebugExcerpt(output, ctx);
                }
            }
        }


        public abstract class Lvalue
        {
            public diagn.Span span;


            public virtual void PrintDebug(util.Output output, Context ctx)
            {

            }


            public class Error : Lvalue
            {
                public override void PrintDebug(util.Output output, Context ctx)
                {
                    output.Write("<error>");
                }
            }


            public class Discard : Lvalue
            {
                public override void PrintDebug(util.Output output, Context ctx)
                {
                    output.Write("_");
                }
            }


            public class Register : Lvalue
            {
                public int index;


                public override void PrintDebug(util.Output output, Context ctx)
                {
                    output.Write("r#" + index);
                }
            }
        }
    }
}
