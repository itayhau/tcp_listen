using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Diagnostics.Runtime;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.GraphViewerGdi;
using System.Net.Sockets;
using System.IO;
using System.IO.Compression;

namespace ConsoleApp5
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(HttpRequestAsync().Result);
            Console.ReadLine();
        }

        private static async Task<string> HttpRequestAsync()
        {
            string result = string.Empty;

            using (var tcp = new TcpClient("jsonplaceholder.typicode.com", 80))
            using (var stream = tcp.GetStream())
            {
                tcp.SendTimeout = 500;
                tcp.ReceiveTimeout = 1000;
                // Send request headers
                var builder = new StringBuilder();
                builder.AppendLine("GET /posts HTTP/1.1");
                builder.AppendLine("Host: jsonplaceholder.typicode.com");
                //builder.AppendLine("Content-Length: " + data.Length);   // only for POST request
                builder.AppendLine("Connection: close");
                builder.AppendLine();
                var header = Encoding.ASCII.GetBytes(builder.ToString());
                await stream.WriteAsync(header, 0, header.Length);

                // Send payload data if you are POST request
                //await stream.WriteAsync(data, 0, data.Length);

                // receive data
                using (var memory = new MemoryStream())
                {
                    await stream.CopyToAsync(memory);
                    memory.Position = 0;
                    var data = memory.ToArray();

                    var index = BinaryMatch(data, Encoding.ASCII.GetBytes("\r\n\r\n")) + 4;
                    var headers = Encoding.ASCII.GetString(data, 0, index);
                    memory.Position = index;

                    if (headers.IndexOf("Content-Encoding: gzip") > 0)
                    {
                        using (GZipStream decompressionStream = new GZipStream(memory, CompressionMode.Decompress))
                        using (var decompressedMemory = new MemoryStream())
                        {
                            decompressionStream.CopyTo(decompressedMemory);
                            decompressedMemory.Position = 0;
                            result = Encoding.UTF8.GetString(decompressedMemory.ToArray());
                        }
                    }
                    else
                    {
                        result = Encoding.UTF8.GetString(data, index, data.Length - index);
                        //result = Encoding.GetEncoding("gbk").GetString(data, index, data.Length - index);
                    }
                }

                //Debug.WriteLine(result);
                return result;
            }
        }

        private static int BinaryMatch(byte[] input, byte[] pattern)
        {
            int sLen = input.Length - pattern.Length + 1;
            for (int i = 0; i < sLen; ++i)
            {
                bool match = true;
                for (int j = 0; j < pattern.Length; ++j)
                {
                    if (input[i + j] != pattern[j])
                    {
                        match = false;
                        break;
                    }
                }
                if (match)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
