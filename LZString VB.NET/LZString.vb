Public Class LZString

    Public Shared Function compressToBase64(input As String) As String

        If (String.IsNullOrEmpty(input)) Then Return String.Empty

        Dim res As String = _compress(input, 6, Function(a) keystrBase64(a))

        Select Case (res.Length Mod 4)
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
    Public Shared Function decompressfromBase64(input As String) As String

        If (String.IsNullOrEmpty(input)) Then Return String.Empty

        Return _decompress(input.Length, 32, Function(index) getBaseValue(keystrBase64, input(index)))

    End Function
    Public Shared Function compressToUTF16(input As String) As String

        If (String.IsNullOrEmpty(input)) Then Return String.Empty

        Return _compress(input, 15, Function(a) ChrW(a + 32))

    End Function
    Public Shared Function decompressfromUTF16(compressed As String) As String

        If (String.IsNullOrEmpty(compressed)) Then Return String.Empty

        Return _decompress(compressed.Length, 16384, Function(index) ChrW(AscW(compressed(index)) - 32))

    End Function
    ''' <summary>compress into uint8array (UCS-2 big endian format)</summary>
    Public Shared Function compressToUint8Array(uncompressed As String) As Byte()

        Dim compressed As String = compress(uncompressed)
        Dim buf = New Byte(compressed.Length * 2) {} ' 2 bytes per character
        Dim TotalLen As Integer = compressed.Length

        For i As Integer = 0 To TotalLen - 1
            Dim current_value As UInteger = AscW(compressed(i))
            buf(i * 2) = current_value >> 8
            buf(i * 2 + 1) = current_value Mod 256
        Next

        Return buf

    End Function
    ''' <summary>decompress from uint8array (UCS-2 big endian format)</summary>
    Public Shared Function decompressFromUint8Array(compressed As Byte()) As String

        If (compressed Is Nothing) Then Return String.Empty

        Dim result As New Text.StringBuilder
        For i As Integer = 0 To compressed.Length / 2 - 1
            Dim current_value As Char = ChrW(compressed(i * 2) * 256 + compressed(i * 2 + 1))
            result.Append(current_value)
        Next

        Return decompress(result.ToString)

    End Function
    Public Shared Function compressToEncodedURIComponent(input As String) As String

        If (String.IsNullOrEmpty(input)) Then Return String.Empty

        Return _compress(input, 6, Function(a) keyStrUriSafe(a))

    End Function
    Public Shared Function decompressFromEncodedURIComponent(input As String) As String

        If (String.IsNullOrEmpty(input)) Then Return String.Empty
        input = input.Replace(" ", "+")
        Return _decompress(input.Length, 32, Function(index) getBaseValue(keyStrUriSafe, input(index)))

    End Function
    Public Shared Function compress(uncompressed As String) As String

        Return _compress(uncompressed, 16, Function(a) ChrW(a))

    End Function
    Public Shared Function decompress(compressed As String) As String

        If (String.IsNullOrEmpty(compressed)) Then Return String.Empty

        Return _decompress(compressed.Length, 32768, Function(index) compressed(index))

    End Function

    Private Shared ReadOnly keystrBase64 As String = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/="
    Private Shared ReadOnly keyStrUriSafe As String = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+-$"
    Private Shared baseReverseDic As New Dictionary(Of String, Dictionary(Of Integer, Char))

    Private Structure CompressData
        Dim [string] As String
        Dim val As Integer
        Dim position As Integer
        Dim index As Integer
        Dim bitsPerChar As Integer
        Dim getCharFromInt As Func(Of Integer, Char)
    End Structure

    Private Structure DecompressData
        Dim val As Integer
        Dim position As Integer
        Dim index As Integer
        Dim resetValue As Integer
        Dim getNextValue As Func(Of Integer, Char)
    End Structure

    Private Structure Context
        Dim dictionary As Dictionary(Of String, Integer)
        Dim dictionaryToCreate As Dictionary(Of String, Boolean)
        Dim c As String
        Dim wc As String
        Dim w As String
        Dim enlargeIn As Integer
        Dim dictSize As Integer
        Dim numBits As Integer
        Dim result As String
        Dim data As CompressData
    End Structure

    Private Shared Sub writeBit(value As Integer, ByRef data As CompressData)

        data.val = (data.val << 1) Or value

        If (data.position = data.bitsPerChar - 1) Then
            data.position = 0
            data.[string] &= data.getCharFromInt(data.val)
            data.val = 0
        Else
            data.position += 1
        End If

    End Sub
    Private Shared Sub writeBits(numBits As Integer, value As Integer, ByRef data As CompressData)

        For i As Integer = 0 To numBits - 1
            writeBit(value And 1, data)
            value >>= 1
        Next

    End Sub
    Private Shared Sub writeBits(numBits As Integer, value As String, ByRef data As CompressData)

        writeBits(numBits, AscW(value(0)), data)

    End Sub
    Private Shared Sub produceW(ByRef context As Context)

        If (context.dictionaryToCreate.ContainsKey(context.w)) Then
            If (AscW(context.w(0)) < 256) Then
                writeBits(context.numBits, 0, context.data)
                writeBits(8, context.w, context.data)
            Else
                writeBits(context.numBits, 1, context.data)
                writeBits(16, context.w, context.data)
            End If
            decrementEnlargeIn(context)
            context.dictionaryToCreate.Remove(context.w)
        Else
            writeBits(context.numBits, context.dictionary(context.w), context.data)
        End If
        decrementEnlargeIn(context)

    End Sub
    Private Shared Sub decrementEnlargeIn(ByRef context As Context)

        context.enlargeIn -= 1

        If (context.enlargeIn = 0) Then
            context.enlargeIn = Math.Pow(2, context.numBits)
            context.numBits += 1
        End If

    End Sub
    Private Shared Function _compress(uncompressed As String, bitsPerChar As Integer, getCharFromInt As Func(Of Integer, Char)) As String

        Dim context As New Context With {
            .dictionary = New Dictionary(Of String, Integer),
            .dictionaryToCreate = New Dictionary(Of String, Boolean),
            .c = "",
            .wc = "",
            .w = "",
            .enlargeIn = 2,
            .dictSize = 3,
            .numBits = 2,
            .result = "",
            .data = New CompressData With {
                .[string] = "",
                .val = 0,
                .position = 0,
                .bitsPerChar = bitsPerChar,
                .getCharFromInt = getCharFromInt
            }
        }

        For i As Integer = 0 To uncompressed.Length - 1

            context.c = uncompressed(i)

            If (Not context.dictionary.ContainsKey(context.c)) Then
                context.dictionary(context.c) = context.dictSize
                context.dictSize += 1
                context.dictionaryToCreate(context.c) = True
            End If

            context.wc = context.w & context.c

            If (context.dictionary.ContainsKey(context.wc)) Then
                context.w = context.wc
            Else
                produceW(context)
                ' Add wc to the dictionary.
                context.dictionary(context.wc) = context.dictSize
                context.dictSize += 1
                context.w = context.c
            End If

        Next

        ' Output the code for w.
        If (context.w <> "") Then
            produceW(context)
        End If

        ' Mark the end of the stream
        writeBits(context.numBits, 2, context.data)

        ' Flush the last char
        While (True)
            context.data.val <<= 1
            If (context.data.position = bitsPerChar - 1) Then
                context.data.string += (getCharFromInt(context.data.val))
                Exit While
            Else
                context.data.position += 1
            End If
        End While

        Return context.data.string

    End Function
    Private Shared Function readBit(ByRef data As DecompressData) As Integer

        Dim res As Integer = data.val And data.position

        data.position >>= 1

        If (data.position = 0) Then
            data.position = data.resetValue
            data.val = AscW(data.getNextValue(data.index))
            data.index += 1
        End If

        Return If(res > 0, 1, 0)

    End Function
    Private Shared Function readBits(numBits As Integer, ByRef data As DecompressData) As Integer

        Dim res As Integer = 0
        Dim maxpower As Integer = Math.Pow(2, numBits)
        Dim power As Integer = 1

        While (power <> maxpower)
            res = res Or readBit(data) * power
            power <<= 1
        End While

        Return res

    End Function
    Private Shared Function _decompress(length As Integer, resetValue As Integer, getNextValue As Func(Of Integer, Char)) As String

        Dim dictionary As New Dictionary(Of Integer, String)
        Dim enlargeIn As Integer = 4
        Dim dictSize As Integer = 4
        Dim numBits As Integer = 3
        Dim entry As String = ""
        Dim result As New Text.StringBuilder
        Dim c As Integer
        Dim w As String
        Dim errorCount As Integer = 0
        Dim data As New DecompressData With {
            .val = AscW(getNextValue(0)),
            .position = resetValue,
            .index = 1,
            .resetValue = resetValue,
            .getNextValue = getNextValue
        }

        For i As Integer = 0 To 2
            dictionary(i) = ChrW(i)
        Next

        Dim [next] As Integer = readBits(2, data)

        Select Case [next]
            Case 0
                c = readBits(8, data)
            Case 1
                c = readBits(16, data)
            Case 2
                Return String.Empty
        End Select

        dictionary(3) = ChrW(c)
        result.Append(ChrW(c))
        w = ChrW(c)

        While (True)

            c = readBits(numBits, data)

            Select Case c
                Case 0
                    If (errorCount > 10000) Then Return "Error"
                    errorCount += 1
                    c = readBits(8, data)
                    dictionary(dictSize) = ChrW(c)
                    dictSize += 1
                    c = dictSize - 1
                    enlargeIn -= 1
                Case 1
                    c = readBits(16, data)
                    dictionary(dictSize) = ChrW(c)
                    dictSize += 1
                    c = dictSize - 1
                    enlargeIn -= 1
                Case 2
                    Return result.ToString
            End Select

            If (enlargeIn = 0) Then
                enlargeIn = Math.Pow(2, numBits)
                numBits += 1
            End If

            If (dictionary.ContainsKey(c)) Then
                entry = dictionary(c)
            ElseIf (c = dictSize)
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

            If (enlargeIn = 0) Then
                enlargeIn = Math.Pow(2, numBits)
                numBits += 1
            End If

        End While

        Return result.ToString

    End Function
    Private Shared Function getBaseValue(alphabet As String, character As Char) As Char

        If (Not baseReverseDic.ContainsKey(alphabet)) Then
            baseReverseDic(alphabet) = New Dictionary(Of Integer, Char)
            For i As Integer = 0 To alphabet.Length - 1
                baseReverseDic(alphabet)(AscW(alphabet(i))) = ChrW(i)
            Next
        End If

        Return baseReverseDic(alphabet)(AscW(character))

    End Function

End Class