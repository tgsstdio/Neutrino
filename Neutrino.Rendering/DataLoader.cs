using glTFLoader.Schema;
using System;
using System.Collections.Generic;
using System.IO;

namespace Neutrino
{
    public class DataLoader
    {
        public MgtfModelDataFile LoadData(string baseDir, Gltf model)
        {
            return new MgtfModelDataFile
            {
                Buffers = ExtractBuffers(baseDir, model.Buffers),
                Images = ExtractImages(baseDir, model.Images, model.BufferViews),
            };
        }

        private static MgtfBuffer[] ExtractBuffers(string baseDir, glTFLoader.Schema.Buffer[] buffers)
        {
            var result = new List<MgtfBuffer>();
            foreach (var selectedBuffer in buffers)
            {
                if (selectedBuffer.Uri.StartsWith("data:") && DataURL.FromUri(selectedBuffer.Uri, out DataURL output))
                {
                    if (output.Data.Length != selectedBuffer.ByteLength)
                    {
                        throw new InvalidDataException(
                            string.Format(
                                "The specified length of embedded data chunk ({0}) is not equal to the actual length of the data chunk ({1}).",
                                selectedBuffer.ByteLength,
                                output.Data.Length)
                        );
                    }
                    result.Add(
                        new MgtfBuffer {
                            Data = output.Data,                            
                        }
                    );
                }
                else
                {
                    // OPEN FILE
                    string additionalFilePath = System.IO.Path.Combine(baseDir, selectedBuffer.Uri);
                    using (var fs = File.Open(additionalFilePath, FileMode.Open))
                    using (var br = new BinaryReader(fs))
                    {
                        // ONLY READ SPECIFIED CHUNK SIZE
                        result.Add(
                            new MgtfBuffer
                            {
                                Data = br.ReadBytes(selectedBuffer.ByteLength)
                            }
                        );
                    }
                }
            }
            return result.ToArray();
        }

        private static MgtfImageMimeType GetImageTypeFromStr(string userDefMimeType)
        {
            switch (userDefMimeType)
            {
                case "":
                default:
                    throw new InvalidOperationException("image type invalid");
                case ".PNG":
                case "image/png":
                    return MgtfImageMimeType.PNG;
                case ".JPG":
                case ".JPEG":
                case "image/jpeg":
                    return MgtfImageMimeType.JPEG;
            }
        }

        private static MgtfImageMimeType GetImageTypeFromJSON(Image.MimeTypeEnum mimeType)
        {
            switch (mimeType)
            {
                case Image.MimeTypeEnum.image_jpeg:
                    return MgtfImageMimeType.JPEG;
                case Image.MimeTypeEnum.image_png:
                    return MgtfImageMimeType.PNG;
                default:
                    throw new InvalidOperationException("image type invalid");
            }
        }

        private MgtfImage[] ExtractImages(string baseDir, Image[] images, BufferView[] bufferViews)
        {
            var count = images != null ? images.Length : 0;

            var output = new MgtfImage[count];

            for (var i = 0; i < count; i += 1)
            {
                var current = images[i];

                byte[] src = null;
                ulong srcOffset = 0;
                ulong srcLength = 0UL;
                int? bufferRef = null;
                string userDefMimeType = null;
                if (current.BufferView.HasValue)
                {
                    var view = bufferViews[current.BufferView.Value];
                    src = null;
                    bufferRef = view.Buffer;
                    srcOffset = (ulong)view.ByteOffset;
                    srcLength = (ulong)view.ByteLength;
                }
                else if (!string.IsNullOrWhiteSpace(current.Uri))
                {
                    if (DataURL.FromUri(current.Uri, out DataURL urlData))
                    {
                        userDefMimeType = urlData.MediaType;
                        src = urlData.Data;
                        srcOffset = 0UL;
                        srcLength = (ulong)urlData.Data.Length;
                    }
                    else
                    {
                        // OPEN FILE
                        userDefMimeType = Path.GetExtension(current.Uri).ToUpperInvariant();

                        string additionalFilePath = System.IO.Path.Combine(baseDir, current.Uri);
                        using (var fs = File.Open(additionalFilePath, FileMode.Open))
                        using (var ms = new MemoryStream())
                        {
                            fs.CopyTo(ms);
                            src = ms.ToArray();
                            srcOffset = 0UL;
                            srcLength = (ulong)src.Length;
                        }
                    }
                }

                MgtfImageMimeType mimeType =
                    current.MimeType.HasValue
                    ? GetImageTypeFromJSON(current.MimeType.Value)
                    : GetImageTypeFromStr(userDefMimeType);

                var temp = new MgtfImage
                {
                    Name = current.Name,
                    MimeType = mimeType,
                    Source = src,
                    Buffer = bufferRef,
                    SrcOffset = srcOffset,
                    SrcLength = srcLength,
                };

                output[i] = temp;
            }

            return output;
        }
    }
}
