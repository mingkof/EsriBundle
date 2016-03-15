using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EsriBundle.IO
{
    /// <summary>
    /// Esri紧凑文件图层，包含多个.bundle、.bundlx的文件
    /// </summary>
    public class EsriBundleLayer
    {
        /// <summary>
        /// 包大小
        /// </summary>
        public readonly int PacketSize;

        /// <summary>
        /// 图层文件存储的位置，不包含_alllayers
        /// </summary>
        public readonly string LayerPath;


        public EsriBundleLayer(string layerPath, int tileSize = 128)
        {
            PacketSize = tileSize;
            LayerPath = System.IO.Path.Combine(layerPath, "_alllayers");

        }

        /// <summary>
        /// 读取指定行列号的数据
        /// </summary>
        /// <param name="level"></param>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        public Byte[] Read(int level, int row, int col)
        {
            string bundleFilePath = GetBundleFileWithouExtention(level, row, col);

            // 打开对应的索引文件
            EsriBundleIndexFile indexFile = new EsriBundleIndexFile(bundleFilePath + EsriBundleFileConfig.BundleIndexExt);
            indexFile.Intiliaze();

            // 读取所有索引
            var bundleIndices = indexFile.GetAllIndices();
            // 读取数据文件
            EsriBundleFile dataFile = new EsriBundleFile(bundleFilePath + EsriBundleFileConfig.BundleExt, bundleIndices);
            dataFile.Intiliaze();
            int index;
            EsriBundleHelper.ComputeIndex(row, col, PacketSize, out index);

            var tileBytes = dataFile.Read(index);
            indexFile.Dispose();
            dataFile.Dispose();
            return tileBytes;
        }

        /// <summary>
        /// 写入指定行列号的瓦片数据，由于需要频繁打开文件然后复制，最好使用批量写入的方式
        /// </summary>
        /// <param name="level"></param>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="tilebytes"></param>
        public void Write(int level, int row, int col, Byte[] tilebytes)
        {
            string bundleFilePath = GetBundleFileWithouExtention(level, row, col);

            // 将旧文件的数据首先读到新文件中,插入新的数据会造成整个文件数据的移动，因而这里采用全部读取出来，然后重新写入的方式
            EsriBundleFile oldDataFile = new EsriBundleFile(bundleFilePath + EsriBundleFileConfig.BundleExt);
            oldDataFile.Intiliaze();
            var newDataFile = new EsriBundleFile(oldDataFile.Config);
            newDataFile.Intiliaze();
            for (int i = 0; i < EsriBundleFileConfig.MaxTileCount; i++)
            {
                var dataBytes = oldDataFile.Read(i);

                newDataFile.Write(i, dataBytes);
            }
            oldDataFile.Dispose();
            oldDataFile = null;
            int index;
            EsriBundleHelper.ComputeIndex(row, col, PacketSize, out index);

            newDataFile.Write(index, tilebytes);
            newDataFile.SaveAs(bundleFilePath + EsriBundleFileConfig.BundleExt);
            newDataFile.Dispose();

        }

        /// <summary>
        /// 批量写入瓦片，此方法会缓存新创建的文件
        /// </summary>
        /// <param name="tileData"></param>
        public void Write(params TileData[] tileData)
        {
            Write(new List<TileData>(tileData));
        }

        /// <summary>
        /// 批量写入瓦片，此方法会缓存新创建的文件
        /// </summary>
        /// <param name="tileDatalist"></param>
        public void Write(IEnumerable<TileData> tileDatalist)
        {
            /// 已经打开的文件字典，避免频繁打开文件
            Dictionary<string, EsriBundleFile> newFileDict = new Dictionary<string, EsriBundleFile>();
            
            foreach (var tileData in tileDatalist)
            {
                int level = tileData.Level;
                int row = tileData.Row;
                int col = tileData.Col;
                string bundleFilePath = GetBundleFileWithouExtention(level, row, col);

                // 尝试打开文件
                EsriBundleFile newDataFile;
                // 查看是否在已经打开的文件里
                if (newFileDict.ContainsKey(bundleFilePath))
                {
                    newDataFile = newFileDict[bundleFilePath];
                }
                else
                {
                    // 将旧文件的数据首先读到新文件中,插入新的数据会造成整个文件数据的移动，因而这里采用全部读取出来，然后重新写入的方式
                    EsriBundleFile oldDataFile = new EsriBundleFile(bundleFilePath + EsriBundleFileConfig.BundleExt);
                    oldDataFile.Intiliaze();
                    newDataFile = new EsriBundleFile(oldDataFile.Config);
                    newDataFile.Intiliaze();
                    for (int i = 0; i < EsriBundleFileConfig.MaxTileCount; i++)
                    {
                        var dataBytes = oldDataFile.Read(i);

                        newDataFile.Write(i, dataBytes);
                    }
                    oldDataFile.Dispose();
                    oldDataFile = null;
                    newFileDict.Add(bundleFilePath, newDataFile);
                }
                // 替换旧数据
                int index;
                EsriBundleHelper.ComputeIndex(row, col, PacketSize, out index);
                newDataFile.Write(index, tileData.Data);
            }
            // 依次保存数据
            foreach (var newfilePair in newFileDict)
            {
                var newfile = newfilePair.Value;
                newfile.SaveAs(newfilePair.Key + EsriBundleFileConfig.BundleExt);
                newfile.Dispose();
                newfile = null;
            }
        }



        private string GetBundleFileWithouExtention(int level, int row, int col)
        {
            string levelDir = "0" + level;

            int lLength = levelDir.Length;

            if (lLength > 2)
            {

                levelDir = levelDir.Substring(lLength - 2);

            }

            levelDir = "L" + levelDir;



            int rowGroup = PacketSize * (row / PacketSize);

            string r = "000" + rowGroup.ToString("X");

            int rLength = r.Length;

            if (rLength > 4)
            {

                r = r.Substring(rLength - 4);

            }

            r = "R" + r;

            int cGroup = PacketSize * (col / PacketSize);

            string c = "000" + cGroup.ToString("X");

            int cLength = c.Length;

            if (cLength > 4)
            {

                c = c.Substring(cLength - 4);

            }

            c = "C" + c;

            string bundleFile = Path.Combine(LayerPath, levelDir);
            bundleFile = Path.Combine(bundleFile, r + c);
            return bundleFile;
        }

    }
}
