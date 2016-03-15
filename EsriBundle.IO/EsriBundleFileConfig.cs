using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EsriBundle.IO
{
    public class EsriBundleFileConfig
    {
        public const string BundleExt = ".bundle";
        public const string BundleIndexExt = ".bundlx";

        public const short HeaderSize = 60;


        public int PacketSize = 128;

        public const short MaxTileCount = 16384;

        /// <summary>
        /// 所有非Null的瓦片数量
        /// </summary>
        public int NotNullTileCount;

        /// <summary>
        /// 最大瓦片的直接
        /// </summary>
        public int MaxTileSize=0;

        public int StartCol;
        public int EndCol;
        public int StartRow;
        public int EndRow;
    }
}
