using System;


namespace pocketl.diagn
{
    public class Span
    {
        public H<mod.Unit> unit;
        public int start, end;


        public Span(H<mod.Unit> unit, int start, int end)
        {
            this.unit = unit;
            this.start = start;
            this.end = end;
        }


        public int Length
        {
            get { return this.end - this.start; }
        }


        public Span JustBefore
        {
            get { return new Span(this.unit, this.start, this.start); }
        }


        public Span JustAfter
        {
            get { return new Span(this.unit, this.end, this.end); }
        }


        public string Excerpt(Context ctx)
        {
            return ctx[unit].ReadSource(ctx).Substring(this.start, this.Length);
        }


        public string CompressedExcerpt(Context ctx, int maxLength = 0)
        {
            var excerpt = this.Excerpt(ctx);
            var compressed = "";
            var skip = false;
            var length = 0;

            foreach (var c in excerpt)
            {
                if (maxLength != 0 && length >= maxLength)
                {
                    compressed += "...";
                    break;
                }

                if (c == ' ' || c == '\t' || c == '\r' || c == '\n')
                {
                    if (!skip)
                    {
                        skip = true;
                        compressed += " ";
                        length++;
                    }
                }
                else
                {
                    skip = false;
                    compressed += c;
                    length++;
                }
            }

            return compressed;
        }


        public static Span operator +(Span a, Span b)
        {
            if (a == null)
                return new Span(b.unit, b.start, b.end);

            if (b == null)
                return new Span(a.unit, a.start, a.end);

            if (a.unit?.id != b.unit?.id)
                throw new Exception("spans point to different units");

            return new Span(
                a.unit,
                Math.Min(a.start, b.start),
                Math.Max(a.end, b.end));
        }
    }
}
