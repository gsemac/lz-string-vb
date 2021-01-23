Imports System.Text
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass()>
Public Class LZStringTests

    ' Public members

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
    Public Sub TestCompressesAndDecompressesAllPrintableUTF16Characters()

        ' This unit test is based off of this unit test:
        ' https://github.com/pieroxy/lz-string/blob/b2e0b270a9f3cf330b778b777385fcba384a1a02/tests/lz-string-spec.js#L53

        Dim sb As New StringBuilder

        For i As Integer = 32 To 127 - 1
            sb.Append(Char.ConvertFromUtf32(i))
        Next

        For i As Integer = 128 + 32 To 55296 - 1
            sb.Append(Char.ConvertFromUtf32(i))
        Next

        For i As Integer = 63744 To 65536 - 1
            sb.Append(Char.ConvertFromUtf32(i))
        Next

        Dim testString As String = sb.ToString()
        Dim compressed As String = LZString.Compress(testString)
        Dim decompressed As String = LZString.Decompress(compressed)

        Assert.AreNotEqual(testString, compressed)
        Assert.AreEqual(testString, decompressed)

    End Sub
    <TestMethod()>
    Public Sub TestCompressesAndDecompressesStringWithRepeatingCharacters()

        ' This unit test is based off of this unit test:
        ' https://github.com/pieroxy/lz-string/blob/b2e0b270a9f3cf330b778b777385fcba384a1a02/tests/lz-string-spec.js#L72

        Dim testString As String = "aaaaabaaaaacaaaaadaaaaaeaaaaa"
        Dim compressed As String = LZString.Compress(testString)
        Dim decompressed As String = LZString.Decompress(compressed)

        Assert.AreNotEqual(testString, compressed)
        Assert.IsTrue(compressed.Length < testString.Length)
        Assert.AreEqual(testString, decompressed)

    End Sub
    <TestMethod()>
    Public Sub TestCompressesAndDecompressesALongString()

        ' This unit test is based off of this unit test:
        ' https://github.com/pieroxy/lz-string/blob/b2e0b270a9f3cf330b778b777385fcba384a1a02/tests/lz-string-spec.js#L82

        Dim testString As String = GetRandomTestString()
        Dim compressed As String = LZString.Compress(testString)
        Dim decompressed As String = LZString.Decompress(compressed)

        Assert.AreNotEqual(testString, compressed)
        Assert.IsTrue(compressed.Length < testString.Length)
        Assert.AreEqual(testString, decompressed)

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

    <TestMethod()>
    Public Sub TestCompressToEncodedURIComponentAllCharactersAreURISafe()

        ' This unit test is based off of this unit test:
        ' https://github.com/pieroxy/lz-string/blob/b2e0b270a9f3cf330b778b777385fcba384a1a02/tests/lz-string-spec.js#L131

        Dim testString As String = GetRandomTestString()
        Dim compressed As String = LZString.CompressToEncodedURIComponent(testString)
        Dim decompressed As String = LZString.DecompressFromEncodedURIComponent(compressed)

        Assert.AreEqual(-1, compressed.IndexOf("="c))
        Assert.AreEqual(-1, compressed.IndexOf("/"c))
        Assert.AreEqual(testString, decompressed)

    End Sub
    <TestMethod()>
    Public Sub TestCompressToEncodedURIComponentPlusAndSpaceAreInterchangeable()

        ' This unit test is based off of this unit test:
        ' https://github.com/pieroxy/lz-string/blob/b2e0b270a9f3cf330b778b777385fcba384a1a02/tests/lz-string-spec.js#L144

        Dim testString As String = GetTestString()
        Dim compressed As String = "CIVwTglgdg5gBAFwIYIQezdGAaO0DWeAznlAFYCmAxghQCanqIAWFcR 0u0ECEKWOEih4AtqJBQ2YCkQAOaKEQq5hDKhQA2mklSTb6cAESikVMGjnMkMWUbii0ANzbQmCVkJlIhUBkYoUOBA5ew9XKHwAOjgAFU9Tc0trW10kMDAAT3Y0UTY0ADMWCMJ3TwAjNDpMgHISTUzRKzgoKtlccpAEHLyWIPS2AogDBgB3XmZSQiJkbLku3ApRcvo6Q2hi9k4oGPiUOrhR627TfFlN5FQMOCcIIghyzTZJNbBNjmgY4H1mNBB7tgAVQgLjA9wQtRIAEEnlQ4AAxfRnKDWUTEOBrFyaSyCHzoOQQPSaODmQJojxBUZoMD4EjlbLIMC2PiwTaJCxWGznCndawuOAyUzQQxBcLsXj5Ipiy7oNAxAByFFGDjMHJS50c-I2TCoiiIIF6YrkMlufyIDTgBJgeSgCAAtEMRiqkpzUr4GOERKIIDAwCg2GU2A0mpNWmsiIsXLaQPoLchtvBY5tqmxxh5iqIYkYAOqsES6prpQS8RBoOCaJDKMB28qVwwy66C5z6bgiI6EyaZP7sCgBirgJS4MVEPQZLBDiqaO60MGtlh3El13CjCg1fnhn1SBg OhgEDwHkYtCyKA1brebTZPlsCRUSaFAp2xnMuAUAoFagIbD2TxEJAQOgs2zVcZBaNBumfCgWUTKBskKTZWjAUxiQ fMtB0XAiDLLsQEORQzx7NgfGxbp4OgAoK3EARFBiABJEQCjML84FrZQGEUTZjTQDQiBIQ8VxqUCmJjS9gnuWBlzYOh8Ig5gCGKUDxm0FiiNg0gKKQKi A4-plLUPBuipEBNG3GgRItFZfD4O1yMo0x0CyKIgAAA$$"
        Dim decompressed As String = LZString.DecompressFromEncodedURIComponent(compressed)

        Assert.AreEqual(testString, decompressed)

    End Sub

    ' Private members

    Private Function GetTestString() As String

        Return "During tattooing, ink is injected into the skin, initiating an immune response, and cells called ""macrophages"" move into the area and ""eat up"" the ink. The macrophages carry some of the ink to the body's lymph nodes, but some that are filled with ink stay put, embedded in the skin. That's what makes the tattoo visible under the skin. Dalhousie Uiversity's Alec Falkenham is developing a topical cream that works by targeting the macrophages that have remained at the site of the tattoo. New macrophages move in to consume the previously pigment-filled macrophages and then migrate to the lymph nodes, eventually taking all the dye with them. ""When comparing it to laser-based tattoo removal, in which you see the burns, the scarring, the blisters, in this case, we've designed a drug that doesn't really have much off-target effect,"" he said. ""We're not targeting any of the normal skin cells, so you won't see a lot of inflammation. In fact, based on the process that we're actually using, we don't think there will be any inflammation at all and it would actually be anti-inflammatory."

    End Function
    Private Function GetRandomTestString() As String

        Dim sb As New StringBuilder
        Dim random As New Random

        For i As Integer = 32 To 1000 - 1

            sb.Append(random.NextDouble())
            sb.Append(" "c)

        Next

        Return sb.ToString()

    End Function

End Class