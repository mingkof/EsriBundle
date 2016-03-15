using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace EsriBundle.IO
{
    public class EsriBundleHelper
    {
        public static void ComputeOrigin(string filename, out int startCol, out int startRow)
        {
            // 读取
            // R0000C0000
            filename = filename.ToUpper();
            int rIndex = filename.IndexOf('R');
            int cIndex = filename.LastIndexOf('C');
            int dotIndex = filename.LastIndexOf('.');
            if (dotIndex < 0) dotIndex = filename.Length;
            if (rIndex < 0 || cIndex <= 0 || dotIndex <= 0)
            {
                throw new Exception();
            }
            if (rIndex >= cIndex || cIndex >= dotIndex || rIndex >= dotIndex)
            {
                throw new Exception();
            }
            // 读取行R之后的数值，为十六进制的列值
            string hexStr = filename.Substring(rIndex + 1, cIndex - rIndex - 1);

            if (!int.TryParse(hexStr, NumberStyles.HexNumber, NumberFormatInfo.InvariantInfo, out startRow))
            {
                throw new Exception();

            }

            hexStr = filename.Substring(cIndex + 1, dotIndex - cIndex - 1);

            if (!int.TryParse(hexStr, NumberStyles.HexNumber, NumberFormatInfo.InvariantInfo, out startCol))
            {
                throw new Exception();

            }
        }

        /// <summary>
        /// 在本文件内的行列号，需要加上起始的行列号才可以作为真实的行列号
        /// </summary>
        /// <param name="index"></param>
        /// <param name="packetSize"></param>
        /// <param name="startCol"></param>
        /// <param name="startRow"></param>
        public static void ComputeOrigin(int index, int packetSize, out int col, out int row)
        {
            col = index / packetSize;
            row = index % packetSize;
        }
        /// <summary>
        /// 计算数据在所在文件中行列号
        /// </summary>
        /// <param name="row">row为全部的行列号</param>
        /// <param name="col">row为全部的行列号</param>
        /// <param name="packetSize"></param>
        /// <param name="index"></param>
        public static void ComputeIndex(int row,int col,  int packetSize, out int index)
        {
            int rGroup = packetSize * (row / packetSize);
            int cGroup = packetSize * (col / packetSize);
            //行列号是整个范围内的，在某个文件中需要先减去前面文件所占有的行列号（都是128的整数）这样就得到在文件中的真是行列号
            index = packetSize * (col - cGroup) + (row - rGroup);

        }
    }
}
