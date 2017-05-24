using System;


namespace pocketl.diagn
{
    public class Span
    {
        public H<mod.Unit>? unit;
        public uint start, end;


        public Span(H<mod.Unit>? unit, uint start, uint end)
        {
            this.unit = unit;
            this.start = start;
            this.end = end;
        }


        public static Span operator +(Span a, Span b)
        {
            if (a.unit?.id != b.unit?.id)
                throw new Exception("spans point to different units");

            return new Span(
                a.unit,
                Math.Min(a.start, b.start),
                Math.Max(a.end, b.end));
        }
    }
}
