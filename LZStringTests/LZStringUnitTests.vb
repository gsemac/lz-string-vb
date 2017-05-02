Imports System.Text
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass()> Public Class LZStringUnitTests

    <TestMethod()> Public Sub testCompressToBase64()

        Assert.AreEqual("BIUwNmD2A0AEDukBOYAmBCIA", LZStringVB.LZString.compressToBase64("Hello, world!"))

    End Sub
    <TestMethod()> Public Sub testDecompressFromBase64()

        Assert.AreEqual("Hello, world!", LZStringVB.LZString.decompressfromBase64("BIUwNmD2A0AEDukBOYAmBCIA"))

    End Sub
    <TestMethod()> Public Sub testCompressToUTF16()

        Assert.AreEqual("ɢ䰭䰾恔@㯄ʓFȱ ", LZStringVB.LZString.compressToUTF16("Hello, world!"))

    End Sub
    <TestMethod()> Public Sub testDecompressFromUTF16()

        Assert.AreEqual("Hello, world!", LZStringVB.LZString.decompressfromUTF16("ɢ䰭䰾恔@㯄ʓFȱ "))

    End Sub
    <TestMethod()> Public Sub testcompressToUint8Array()

        Dim actual_bytes As Byte() = LZStringVB.LZString.compressToUint8Array("Hello, world!")
        Dim expected_bytes As Byte() = {4, 133, 48, 54, 96, 246, 3, 64, 4, 14, 233, 1, 57, 128, 38, 4, 34, 0}

        For i As Integer = 0 To expected_bytes.Length - 1
            Assert.AreEqual(expected_bytes(i), actual_bytes(i))
        Next

    End Sub
    <TestMethod()> Public Sub testDecompressFromUint8Array()

        Dim bytes As Byte() = {4, 133, 48, 54, 96, 246, 3, 64, 4, 14, 233, 1, 57, 128, 38, 4, 34, 0}

        Assert.AreEqual("Hello, world!", LZStringVB.LZString.decompressFromUint8Array(bytes))

    End Sub

End Class