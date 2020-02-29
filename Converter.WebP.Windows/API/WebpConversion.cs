﻿using System;
using System.IO;
using System.Threading;
using Squirrel_Sizer;

namespace Converter.WebP.Windows.API {
    internal class WebpConversion {
        private ThreadStart ConvertThreadStart { get; set; }
        private Thread ConvertThread { get; set; }
        internal DroppedImage DroppedImage { get; set; }
        internal Guid ImageGuid { get; set; }
        private string Image { get; set; }

        internal void StartConvert() {
            Image = $"{DroppedImage.Location}\\{DroppedImage.Name}{DroppedImage.Type}";
            ConvertThreadStart = Convert;
            ConvertThreadStart += () => {
                Reference.MainDispatcher.Invoke(() => {
                    foreach (DroppedImage image in Reference.ListView.ItemsSource) {
                        if (image.ImageGuid != ImageGuid) continue;
                        FileInfo info = new FileInfo($"{image.Location}\\{image.Name}.webp");
                        image.ConvertedSize = Sizer.SizeSuffix(info.Length);
                        Reference.ConvertedSize += info.Length;
                        Reference.ConvertedSizeLabel.Content = $"Converted Size: {Sizer.SizeSuffix(Reference.ConvertedSize)}";
                        Reference.ListView.Items.Refresh();
                    }
                }, System.Windows.Threading.DispatcherPriority.Background);
            };
            ConvertThread = new Thread(ConvertThreadStart);
            ConvertThread.Start();
        }

        /// <summary>
        /// Options go before the input image
        /// 
        /// cwebp flag -lossless default is true
        /// cwebp flag -q Compression Level(float) default is 80
        /// cwebp flag -noalpha default is false
        /// cwebp flag -metadata(string) either "all" or "none" default is none
        /// 
        /// gif2webp compression default is 75 not 80
        /// gif2webp cannot use lossless flag
        /// gif2webp cannot use noalpha flag
        /// gif2webp cannot use metadata flag
        /// </summary>
        internal void Convert() {
            Reference.StartProcess();
            Reference.Process.StandardInput.WriteLine(DroppedImage.Type.ToLower() != ".gif"
                ? $"cwebp -mt \"{Image}\" -o \"{DroppedImage.Location}\\{DroppedImage.Name}.webp\""
                : $"gif2webp -mt \"{Image}\"  -o \"{DroppedImage.Location}\\{DroppedImage.Name}.webp\"");
            Reference.ExitProcess();
        }
    }
}
