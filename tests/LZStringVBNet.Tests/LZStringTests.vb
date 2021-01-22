Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass()>
Public Class LZStringTests

    <TestMethod()>
    Public Sub TestCompress()

        Assert.AreEqual($"օ〶惶@✰ӈ{Char.ConvertFromUtf32(0)}", LZString.Compress("hello world"))

    End Sub
    <TestMethod()>
    Public Sub TestDecompress()

        Assert.AreEqual("hello world", LZString.Decompress($"օ〶惶@✰ӈ{Char.ConvertFromUtf32(0)}"))

    End Sub
    <TestMethod()>
    Public Sub TestCompressWithNull()

        Assert.AreEqual("", LZString.Compress(Nothing))

    End Sub
    <TestMethod()>
    Public Sub TestDecompressWithNull()

        Assert.AreEqual("", LZString.Decompress(Nothing))

    End Sub
    <TestMethod()>
    Public Sub TestCompressWithEmptyString()

        Assert.AreEqual("䀀", LZString.Compress(""))

    End Sub
    <TestMethod()>
    Public Sub TestDecompressWithEmptyString()

        Assert.AreEqual("", LZString.Decompress("䀀"))

    End Sub
    <TestMethod()>
    Public Sub TestCompressToBase64()

        Assert.AreEqual("BIUwNmD2A0AEDukBOYAmBCIA", LZString.CompressToBase64("Hello, world!"))

    End Sub
    <TestMethod()>
    Public Sub TestDecompressFromBase64()

        Assert.AreEqual("Hello, world!", LZString.DecompressFromBase64("BIUwNmD2A0AEDukBOYAmBCIA"))

    End Sub
    <TestMethod()>
    Public Sub TestCompressToUTF16()

        Assert.AreEqual("ɢ䰭䰾恔@㯄ʓFȱ ", LZString.CompressToUTF16("Hello, world!"))

    End Sub
    <TestMethod()>
    Public Sub TestDecompressFromUTF16()

        Assert.AreEqual("Hello, world!", LZString.DecompressFromUTF16("ɢ䰭䰾恔@㯄ʓFȱ "))

    End Sub
    <TestMethod()>
    Public Sub TestCompressToUint8Array()

        Dim expectedBytes As Byte() = {4, 133, 48, 54, 96, 246, 3, 64, 4, 14, 233, 1, 57, 128, 38, 4, 34, 0}
        Dim actualBytes As Byte() = LZString.CompressToUInt8Array("Hello, world!")

        Assert.IsTrue(expectedBytes.SequenceEqual(actualBytes))

    End Sub
    <TestMethod()>
    Public Sub TestDecompressFromUint8Array()

        Dim bytes As Byte() = {4, 133, 48, 54, 96, 246, 3, 64, 4, 14, 233, 1, 57, 128, 38, 4, 34, 0}

        Assert.AreEqual("Hello, world!", LZString.DecompressFromUInt8Array(bytes))

    End Sub

End Class