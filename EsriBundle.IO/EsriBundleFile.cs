using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;

namespace EsriBundle.IO
{
    /// <summary>
    /// Esri紧凑文件单个文件
    /// </summary>
    public class EsriBundleFile
    {
        protected bool IsIntiliazed;
        public readonly string FilePath;
        protected Stream TileDataStream;

        public readonly bool NewFileMode;
        /// <summary>
        /// 索引记录
        /// </summary>
        private Dictionary<int, TileIndex> IndicesDict;
        private IList<TileIndex> AllIndices;

        private Dictionary<int, byte[]> TilesBytesDict;
        private bool _IsIntiliazed;

        public EsriBundleFileConfig Config { get; private set; }


        public EsriBundleFile(string filePath, IList<TileIndex> allIndices)
            : this(filePath)
        {
            AllIndices = allIndices;
        }
        public EsriBundleFile(string filePath)
        {
            FilePath = filePath;
            NewFileMode = false;
            Config = new EsriBundleFileConfig();
            Config.PacketSize = 128;
            //尝试从文件名字中读取开始行列号
            try
            {
                EsriBundleHelper.ComputeOrigin(Path.GetFileName(filePath), out Config.StartCol, out Config.StartRow);
            }
            catch (Exception)
            {


            }
        }
        public EsriBundleFile(string filePath, EsriBundleFileConfig config)
            : this(filePath)
        {
            Config = config;
        }

        public EsriBundleFile(EsriBundleFileConfig config = null)
        {
            NewFileMode = true;
            if (config == null)
            {
                Config = new EsriBundleFileConfig();
                Config.PacketSize = 128;
            }
            else
            {
                Config = config;
            }
        }

        public void Intiliaze()
        {
            if (IsIntiliazed) return;
            if (NewFileMode)
            {
                TilesBytesDict = new Dictionary<int, byte[]>(EsriBundleFileConfig.MaxTileCount);
                AllIndices = new List<TileIndex>();
                IsIntiliazed = true;
                return;
            }

            TileDataStream = new FileStream(FilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            ReadHeader(TileDataStream);
            if (AllIndices != null)
            {
                IntilizeIndicesDict();
            }
            else
            {
                var indexFilePath = Path.ChangeExtension(FilePath, EsriBundleFileConfig.BundleIndexExt);
                using (EsriBundleIndexFile indefile = new EsriBundleIndexFile(indexFilePath))
                {
                    indefile.Intiliaze();
                    AllIndices = indefile.GetAllIndices();
                }
                IntilizeIndicesDict();
            }
            IsIntiliazed = true;
        }


      

        /// <summary>
        /// 转换存储索引为字典，方便快速查找
        /// </summary>
        private void IntilizeIndicesDict()
        {
            IndicesDict = new Dictionary<int, TileIndex>(EsriBundleFileConfig.MaxTileCount);
            foreach (var storageIndex in AllIndices)
            {
                if (!IndicesDict.ContainsKey(storageIndex.Number))
                {
                    IndicesDict.Add(storageIndex.Number, storageIndex);
                }
                else
                {
                    IndicesDict[storageIndex.Number] = storageIndex;
                }
            }
        }


        public byte[] Read(int index)
        {
            TileIndex storageIndex = null;
            if (!IndicesDict.TryGetValue(index, out storageIndex))
            {
                throw new NullReferenceException("不存在此文件");
            }

            var offset = storageIndex.Offset;
            TileDataStream.Seek(offset, SeekOrigin.Begin);
            // 读取4字节，作为实际数据
            byte[] datalengthBytes = new byte[4];
            TileDataStream.Read(datalengthBytes, 0, 4);
            var datalength = BitConverter.ToInt32(datalengthBytes, 0);
            if (datalength == 0) return null;
            // 读取实际数据
            byte[] dateBytes = new byte[datalength];
            TileDataStream.Read(dateBytes, 0, dateBytes.Length);
            return dateBytes;
        }

        public void Write(int index, byte[] dataBytes)
        {
            if (!NewFileMode) throw new NotSupportedException("非NewFileMode不支持写入");
            if (dataBytes == null || dataBytes.Length == 0) return;
            if (!TilesBytesDict.ContainsKey(index))
            {
                TilesBytesDict.Add(index, dataBytes);
            }
            else
            {
                TilesBytesDict[index] = dataBytes;
            }
        }

        public void SaveAs(string newDataPath)
        {
            newDataPath = Path.ChangeExtension(newDataPath, EsriBundleFileConfig.BundleExt);
            if (File.Exists(newDataPath)) File.Delete(newDataPath);
            Dictionary<int, byte[]> tilesBytesDict;
            if (!NewFileMode)
            {
                tilesBytesDict = new Dictionary<int, byte[]>();
                for (int i = 0; i < EsriBundleFileConfig.MaxTileCount; i++)
                {
                    var dataBytes = Read(i);
                    if (dataBytes == null || dataBytes.Length == 0) continue;
                    tilesBytesDict.Add(i, dataBytes);
                }
            }
            else
            {
                tilesBytesDict = TilesBytesDict;

            }
            NewFileModeSaveAs(newDataPath, tilesBytesDict);
        }


        public void NewFileModeSaveAs(string newDataPath, Dictionary<int, byte[]> tilesBytesDict)
        {
            Config.NotNullTileCount = tilesBytesDict.Count;
            Config.MaxTileSize = 0;
            IList<TileIndex> allIndices = new List<TileIndex>();
            using (Stream newDataStream = new FileStream(newDataPath, FileMode.CreateNew))
            {
                // 写入所有数据的空数据位置，每个4字节，空数据按其索引指向此位置
                newDataStream.Seek(EsriBundleFileConfig.HeaderSize, SeekOrigin.Begin);
                newDataStream.Write(new byte[EsriBundleFileConfig.MaxTileCount * 4], 0, EsriBundleFileConfig.MaxTileCount * 4);
               
                for (int i = 0; i < EsriBundleFileConfig.MaxTileCount; i++)
                {
                    byte[] dataBytes = null;
                    tilesBytesDict.TryGetValue(i, out dataBytes);

                    // 数据为空,只写入4个字节的长度，表示0
                    if (dataBytes == null)
                    {
                        //计算索引位置，位于空数据位置
                        allIndices.Add(new TileIndex()
                        {
                            Number = i,
                            Offset = EsriBundleFileConfig.HeaderSize + 4 * i
                        });
                    }
                    else
                    {
                        //生成有数据的文件索引
                        allIndices.Add(new TileIndex()
                        {
                            Number = i,
                            Offset = newDataStream.Position
                        });

                        //写入数据的大小和数据本身
                        newDataStream.Write(BitConverter.GetBytes(dataBytes.Length), 0, 4);
                        newDataStream.Write(dataBytes, 0, dataBytes.Length);
                        if (Config.MaxTileSize < dataBytes.Length)
                        {
                            Config.MaxTileSize = dataBytes.Length;
                        }
                    }
                }
                WriteHeaderBytes(newDataStream);
            }
            //同时生成索引文件
            EsriBundleIndexFile indexFile = new EsriBundleIndexFile(allIndices);
            indexFile.Intiliaze();
            string newIndexPath = Path.ChangeExtension(newDataPath, EsriBundleFileConfig.BundleIndexExt);
            indexFile.SaveAs(newIndexPath);
            indexFile.Dispose();
            indexFile = null;
        }

  

        #region 读文件头

        private void ReadHeader(Stream dataStream)
        {
            // 读取8-11位，表示第最大瓦片的大小
            dataStream.Seek(8, SeekOrigin.Begin);
            byte[] tempBytes = new byte[4];
            dataStream.Read(tempBytes, 0, 4);
            Config.MaxTileSize = BitConverter.ToInt32(tempBytes, 0);

            // 读取非Null瓦片数量
            dataStream.Seek(16, SeekOrigin.Begin);
            dataStream.Read(tempBytes, 0, 4);
            Config.NotNullTileCount = BitConverter.ToInt32(tempBytes, 0) / 4;

            // 读取文件头内存储的行列号信息
            ReadColAndRowExtent(dataStream, Config);

        }

        /// <summary>
        /// 读取文件头行列号
        /// </summary>
        /// <param name="dataStream"></param>
        private void ReadColAndRowExtent(Stream dataStream, EsriBundleFileConfig config)
        {
            dataStream.Seek(44, SeekOrigin.Begin);
            // 44-47为开始行
            byte[] tempBytes = new byte[4];
            dataStream.Read(tempBytes, 0, 4);
            config.StartRow = BitConverter.ToInt32(tempBytes, 0);
            // 48-51为结束行
            dataStream.Read(tempBytes, 0, 4);
            config.EndRow = BitConverter.ToInt32(tempBytes, 0);

            // 52-55为开始列
            dataStream.Read(tempBytes, 0, 4);
            config.StartCol = BitConverter.ToInt32(tempBytes, 0);
            // 56-59为结束列
            dataStream.Read(tempBytes, 0, 4);
            config.EndCol = BitConverter.ToInt32(tempBytes, 0);
        }


        #endregion


        #region 写入文件头
        private void WriteHeaderBytes(Stream newDataStream)
        {
            WriteFixedHeaderBytes(newDataStream);

            //写入16-19位，表示非空文件个数*4
            newDataStream.Seek(16, SeekOrigin.Begin);
            byte[] countBytes = BitConverter.GetBytes(Config.NotNullTileCount * 4);
            newDataStream.Write(countBytes, 0, 4);
            int maxDataSize = -1;

            //写入行列范围
            WriteColAndRowExtent(newDataStream);

            // 写入8-11位，表示第最大瓦片的大小
            newDataStream.Seek(8, SeekOrigin.Begin);
            newDataStream.Write(BitConverter.GetBytes(Config.MaxTileSize), 0, 4);

            // 向写入头部的整个的文件大小,24-27字节
            newDataStream.Seek(24, SeekOrigin.Begin);
            newDataStream.Write(BitConverter.GetBytes(newDataStream.Length), 0, 4);

        }
        private void WriteFixedHeaderBytes(Stream dataStream)
        {
            // 写入最开始的8个字节，固定值
            dataStream.Seek(0, SeekOrigin.Begin);
            dataStream.Write(new byte[8] { 0x03, 0x00, 0x00, 0x00, 0x00, 0x40, 0x00, 0x00 }, 0, 8);


            // 写入12-15字节的固定值
            dataStream.Seek(12, SeekOrigin.Begin);
            dataStream.Write(new byte[4] { 0x05, 0x00, 0x00, 0x00 }, 0, 4);

            // 写入32-43字节的固定值
            dataStream.Seek(32, SeekOrigin.Begin);
            dataStream.Write(new byte[] { 0x28, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x10, 0x00, 0x00, 0x00 }, 0, 12);
        }

        private void WriteColAndRowExtent(Stream dataStream)
        {
            dataStream.Seek(44, SeekOrigin.Begin);
            // 44-47为开始行
            byte[] tempBytes = BitConverter.GetBytes(Config.StartRow);
            dataStream.Write(tempBytes, 0, 4);

            // 48-51为结束行
            tempBytes = BitConverter.GetBytes(Config.EndRow);
            dataStream.Write(tempBytes, 0, 4);


            // 52-55为开始列
            tempBytes = BitConverter.GetBytes(Config.StartCol);
            dataStream.Write(tempBytes, 0, 4);
            // 56-59为结束列
            tempBytes = BitConverter.GetBytes(Config.EndCol);
            dataStream.Write(tempBytes, 0, 4);
        }

        #endregion


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (!disposing) return;
            if (TileDataStream != null) TileDataStream.Dispose();
        }

    }
}
