using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EsriBundle.IO
{
    public class EsriTileFormat
    {
        public const string PNG8 = "PNG8";
        public const string PNG24 = "PNG24";
        public const string PNG32 = "PNG32";
        public const string JPEG = "JPEG";

        public static IList<string> AllFormate()
        {
            return new List<string>() { PNG8, PNG24, PNG32, JPEG };
        }
    }
}
