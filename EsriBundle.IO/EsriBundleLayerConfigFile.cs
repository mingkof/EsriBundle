using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace EsriBundle.IO
{
    public class EsriBundleLayerConfigFile
    {
        public readonly string XmlFilePath;
        public readonly string CdiFilePath;
        public readonly bool NewFileMode = false;
        protected bool IsIntiliazed;
        private XmlDocument xmlFileDoc;
        private XmlDocument cdiFileReader;
        public EsriBundleLayerConfigFile(string xmlFilePath)
        {
            XmlFilePath = xmlFilePath;
            CdiFilePath = Path.ChangeExtension(xmlFilePath, ".cdi");
            NewFileMode = false;
        }

        public void Intiliaze()
        {
            if (IsIntiliazed) return;

            xmlFileDoc = new XmlDocument();
            xmlFileDoc.Load(XmlFilePath);
            cdiFileReader = new XmlDocument();
            cdiFileReader.Load(CdiFilePath);

            IsIntiliazed = true;
        }

        public EsriBundleLayerConfig Read()
        {
            var config = new EsriBundleLayerConfig();
            var tileCacheNode = xmlFileDoc.SelectSingleNode("/CacheInfo/TileCacheInfo");
            if (tileCacheNode != null) config.TileCacheInfo = tileCacheNode.InnerXml;

            var tileImageNode = xmlFileDoc.SelectSingleNode("/CacheInfo/TileImageInfo");
            if (tileImageNode != null)
            {
                config.TileImageInfo = ReadTileImageInfo(tileImageNode);
            }

            return config;
        }

        private TileImageInfo ReadTileImageInfo(XmlNode node)
        {
            TileImageInfo imgInfo = new TileImageInfo();
            var tempNode = node.SelectSingleNode("CacheTileFormat");
            if (tempNode != null) imgInfo.CacheTileFormat = tempNode.InnerText;

            tempNode = node.SelectSingleNode("CompressionQuality");
            if (tempNode != null)
            {
                int cq;
                int.TryParse(tempNode.InnerText, out cq);
                if (cq >= 0 && cq <= 100)
                    imgInfo.CompressionQuality = cq;
            }

            tempNode = node.SelectSingleNode("Antialiasing");
            if (tempNode != null)
            {
                imgInfo.Antialiasing = tempNode.InnerText.ToUpper() == "TRUE";
            }

            return imgInfo;
        }


    }
}
