using System;
using System.Collections.Generic;
using System.IO;


namespace EsriBundle.IO
{
    public class EsriBundleIndexFile : IDisposable
    {

        private bool IsIntiliazed = false;

        public int StartCol;
        public int StartRow;
        /// <summary>
        /// 头部16个字节
        /// </summary>
        public static readonly byte[] HeaderBytes = new byte[16] { 0x03, 0x00, 0x00, 0x00, 0x10, 0x00, 0x00, 0x00, 0x00, 0x40, 0x00, 0x00, 0x05, 0x00, 0x00, 0x00 };

        /// <summary>
        /// 尾部16个字节
        /// </summary>
        public static readonly byte[] FooterBytes = new byte[16] { 0x00, 0x00, 0x00, 0x00, 0x10, 0x00, 0x00, 0x00, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

        public readonly string FilePath;
        protected Stream IndexStream;


        private List<TileIndex> AllIndices;

        /// <summary>
        /// 最多128*128个索引文件
        /// </summary>
        public const short IndexCount = 16384;

        /// <summary>
        /// 每个索引的字节大小
        /// </summary>
  

        public readonly bool NewFileMode;

        public EsriBundleIndexFile(string filePath)
        {
            FilePath = filePath;
            NewFileMode = false;
        }

        public EsriBundleIndexFile(ICollection<TileIndex> allIndices)
        {
            if (allIndices == null || allIndices.Count != IndexCount)
            {
                throw new ArgumentException("数组个数只能为" + IndexCount);
            }
            AllIndices = new List<TileIndex>(allIndices);
            NewFileMode = true;
        }

        public void Intiliaze()
        {
            if (IsIntiliazed) return;
            if (!NewFileMode)
            {
                AllIndices = new List<TileIndex>(IndexCount);
                IndexStream = new FileStream(FilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            }

            InternalReadAllIndices();
            IsIntiliazed = true;
        }

        public IList<TileIndex> GetAllIndices()
        {
            return AllIndices;
        }

        public TileIndex Get(short index)
        {
            return AllIndices[index];
        }

        protected void InternalReadAllIndices()
        {
            if (!NewFileMode)
            {
                for (short i = 0; i < IndexCount; i++)
                {
                    AllIndices.Add(InternalRead(i));
                }
            }
            else
            {
                AllIndices.Sort();
            }
        }

        private TileIndex InternalRead(short index)
        {
            if (index < 0 || index > IndexCount) throw new IndexOutOfRangeException(string.Format("index在0~{0}之间", IndexCount));

            // 定位到索引位置
            IndexStream.Seek(16 + TileIndex.SizeInbytes * index, SeekOrigin.Begin);
            var indexBytes = new byte[TileIndex.SizeInbytes];
            IndexStream.Read(indexBytes, 0, TileIndex.SizeInbytes);

            //转换为long型
            long offset =
                     (indexBytes[0] & 0xff) +
                     (indexBytes[1] & 0xff) * 256 + (indexBytes[2] & 0xff) * 65536 +
                     (indexBytes[3] & 0xff) * 16777216 + (indexBytes[4] & 0xff) * 4294967296L;

            
            TileIndex tileIndex = new TileIndex();
            tileIndex.Number = index;
            // 瓦片在主文件中的偏移地址
            tileIndex.Offset = offset;
            return tileIndex;
        }

        /// <summary>
        /// 保存为新文件
        /// </summary>
        /// <param name="newPath"></param>
        public void SaveAs(string newPath)
        {
            if (File.Exists(newPath)) File.Delete(newPath);
            
            using (Stream newIndexStream = new FileStream(newPath, FileMode.CreateNew))
            {
                // 写入文件头
                newIndexStream.Write(HeaderBytes, 0, 16);
                // 依次写入各个索引
                foreach (var tileIndex in AllIndices)
                {
                    var tempBytes = tileIndex.ToBytes();
                    newIndexStream.Write(tempBytes, 0, 5);
                }

                newIndexStream.Write(FooterBytes, 0, 16);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (!disposing) return;
            if (IndexStream != null) IndexStream.Dispose();
        }
    }
}
