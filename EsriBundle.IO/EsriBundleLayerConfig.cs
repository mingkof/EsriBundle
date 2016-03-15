using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace EsriBundle.IO
{
    public class EsriBundleLayerConfig
    {
        #region 范围信息，存储在CDI文件中
       
        public decimal XMin;
        public decimal YMin;
        public decimal XMax;
        public decimal YMax;

        #endregion

        public string TileCacheInfo;

        public TileImageInfo TileImageInfo;

        public CacheStorageInfo CacheStorageInfo;
    }

    public class TileImageInfo
    {
        public string CacheTileFormat;

        public decimal CompressionQuality;

        public bool Antialiasing;
    }

    public class CacheStorageInfo
    {
        public string StorageFormat;

        public int PacketSize;
    }
}
