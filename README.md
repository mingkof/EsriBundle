ArcGIS 10之后增加紧凑型文件格式，用于瓦片的缓存。
EsriBundle用于C#下对该格式进行读写，核心库为EsriBundle.IO。
支持单个bundle文件的读写（EsriBundleFile类实现）
支持使用行列号和层级的读写，可以不考虑瓦片在哪个bundle文件（EsriBundleLayer类实现）
主要参考
http://www.cnblogs.com/yuantf/p/3320876.html
http://blog.csdn.net/warrenwyf/article/details/6069711
https://gdbgeek.wordpress.com/2012/08/09/demystifying-the-esri-compact-cache/
