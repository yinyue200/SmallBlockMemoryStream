using System;
using System.IO;
using System.Linq;
using BenchmarkDotNet.Attributes;
using AutoFixture;

namespace SmallBlockMemoryStream.Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            var result = BenchmarkDotNet.Running.BenchmarkRunner.Run<BasicTest>();
        }
    }
    [MemoryDiagnoser]
    public class BasicTest
    {
        MemoryStream memoryStream = new MemoryStream();
        Aethon.IO.SmallBlockMemoryStream smallBlockMemoryStream = new Aethon.IO.SmallBlockMemoryStream();
        Random random = new Random();
        AutoFixture.Fixture fixture = new AutoFixture.Fixture();
        [Benchmark]
        public void Write()
        {
            write(memoryStream);
        }
        [Benchmark]
        public void AethonWrite()
        {
            write(smallBlockMemoryStream);
        }
        private void write(Stream stream)
        {
            stream.SetLength(0);
            var content = new byte[random.Next(0, 850000)];
            random.NextBytes(content);
            stream.Write(content, 0, content.Length);
            stream.WriteByte(unchecked((byte)random.Next()));
        }
        MemoryStream memoryStream2 = new MemoryStream();
        Aethon.IO.SmallBlockMemoryStream smallBlockMemoryStream2 = new Aethon.IO.SmallBlockMemoryStream();
        [GlobalSetup]
        public void setupread()
        {
            writedata(memoryStream2);
            writedata(smallBlockMemoryStream2);
        }
        private void writedata(Stream stream)
        {
            var text = new StreamWriter(stream);
            for (int i = 0; i < 10000; i++)
            {
                text.WriteLine(fixture.Create<string>());
            }
        }
        private void read(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            var text = new StreamReader(stream);
            text.ReadLine();

        }
        private void readall(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            var text = new StreamReader(stream);
            text.ReadToEnd();
        }
        [Benchmark]
        public void Read()
        {
            read(memoryStream2);
        }
        [Benchmark]
        public void AethonRead()
        {
            read(smallBlockMemoryStream);
        }
        [Benchmark]
        public void Readall()
        {
            readall(memoryStream2);
        }
        [Benchmark]
        public void AethonReadall()
        {
            readall(smallBlockMemoryStream);
        }
    }
}
