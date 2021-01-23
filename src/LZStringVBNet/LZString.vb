Public NotInheritable Class LZString

    ' Public members

    Public Shared Function CompressToBase64(input As String) As String

        If String.IsNullOrEmpty(input) Then Return String.Empty

        Dim res As String = Compress(input, 6, Function(a) KeyStrBase64(a))

        Select Case res.Length Mod 4

            Case 1
                Return res & "==="

            Case 2
                Return res & "=="

            Case 3
                Return res & "="

            Case Else
                Return res

        End Select

    End Function
    Public Shared Function DecompressFromBase64(input As String) As String

        If String.IsNullOrEmpty(input) Then Return String.Empty

        Return Decompress(input.Length, 32, Function(index) GetBaseValue(KeyStrBase64, input(index)))

    End Function

    Public Shared Function CompressToUtf16(input As String) As String

        If String.IsNullOrEmpty(input) Then Return String.Empty

        Return Compress(input, 15, Function(a) ChrW(a + 32))

    End Function
    Public Shared Function DecompressFromUtf16(compressed As String) As String

        If String.IsNullOrEmpty(compressed) Then Return String.Empty

        Return Decompress(compressed.Length, 16384, Function(index) ChrW(AscW(compressed(index)) - 32))

    End Function

    ''' <summary>compress into uint8array (UCS-2 big endian format)</summary>
    Public Shared Function CompressToUInt8Array(uncompressed As String) As Byte()

        Dim compressed As String = Compress(uncompressed)
        Dim buffer = New Byte(compressed.Length * 2 - 1) {} ' 2 bytes per character
        Dim totalLength As Integer = compressed.Length

        For i As Integer = 0 To totalLength - 1

            Dim currentValue As UInteger = AscW(compressed(i))

            buffer(i * 2) = currentValue >> 8
            buffer(i * 2 + 1) = currentValue Mod 256

        Next

        Return buffer

    End Function
    ''' <summary>decompress from uint8array (UCS-2 big endian format)</summary>
    Public Shared Function DecompressFromUInt8Array(compressed As Byte()) As String

        If compressed Is Nothing Then Return String.Empty

        Dim result As New Text.StringBuilder

        For i As Integer = 0 To compressed.Length / 2 - 1

            Dim currentValue As Char = ChrW(compressed(i * 2) * 256 + compressed(i * 2 + 1))

            result.Append(currentValue)

        Next

        Return Decompress(result.ToString())

    End Function

    Public Shared Function CompressToEncodedUriComponent(input As String) As String

        If input Is Nothing Then Return String.Empty

        Return Compress(input, 6, Function(a) KeyStrUriSafe(a))

    End Function
    Public Shared Function DecompressFromEncodedUriComponent(input As String) As String

        If input Is Nothing Then Return String.Empty
        If String.IsNullOrEmpty(input) Then Return Nothing

        input = input.Replace(" ", "+")

        Return Decompress(input.Length, 32, Function(index) GetBaseValue(KeyStrUriSafe, input(index)))

    End Function

    Public Shared Function Compress(uncompressed As String) As String

        If uncompressed Is Nothing Then Return String.Empty

        Return Compress(uncompressed, 16, Function(a) ChrW(a))

    End Function
    Public Shared Function Decompress(compressed As String) As String

        If String.IsNullOrEmpty(compressed) Then Return String.Empty

        Return Decompress(compressed.Length, 32768, Function(index) compressed(index))

    End Function

    ' Private members

    Private Structure CompressData

        Dim [String] As Text.StringBuilder
        Dim Val As Integer
        Dim Position As Integer
        Dim Index As Integer
        Dim BitsPerChar As Integer
        Dim GetCharFromInt As Func(Of Integer, Char)

    End Structure

    Private Structure DecompressData

        Dim Val As Integer
        Dim Position As Integer
        Dim Index As Integer
        Dim ResetValue As Integer
        Dim GetNextValue As Func(Of Integer, Char)

    End Structure

    Private Structure Context

        Dim Dictionary As Dictionary(Of String, Integer)
        Dim DictionaryToCreate As Dictionary(Of String, Boolean)
        Dim C As String
        Dim WC As String
        Dim W As String
        Dim EnlargeIn As Integer
        Dim DictSize As Integer
        Dim NumBits As Integer
        Dim Result As String
        Dim Data As CompressData

    End Structure

    Private Const KeyStrBase64 As String = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/="
    Private Const KeyStrUriSafe As String = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+-$"
    Private Shared ReadOnly BaseReverseDic As New Dictionary(Of String, Dictionary(Of Integer, Char))

    Private Sub New()
    End Sub

    Private Shared Function ReadBit(ByRef data As DecompressData) As Integer

        Dim res As Integer = data.Val And data.Position

        data.Position >>= 1

        If data.Position = 0 Then

            data.Position = data.ResetValue
            data.Val = AscW(data.GetNextValue(data.Index))
            data.Index += 1

        End If

        Return If(res > 0, 1, 0)

    End Function
    Private Shared Function ReadBits(numBits As Integer, ByRef data As DecompressData) As Integer

        Dim res As Integer = 0
        Dim maxpower As Integer = Math.Pow(2, numBits)
        Dim power As Integer = 1

        While power <> maxpower

            res = res Or ReadBit(data) * power
            power <<= 1

        End While

        Return res

    End Function
    Private Shared Sub WriteBit(value As Integer, ByRef data As CompressData)

        data.Val = (data.Val << 1) Or value

        If data.Position = data.BitsPerChar - 1 Then

            data.Position = 0

            data.[String].Append(data.GetCharFromInt(data.Val))

            data.Val = 0

        Else

            data.Position += 1

        End If

    End Sub
    Private Shared Sub WriteBits(numBits As Integer, value As Integer, ByRef data As CompressData)

        For i As Integer = 0 To numBits - 1

            WriteBit(value And 1, data)

            value >>= 1

        Next

    End Sub
    Private Shared Sub WriteBits(numBits As Integer, value As String, ByRef data As CompressData)

        WriteBits(numBits, AscW(value(0)), data)

    End Sub

    Private Shared Sub ProduceW(ByRef context As Context)

        If context.DictionaryToCreate.ContainsKey(context.W) Then

            If AscW(context.W(0)) < 256 Then

                WriteBits(context.NumBits, 0, context.Data)
                WriteBits(8, context.W, context.Data)

            Else

                WriteBits(context.NumBits, 1, context.Data)
                WriteBits(16, context.W, context.Data)

            End If

            DecrementEnlargeIn(context)

            context.DictionaryToCreate.Remove(context.W)

        Else

            WriteBits(context.NumBits, context.Dictionary(context.W), context.Data)

        End If

        DecrementEnlargeIn(context)

    End Sub
    Private Shared Sub DecrementEnlargeIn(ByRef context As Context)

        context.EnlargeIn -= 1

        If context.EnlargeIn = 0 Then

            context.EnlargeIn = Math.Pow(2, context.NumBits)

            context.NumBits += 1

        End If

    End Sub
    Private Shared Function GetBaseValue(alphabet As String, character As Char) As Char

        If Not BaseReverseDic.ContainsKey(alphabet) Then

            Dim newBaseReverseDict = New Dictionary(Of Integer, Char)

            For i As Integer = 0 To alphabet.Length - 1

                newBaseReverseDict(AscW(alphabet(i))) = ChrW(i)

            Next

            BaseReverseDic(alphabet) = newBaseReverseDict

        End If

        Return BaseReverseDic(alphabet)(AscW(character))

    End Function

    Private Shared Function Compress(uncompressed As String, bitsPerChar As Integer, getCharFromInt As Func(Of Integer, Char)) As String

        Dim context As New Context With {
            .Dictionary = New Dictionary(Of String, Integer),
            .DictionaryToCreate = New Dictionary(Of String, Boolean),
            .C = "",
            .WC = "",
            .W = "",
            .EnlargeIn = 2,
            .DictSize = 3,
            .NumBits = 2,
            .Result = "",
            .Data = New CompressData With {
                .[String] = New Text.StringBuilder,
                .Val = 0,
                .Position = 0,
                .BitsPerChar = bitsPerChar,
                .GetCharFromInt = getCharFromInt
            }
        }

        For i As Integer = 0 To uncompressed.Length - 1

            context.C = uncompressed(i)

            If Not context.Dictionary.ContainsKey(context.C) Then

                context.Dictionary(context.C) = context.DictSize
                context.DictSize += 1
                context.DictionaryToCreate(context.C) = True

            End If

            context.WC = context.W & context.C

            If context.Dictionary.ContainsKey(context.WC) Then

                context.W = context.WC

            Else

                ProduceW(context)

                ' Add WC to the dictionary.

                context.Dictionary(context.WC) = context.DictSize
                context.DictSize += 1
                context.W = context.C

            End If

        Next

        ' Output the code for W.

        If Not String.IsNullOrEmpty(context.W) Then

            ProduceW(context)

        End If

        ' Mark the end of the stream.

        WriteBits(context.NumBits, 2, context.Data)

        ' Flush the last character.

        While True

            context.Data.Val <<= 1

            If context.Data.Position = bitsPerChar - 1 Then

                context.Data.String.Append(getCharFromInt(context.Data.Val))

                Exit While

            Else

                context.Data.Position += 1

            End If

        End While

        Return context.Data.String.ToString

    End Function
    Private Shared Function Decompress(length As Integer, resetValue As Integer, getNextValue As Func(Of Integer, Char)) As String

        Dim dictionary As New Dictionary(Of Integer, String)
        Dim enlargeIn As Integer = 4
        Dim dictSize As Integer = 4
        Dim numBits As Integer = 3
        Dim entry As String
        Dim result As New Text.StringBuilder
        Dim c As Integer
        Dim w As String
        Dim errorCount As Integer = 0

        Dim data As New DecompressData With {
            .Val = AscW(getNextValue(0)),
            .Position = resetValue,
            .Index = 1,
            .ResetValue = resetValue,
            .GetNextValue = getNextValue
        }

        For i As Integer = 0 To 3 - 1
            dictionary(i) = ChrW(i)
        Next

        Dim [next] As Integer = ReadBits(2, data)

        Select Case [next]

            Case 0
                c = ReadBits(8, data)

            Case 1
                c = ReadBits(16, data)

            Case 2
                Return String.Empty

        End Select

        dictionary(3) = ChrW(c)
        w = ChrW(c)
        result.Append(ChrW(c))

        While True

            If data.Index > length Then
                Return String.Empty
            End If

            c = ReadBits(numBits, data)

            Select Case c

                Case 0

                    If errorCount > 10000 Then Return "Error"

                    errorCount += 1

                    c = ReadBits(8, data)

                    dictionary(dictSize) = ChrW(c)
                    dictSize += 1
                    c = dictSize - 1
                    enlargeIn -= 1

                Case 1

                    c = ReadBits(16, data)

                    dictionary(dictSize) = ChrW(c)
                    dictSize += 1
                    c = dictSize - 1
                    enlargeIn -= 1

                Case 2

                    Return result.ToString()

            End Select

            If enlargeIn = 0 Then

                enlargeIn = Math.Pow(2, numBits)
                numBits += 1

            End If

            If dictionary.ContainsKey(c) Then

                entry = dictionary(c)

            ElseIf c = dictSize Then

                entry = w & w(0)

            Else

                Return String.Empty

            End If

            result.Append(entry)

            ' Add w+entry[0] to the dictionary.

            dictionary(dictSize) = w & entry(0)
            dictSize += 1
            enlargeIn -= 1

            w = entry

            If enlargeIn = 0 Then

                enlargeIn = Math.Pow(2, numBits)
                numBits += 1

            End If

        End While

        Return result.ToString()

    End Function

End Class