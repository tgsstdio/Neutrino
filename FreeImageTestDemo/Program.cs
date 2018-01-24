using System;
using System.IO;

namespace FreeImageTestDemo
{ 
    class Program
    {
        static void Main(string[] args)
        {
            IMgImageSourceExaminer examiner = new BitmapImageSourceExaminer();
            using (var fs = File.Open("Profile.jpg", FileMode.Open)) {
                var src = examiner.DetermineSource(fs);

                Console.WriteLine($"Format : {src.Format}");
                Console.WriteLine($"> Width: {src.Width}");
                Console.WriteLine($"> Height : {src.Height}");
                Console.WriteLine($"> Size: {src.Size}");
            }
        }

    }
}
