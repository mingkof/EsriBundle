using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EsriBundle.IO
{
    /// <summary>
    /// 
    /// </summary>
    public class TileIndex : IComparable, IComparer<TileIndex>
    {
        public const short SizeInbytes = 5;
        ///// <summary>
        ///// 列
        ///// </summary>
        //public int Col;

        ///// <summary>
        ///// 行
        ///// </summary>
        //public int Row;

        public int Number;

        /// <summary>
        /// 偏移地址
        /// </summary>
        public long Offset;

        public int Compare(TileIndex x, TileIndex y)
        {
            if (x.Number > y.Number)
            {
                return 1;
            }
            if (x.Number == y.Number)
            {
                return 0;
            }
            return -1;

        }

        public int CompareTo(object obj)
        {
            var other = obj as TileIndex;
            if (other == null) return -1;
            return Compare(this, other);
        }

        // 转换为二进制文件
        public byte[] ToBytes()
        {
            byte[] bytes = new byte[SizeInbytes];

            var numberbytes = BitConverter.GetBytes(Offset);
            for (int i = 0; i < SizeInbytes; i++)
            {
                bytes[i] = numberbytes[i];
            }
           
            return bytes;
        }
    }
}
