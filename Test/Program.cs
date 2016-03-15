using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using EsriBundle.IO;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
           var tiledata= File.ReadAllBytes(@"D:\esri\wmts.png");

            EsriBundleLayer layer = new EsriBundleLayer(@"D:\esri\SDPubMap_Vec\Layers");
          
            layer.Write(new TileData(){Level=1,Row= 0, Col=1,Data=tiledata});

            //TestlayerConfig();
            //int startCol1, startRow1;
            //int startCol2, startRow2;
            //EsriBundleHelper.ComputeOrigin(13459,128, out startCol1, out startRow1);
            //EsriBundleHelper.ComputeOrigin(13330, 128, out startCol2, out startRow2);
            //Console.WriteLine();
            //TestSaveAs();
            //CreateDataFileTest();
            int startCol, startRow;
            EsriBundleHelper.ComputeOrigin("R0100C0600", out startCol, out startRow);

            //ReadTest();
            //BatchCompareIndexfiles();



            //var oldfilepath = @"d:\esri\R30a00C25200.bundlx";
            //var newfilepath = @"d:\esri\new\R30a00C25200.bundlx";
            //oldfilepath = @"d:\esri\R0000C0000.bundlx";
            //newfilepath = @"d:\esri\newR0000C0000.bundlx";

            //CreateIndexFile(oldfilepath, newfilepath);
            //Compare(@"D:\esri\R1300C6980.bundlx", @"D:\esri\R1300C6980new.bundlx");
            //OutputDataFileTest();
            BatchTestSaveas();
        }

        static void BatchTestSaveas()
        {
            var odlFolder = @"C:\arcgisserver\directories\arcgiscache\TestChinaMap\图层\_alllayers11111";
            var newFolder = @"C:\arcgisserver\directories\arcgiscache\TestChinaMap\图层\_alllayersnew";

            var allOldFiles = Directory.GetFiles(odlFolder, "*.bundle", SearchOption.AllDirectories);
            foreach (var oldFilePath in allOldFiles)
            {

                EsriBundleFile oldDataFile = new EsriBundleFile(oldFilePath);
                oldDataFile.Intiliaze();
                string filename = Path.GetFileName(oldFilePath);
                string newDataSavepath = oldFilePath.Replace(odlFolder, newFolder).Replace(filename, "");

                if (!Directory.Exists(newDataSavepath)) Directory.CreateDirectory(newDataSavepath);
                oldDataFile.SaveAs(Path.Combine(newDataSavepath, filename));
            }

        }

        static void TestlayerConfig()
        {
            var xmlPath = @"D:\esri\SDPubMap_Vec\Layers\conf.xml";
            EsriBundleLayerConfigFile configfile = new EsriBundleLayerConfigFile(xmlPath);
            configfile.Intiliaze();
            configfile.Read();
        }
        static void ReadTest()
        {
            var oldIndexPath = @"d:\esri\R0000C0000.bundlx";
            var oldDataPath = @"d:\esri\R0000C0000.bundle";


            EsriBundleIndexFile oldIndexFile = new EsriBundleIndexFile(oldIndexPath);
            oldIndexFile.Intiliaze();

            var allOldStorageIndices = oldIndexFile.GetAllIndices();
            EsriBundleFile oldDataFile = new EsriBundleFile(oldDataPath, allOldStorageIndices);
            oldDataFile.Intiliaze();
            for (int i = 0; i < 16384; i++)
            {
                var dataBytes = oldDataFile.Read(i);
                if (dataBytes != null)
                {
                    var indexx = allOldStorageIndices[i];
                }
            }
        }

        static void TestSaveAs()
        {
            var oldIndexPath = @"d:\esri\R1300C6980.bundlx";
            var oldDataPath = @"d:\esri\R1300C6980.bundle";
            var newDataPath = @"d:\esri\R1300C6980new.bundle";

            EsriBundleIndexFile oldIndexFile = new EsriBundleIndexFile(oldIndexPath);
            oldIndexFile.Intiliaze();

            var allOldStorageIndices = oldIndexFile.GetAllIndices();
            EsriBundleFile oldDataFile = new EsriBundleFile(oldDataPath, allOldStorageIndices);
            oldDataFile.Intiliaze();
            oldDataFile.SaveAs(newDataPath);

        }
        static void CreateDataFileTest()
        {
            var oldIndexPath = @"d:\esri\R1300C6980.bundlx";
            var oldDataPath = @"d:\esri\R1300C6980.bundle";
            var newDataPath = @"d:\esri\R1300C6980new.bundle";

            EsriBundleIndexFile oldIndexFile = new EsriBundleIndexFile(oldIndexPath);
            oldIndexFile.Intiliaze();

            var allOldStorageIndices = oldIndexFile.GetAllIndices();
            EsriBundleFile oldDataFile = new EsriBundleFile(oldDataPath, allOldStorageIndices);
            oldDataFile.Intiliaze();
            EsriBundleFile newDataFile = new EsriBundleFile(oldDataFile.Config);
            newDataFile.Intiliaze();
            for (int i = 0; i < 16384; i++)
            {
                var dataBytes = oldDataFile.Read(i);

                newDataFile.Write(i, dataBytes);
            }
            newDataFile.SaveAs(newDataPath);
        }

        static void OutputDataFileTest()
        {
            var oldIndexPath = @"d:\esri\R1300C6980new.bundlx";
            var oldDataPath = @"d:\esri\R1300C6980new.bundle";


            //EsriBundleIndexFile oldIndexFile = new EsriBundleIndexFile(oldIndexPath);
            //oldIndexFile.Intiliaze();


            EsriBundleFile oldDataFile = new EsriBundleFile(oldDataPath);
            oldDataFile.Intiliaze();
            for (int i = 0; i < 16384; i++)
            {
                var dataBytes = oldDataFile.Read(i);
                if (dataBytes != null)
                    File.WriteAllBytes(Path.Combine(@"d:\esri\R1300C6980new\", i + ".jpg"), dataBytes);
            }

        }

        static void BatchCompareIndexfiles()
        {
            var oldfileFolder = @"d:\esri\testBundle\";
            var newfileFolder = @"d:\esri\testBundle.new\";

            string[] oldfiles = Directory.GetFiles(oldfileFolder);
            foreach (var oldfilepath in oldfiles)
            {

                var newfilepath = Path.Combine(newfileFolder, Path.GetFileName(oldfilepath));

                CreateIndexFile(oldfilepath, newfilepath);
                Compare(oldfilepath, newfilepath);
            }

        }

        static void CreateIndexFile(string oldfilepath, string newfilepath)
        {
            EsriBundleIndexFile indexfile = new EsriBundleIndexFile(oldfilepath);
            indexfile.Intiliaze();
            //indexfile.SaveAs(newfilepath);

            EsriBundleIndexFile indexfile1 = new EsriBundleIndexFile(indexfile.GetAllIndices());
            indexfile1.Intiliaze();
            indexfile1.SaveAs(newfilepath);
            indexfile1.Dispose();
            indexfile.Dispose();

        }

        static void Compare(string file1, string file2)
        {

            byte[] file1Bytes = File.ReadAllBytes(file1);
            byte[] file2Bytes = File.ReadAllBytes(file2);
            if (file1Bytes.Length != file2Bytes.Length)
            {
                return;
            }
            for (int i = 0; i < file1Bytes.Length; i++)
            {
                if (file1Bytes[i] != file2Bytes[i])
                {
                    Console.WriteLine(i);
                }
            }
        }
    }
}
