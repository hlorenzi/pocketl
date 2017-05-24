using System.Collections.Generic;


namespace pocketl.mod
{
    public class Package
    {
        public string name;
        public util.FileSystem filesystem;
        public List<H<Unit>> units = new List<H<Unit>>();
    }
}
