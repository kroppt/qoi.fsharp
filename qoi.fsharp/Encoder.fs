namespace Qoi.Fsharp

module Encoder =
    open System.IO

    type public Channels =
        | Rgb = 3
        | Rgba = 4

    type public ColorSpace =
        | SRgb = 0
        | Linear = 1

    type public Encoder =
        private new(binWriter: BinaryWriter,
                    input: byte list,
                    width: int,
                    height: int,
                    channels: Channels,
                    colorSpace: ColorSpace) =
            { binWriter = binWriter
              input = input
              width = width
              height = height
              channels = channels
              colorSpace = colorSpace }

        val binWriter: BinaryWriter
        val input: byte list
        val width: int
        val height: int
        val channels: Channels
        val colorSpace: ColorSpace

        static member public Encode
            (
                input: byte list,
                width: int,
                height: int,
                channels: Channels,
                colorSpace: ColorSpace
            ) : byte [] =
            using (new MemoryStream()) (fun memStream ->
                using (new BinaryWriter(memStream)) (fun binWriter ->
                    let encoder = new Encoder(binWriter, input, width, height, channels, colorSpace)
                    encoder.Encode())

                memStream.ToArray())

        member private this.writeBigEndian(value: int) =
            this.binWriter.Write(byte ((value >>> 24) &&& 0xFF))
            this.binWriter.Write(byte ((value >>> 16) &&& 0xFF))
            this.binWriter.Write(byte ((value >>> 8) &&& 0xFF))
            this.binWriter.Write(byte ((value >>> 0) &&& 0xFF))

        member private this.WriteHeader() =
            this.binWriter.Write(byte 'q')
            this.binWriter.Write(byte 'o')
            this.binWriter.Write(byte 'i')
            this.binWriter.Write(byte 'f')
            this.writeBigEndian (this.width)
            this.writeBigEndian (this.height)
            this.binWriter.Write(byte this.channels)
            this.binWriter.Write(byte this.colorSpace)

        member private this.WriteFooter() =
            this.binWriter.Write(byte 0)
            this.binWriter.Write(byte 0)
            this.binWriter.Write(byte 0)
            this.binWriter.Write(byte 0)
            this.binWriter.Write(byte 0)
            this.binWriter.Write(byte 0)
            this.binWriter.Write(byte 0)
            this.binWriter.Write(byte 1)

        member private this.Encode() =
            this.WriteHeader()
            this.WriteFooter()
