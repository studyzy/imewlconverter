namespace ImeWlConverter.Formats.LibIMEPinyin;

using System.Buffers.Binary;

/// <summary>
/// libime 拼音词库内部使用的 cedar 双数组 Trie（<c>DATrie&lt;float&gt;</c>）的 C# 移植。
///
/// 仅移植构建（<see cref="Set"/>）、序列化（<see cref="Save"/>）与读回
/// （<see cref="Load"/>、<see cref="Enumerate"/>）所需的子集，字节序与
/// libime <c>libime/core/datrie.cpp</c> 保持一致，使生成的文件可被 Fcitx5/libime 直接加载。
/// </summary>
internal sealed class CedarFloatTrie
{
    private const int ValueSize = 4;
    private const int MaxAllocSize = 1 << 16;
    private const bool Ordered = true;
    private const int MaxTrial = 1;

    /// <summary>CEDAR_NO_VALUE（float quiet NaN，载荷 1）。</summary>
    public const int NoValue = unchecked((int)0x7fc00001);

    /// <summary>CEDAR_NO_PATH（float quiet NaN，载荷 2）。</summary>
    public const int NoPath = unchecked((int)0x7fc00002);

    private struct Node
    {
        public int Base;
        public int Check;
    }

    private struct Ninfo
    {
        public byte Sibling;
        public byte Child;
    }

    private struct Block
    {
        public int Prev;
        public int Next;
        public short Num;
        public short Reject;
        public int Trial;
        public int Ehead;
    }

    private struct Npos
    {
        public long Offset;
        public int Index;
    }

    private Node[] _array = new Node[256];
    private int _arrayLen;

    private byte[] _tail = new byte[4];
    private int _tailLen;

    private readonly List<int> _tail0 = new();

    private Block[] _block = new Block[1];
    private int _blockLen;

    private Ninfo[] _ninfo = new Ninfo[256];
    private int _ninfoLen;

    private int _bheadF;
    private int _bheadC;
    private int _bheadO;
    private readonly int[] _reject = new int[257];

    public CedarFloatTrie()
    {
        Init();
    }

    private int Size => _ninfoLen;

    private int Capacity => _arrayLen;

    private void Init()
    {
        _bheadF = _bheadC = _bheadO = 0;
        _tail0.Clear();

        ArrayResize(256);
        _array[0].Base = 0;
        _array[0].Check = -1;
        for (var i = 1; i < 256; i++)
        {
            _array[i].Base = i == 1 ? -255 : -(i - 1);
            _array[i].Check = i == 255 ? -1 : -(i + 1);
        }

        NinfoResize(256);
        BlockResize(1);
        _block[0].Ehead = 1;

        TailResize(0);
        TailResize(ValueSize);

        for (var i = 0; i <= 256; i++)
            _reject[i] = i + 1;
    }

    // ---------------------------------------------------------------- build

    /// <summary>插入/覆盖一个键（键为原始字节，例如 "ni!你好" 的 libime 编码形式）。</summary>
    public void Set(ReadOnlySpan<byte> key, float value)
    {
        var from = 0;
        long offset = 0;
        var pos = 0;
        Update(key, ref from, ref offset, ref pos, value);
    }

    private void Update(ReadOnlySpan<byte> key, ref int from, ref long offset, ref int pos, float value)
    {
        var len = key.Length;
        var hadTraversalOffset = offset != 0;
        if (offset == 0)
        {
            while (_array[from].Base >= 0)
            {
                if (pos == len)
                {
                    var to = Follow(ref from, 0);
                    StoreNodeValue(to, value);
                    return;
                }

                from = Follow(ref from, key[pos]);
                pos++;
            }

            offset = -_array[from].Base;
        }

        if (offset >= ValueSize)
        {
            var posOrig = pos;
            var tailBase = (int)offset - posOrig;
            while (pos < len && key[pos] == _tail[tailBase + pos])
                pos++;

            if (pos == len && _tail[tailBase + pos] == 0)
            {
                StoreTailValue(tailBase + len + 1, value);
                return;
            }

            if (hadTraversalOffset)
            {
                for (var offset2 = -_array[from].Base; offset2 < offset;)
                {
                    from = Follow(ref from, _tail[offset2]);
                    offset2++;
                }
            }

            for (var pos2 = posOrig; pos2 < pos; pos2++)
                from = Follow(ref from, key[pos2]);

            var moved = pos - posOrig;
            if (_tail[tailBase + pos] != 0)
            {
                var toOld = Follow(ref from, _tail[tailBase + pos]);
                moved++;
                _array[toOld].Base = -(int)(offset + moved);
                moved -= 1 + ValueSize;
            }

            moved += (int)offset;
            for (var i = (int)offset; i <= moved; i += 1 + ValueSize)
                _tail0.Add(i);

            if (pos == len || _tail[tailBase + pos] == 0)
            {
                var to = Follow(ref from, 0);
                if (pos == len)
                {
                    StoreNodeValue(to, value);
                    return;
                }

                StoreNodeBits(to, LoadTailValue(tailBase + pos + 1));
            }

            from = Follow(ref from, key[pos]);
            pos++;
        }

        var needed = len - pos + 1 + ValueSize;
        if (pos == len && _tail0.Count > 0)
        {
            var offset0 = _tail0[^1];
            _tail0.RemoveAt(_tail0.Count - 1);
            _tail[offset0] = 0;
            _array[from].Base = -offset0;
            StoreTailValue(offset0 + 1, value);
            return;
        }

        _array[from].Base = -_tailLen;
        var oldLength = _tailLen;
        var writePos = pos;
        TailResize(_tailLen + needed);
        if (pos < len)
        {
            do
            {
                _tail[oldLength + pos - writePos] = key[pos];
            } while (++pos < len);
        }

        StoreTailValue(oldLength - writePos + len + 1, value);
    }

    /// <summary>对应 cedar 的 <c>_follow</c>：沿标签下行，必要时新建节点。</summary>
    private int Follow(ref int from, byte label)
    {
        int to;
        var nodeBase = _array[from].Base;
        if (nodeBase < 0 || _array[nodeBase ^ label].Check < 0)
        {
            to = PopEnode(nodeBase, label, from);
            PushSibling(from, to ^ label, label, nodeBase >= 0);
        }
        else if (_array[nodeBase ^ label].Check != from)
        {
            to = Resolve(ref from, nodeBase, label);
        }
        else
        {
            to = nodeBase ^ label;
        }

        return to;
    }

    // -------------------------------------------------------------- placement

    private int FindPlace()
    {
        if (_bheadC != 0)
            return _block[_bheadC].Ehead;
        if (_bheadO != 0)
            return _block[_bheadO].Ehead;
        return AddBlock() << 8;
    }

    private int FindPlace(byte[] child, int first, int last)
    {
        if (_bheadO != 0)
        {
            var bi = _bheadO;
            var bz = _block[_bheadO].Prev;
            var nc = last - first;
            while (true)
            {
                if (_block[bi].Num >= nc && nc < _block[bi].Reject)
                {
                    var e = _block[bi].Ehead;
                    while (true)
                    {
                        var baseIndex = e ^ child[first];
                        var p = first;
                        while (true)
                        {
                            p++;
                            if (_array[baseIndex ^ child[p]].Check >= 0)
                                break;
                            if (p + 1 == last)
                            {
                                _block[bi].Ehead = e;
                                return _block[bi].Ehead;
                            }
                        }

                        e = -_array[e].Check;
                        if (e == _block[bi].Ehead)
                            break;
                    }
                }

                _block[bi].Reject = (short)nc;
                if (_block[bi].Reject < _reject[_block[bi].Num])
                    _reject[_block[bi].Num] = _block[bi].Reject;

                var biNext = _block[bi].Next;
                _block[bi].Trial++;
                if (_block[bi].Trial == MaxTrial)
                    TransferBlock(bi, ref _bheadO, ref _bheadC);
                if (bi == bz)
                    break;
                bi = biNext;
            }
        }

        return AddBlock() << 8;
    }

    private void PopBlock(int bi, ref int headIn, bool last)
    {
        if (last)
        {
            headIn = 0;
        }
        else
        {
            var prev = _block[bi].Prev;
            var next = _block[bi].Next;
            _block[prev].Next = next;
            _block[next].Prev = prev;
            if (bi == headIn)
                headIn = next;
        }
    }

    private void PushBlock(int bi, ref int headOut, bool empty)
    {
        if (empty)
        {
            _block[bi].Prev = bi;
            _block[bi].Next = bi;
            headOut = bi;
        }
        else
        {
            var tailOut = _block[headOut].Prev;
            _block[bi].Prev = tailOut;
            _block[bi].Next = headOut;
            _block[tailOut].Next = bi;
            _block[headOut].Prev = bi;
            headOut = bi;
        }
    }

    private int AddBlock()
    {
        if (Size == Capacity)
        {
            var newCapacity = Capacity + (Size >= MaxAllocSize ? MaxAllocSize : Size);
            ArrayResize(newCapacity);
            BlockResize(Size >> 8);
        }

        BlockResize(_blockLen + 1);
        _block[Size >> 8].Ehead = Size;

        _array[Size].Base = -(Size + 255);
        _array[Size].Check = -(Size + 1);
        for (var i = Size + 1; i < Size + 255; i++)
        {
            _array[i].Base = -(i - 1);
            _array[i].Check = -(i + 1);
        }

        _array[Size + 255].Base = -(Size + 254);
        _array[Size + 255].Check = -Size;

        PushBlock(Size >> 8, ref _bheadO, _bheadO == 0);
        NinfoResize(Size + 256);
        return (Size >> 8) - 1;
    }

    private void TransferBlock(int bi, ref int headIn, ref int headOut)
    {
        PopBlock(bi, ref headIn, bi == _block[bi].Next);
        PushBlock(bi, ref headOut, headOut == 0 && _block[bi].Num != 0);
    }

    private int PopEnode(int nodeBase, byte label, int from)
    {
        var e = nodeBase < 0 ? FindPlace() : nodeBase ^ label;
        var bi = e >> 8;

        _block[bi].Num--;
        if (_block[bi].Num == 0)
        {
            if (bi != 0)
                TransferBlock(bi, ref _bheadC, ref _bheadF);
        }
        else
        {
            _array[-_array[e].Base].Check = _array[e].Check;
            _array[-_array[e].Check].Base = _array[e].Base;
            if (e == _block[bi].Ehead)
                _block[bi].Ehead = -_array[e].Check;
            if (bi != 0 && _block[bi].Num == 1 && _block[bi].Trial != MaxTrial)
                TransferBlock(bi, ref _bheadO, ref _bheadC);
        }

        _array[e].Base = label != 0 ? -1 : 0;
        _array[e].Check = from;
        if (nodeBase < 0)
            _array[from].Base = e ^ label;
        return e;
    }

    private void PushEnode(int e)
    {
        var bi = e >> 8;
        _block[bi].Num++;
        if (_block[bi].Num == 1)
        {
            _block[bi].Ehead = e;
            _array[e].Base = -e;
            _array[e].Check = -e;
            if (bi != 0)
                TransferBlock(bi, ref _bheadF, ref _bheadC);
        }
        else
        {
            var prev = _block[bi].Ehead;
            var next = -_array[prev].Check;
            _array[e].Base = -prev;
            _array[e].Check = -next;
            _array[prev].Check = -e;
            _array[next].Base = -e;
            if (_block[bi].Num == 2 || _block[bi].Trial == MaxTrial)
            {
                if (bi != 0)
                    TransferBlock(bi, ref _bheadC, ref _bheadO);
            }

            _block[bi].Trial = 0;
        }

        if (_block[bi].Reject < _reject[_block[bi].Num])
            _block[bi].Reject = (short)_reject[_block[bi].Num];

        _ninfo[e] = default;
    }

    private void PushSibling(int from, int nodeBase, byte label, bool flag = true)
    {
        var useChild = true;
        var index = from;
        var c = _ninfo[from].Child;
        if (flag && (Ordered ? label > c : c == 0))
        {
            do
            {
                useChild = false;
                index = nodeBase ^ c;
                c = _ninfo[index].Sibling;
            } while (Ordered && c != 0 && c < label);
        }

        _ninfo[nodeBase ^ label].Sibling = c;
        if (useChild)
            _ninfo[index].Child = label;
        else
            _ninfo[index].Sibling = label;
    }

    private void PopSibling(int from, int nodeBase, byte label)
    {
        var useChild = true;
        var index = from;
        var c = _ninfo[from].Child;
        while (c != label)
        {
            useChild = false;
            index = nodeBase ^ c;
            c = _ninfo[index].Sibling;
        }

        if (useChild)
            _ninfo[index].Child = _ninfo[nodeBase ^ label].Sibling;
        else
            _ninfo[index].Sibling = _ninfo[nodeBase ^ label].Sibling;
    }

    private bool Consult(int baseN, int baseP, byte cN, byte cP)
    {
        do
        {
            cN = _ninfo[baseN ^ cN].Sibling;
            cP = _ninfo[baseP ^ cP].Sibling;
        } while (cN != 0 && cP != 0);

        return cP != 0;
    }

    private int SetChild(byte[] p, int index, int nodeBase, byte c, byte label, bool hasLabel)
    {
        if (c == 0)
        {
            p[index] = c;
            index++;
            c = _ninfo[nodeBase ^ c].Sibling;
        }

        if (Ordered && hasLabel)
        {
            while (c != 0 && c < label)
            {
                p[index] = c;
                index++;
                c = _ninfo[nodeBase ^ c].Sibling;
            }
        }

        if (hasLabel)
        {
            p[index] = label;
            index++;
        }

        while (c != 0)
        {
            p[index] = c;
            index++;
            c = _ninfo[nodeBase ^ c].Sibling;
        }

        return index;
    }

    private int Resolve(ref int fromN, int baseN, byte labelN)
    {
        var toPn = baseN ^ labelN;
        var fromP = _array[toPn].Check;
        var baseP = _array[fromP].Base;
        var flag = Consult(baseN, baseP, _ninfo[fromN].Child, _ninfo[fromP].Child);

        var child = new byte[256];
        var first = 0;
        var last = flag
            ? SetChild(child, first, baseN, _ninfo[fromN].Child, labelN, true)
            : SetChild(child, first, baseP, _ninfo[fromP].Child, 0, false);

        var newBase = (first + 1 == last ? FindPlace() : FindPlace(child, first, last)) ^ child[first];
        var from = flag ? fromN : fromP;
        var oldBase = flag ? baseN : baseP;
        if (flag && child[first] == labelN)
            _ninfo[from].Child = labelN;

        _array[from].Base = newBase;
        for (var pi = first; pi < last; pi++)
        {
            var to = PopEnode(newBase, child[pi], from);
            var toOld = oldBase ^ child[pi];
            _ninfo[to].Sibling = pi + 1 == last ? (byte)0 : child[pi + 1];
            if (flag && toOld == toPn)
                continue;

            _array[to].Base = _array[toOld].Base;
            if (_array[to].Base > 0 && child[pi] != 0)
            {
                var c = _ninfo[toOld].Child;
                _ninfo[to].Child = c;
                while (true)
                {
                    _array[_array[to].Base ^ c].Check = to;
                    c = _ninfo[_array[to].Base ^ c].Sibling;
                    if (c == 0)
                        break;
                }
            }

            if (!flag && toOld == fromN)
                fromN = to;

            if (!flag && toOld == toPn)
            {
                PushSibling(fromN, toPn ^ labelN, labelN);
                _ninfo[toOld].Child = 0;
                _array[toOld].Base = labelN != 0 ? -1 : 0;
                _array[toOld].Check = fromN;
            }
            else
            {
                PushEnode(toOld);
            }
        }

        return flag ? newBase ^ labelN : toPn;
    }

    // ------------------------------------------------------------ serialize

    /// <summary>把 trie 序列化为 libime <c>DATrie&lt;float&gt;</c> 的字节流（不含 magic/zstd 外壳）。</summary>
    public void Save(Stream output)
    {
        ShrinkTail();

        WriteUInt32BigEndian(output, (uint)_tailLen);
        WriteUInt32BigEndian(output, (uint)Size);
        output.Write(_tail, 0, _tailLen);

        for (var i = 0; i < Size; i++)
        {
            WriteInt32BigEndian(output, _array[i].Base);
            WriteInt32BigEndian(output, _array[i].Check);
        }

        WriteInt32BigEndian(output, _bheadF);
        WriteInt32BigEndian(output, _bheadC);
        WriteInt32BigEndian(output, _bheadO);

        for (var i = 0; i < Size; i++)
        {
            output.WriteByte(_ninfo[i].Sibling);
            output.WriteByte(_ninfo[i].Child);
        }

        for (var i = 0; i < _blockLen; i++)
        {
            WriteInt32BigEndian(output, _block[i].Prev);
            WriteInt32BigEndian(output, _block[i].Next);
            WriteInt16BigEndian(output, _block[i].Num);
            WriteInt16BigEndian(output, _block[i].Reject);
            WriteInt32BigEndian(output, _block[i].Trial);
            WriteInt32BigEndian(output, _block[i].Ehead);
        }
    }

    private void ShrinkTail()
    {
        var t = new List<byte>(_tailLen);
        for (var i = 0; i < ValueSize; i++)
            t.Add(0);

        for (var to = 0; to < Size; to++)
        {
            if (_array[to].Check < 0 || _array[_array[to].Check].Base == to || _array[to].Base >= 0)
                continue;

            var source = -_array[to].Base;
            _array[to].Base = -t.Count;
            var i = 0;
            do
            {
                t.Add(_tail[source + i]);
            } while (_tail[source + i++] != 0);

            var raw = BinaryPrimitives.ReadInt32LittleEndian(_tail.AsSpan(source + i, ValueSize));
            t.Add((byte)raw);
            t.Add((byte)(raw >> 8));
            t.Add((byte)(raw >> 16));
            t.Add((byte)(raw >> 24));
        }

        _tail = t.ToArray();
        _tailLen = _tail.Length;
        _tail0.Clear();
    }

    /// <summary>从 libime <c>DATrie&lt;float&gt;</c> 字节流（不含 magic/zstd 外壳）读入。</summary>
    public static CedarFloatTrie Load(Stream input)
    {
        var trie = new CedarFloatTrie();
        trie.ReadFrom(input);
        return trie;
    }

    private void ReadFrom(Stream input)
    {
        var tailLength = ReadUInt32BigEndian(input);
        var count = ReadUInt32BigEndian(input);

        _tail = new byte[tailLength];
        _tailLen = (int)tailLength;
        ReadExactly(input, _tail);

        ArrayResize((int)count);
        var buffer = new byte[8];
        for (var i = 0; i < count; i++)
        {
            ReadExactly(input, buffer);
            _array[i].Base = BinaryPrimitives.ReadInt32BigEndian(buffer);
            _array[i].Check = BinaryPrimitives.ReadInt32BigEndian(buffer.AsSpan(4));
        }

        _bheadF = ReadInt32BigEndian(input);
        _bheadC = ReadInt32BigEndian(input);
        _bheadO = ReadInt32BigEndian(input);

        NinfoResize((int)count);
        var ni = new byte[2];
        for (var i = 0; i < count; i++)
        {
            ReadExactly(input, ni);
            _ninfo[i].Sibling = ni[0];
            _ninfo[i].Child = ni[1];
        }

        var blockCount = (int)(count >> 8);
        BlockResize(blockCount);
        var blk = new byte[20];
        for (var i = 0; i < blockCount; i++)
        {
            ReadExactly(input, blk);
            _block[i].Prev = BinaryPrimitives.ReadInt32BigEndian(blk);
            _block[i].Next = BinaryPrimitives.ReadInt32BigEndian(blk.AsSpan(4));
            _block[i].Num = BinaryPrimitives.ReadInt16BigEndian(blk.AsSpan(8));
            _block[i].Reject = BinaryPrimitives.ReadInt16BigEndian(blk.AsSpan(10));
            _block[i].Trial = BinaryPrimitives.ReadInt32BigEndian(blk.AsSpan(12));
            _block[i].Ehead = BinaryPrimitives.ReadInt32BigEndian(blk.AsSpan(16));
        }

        _tail0.Clear();
    }

    // ------------------------------------------------------------- enumerate

    /// <summary>枚举全部键值对。</summary>
    public IEnumerable<(byte[] Key, float Value)> Enumerate()
    {
        var npos = default(Npos);
        var len = 0;
        var root = default(Npos);
        var resultRaw = Begin(ref npos, ref len);
        while (resultRaw != NoPath)
        {
            if (resultRaw != NoValue)
                yield return (Suffix(len, npos), BitConverter.Int32BitsToSingle(resultRaw));

            resultRaw = Next(ref npos, ref len, root);
        }
    }

    private int Begin(ref Npos npos, ref int len)
    {
        var from = npos.Index;
        var nodeBase = npos.Offset != 0 ? -(int)npos.Offset : _array[from].Base;
        if (nodeBase >= 0)
        {
            var c = _ninfo[from].Child;
            if (from == 0)
            {
                c = _ninfo[nodeBase ^ c].Sibling;
                if (c == 0)
                    return NoPath;
            }

            while (c != 0 && nodeBase >= 0)
            {
                from = nodeBase ^ c;
                nodeBase = _array[from].Base;
                c = _ninfo[from].Child;
                len++;
            }

            if (nodeBase >= 0)
            {
                npos.Index = from;
                return _array[nodeBase ^ c].Base;
            }
        }

        npos.Index = from;
        var lenTail = StrLen(-nodeBase);
        npos.Offset = (uint)(-nodeBase) + lenTail;
        len += lenTail;
        return LoadTailValue(-nodeBase + lenTail + 1);
    }

    private int Next(ref Npos npos, ref int len, Npos root)
    {
        byte c = 0;
        if (npos.Offset != 0)
        {
            if (root.Offset != 0)
                return NoPath;
            var offset = npos.Offset;
            npos.Offset = 0;
            len -= (int)(offset - -_array[npos.Index].Base);
        }
        else
        {
            c = _ninfo[_array[npos.Index].Base].Sibling;
        }

        while (c == 0 && !(npos.Offset == root.Offset && npos.Index == root.Index))
        {
            c = _ninfo[npos.Index].Sibling;
            npos.Index = _array[npos.Index].Check;
            len--;
        }

        if (c == 0)
            return NoPath;

        npos.Index = _array[npos.Index].Base ^ c;
        len++;
        return Begin(ref npos, ref len);
    }

    private byte[] Suffix(int len, Npos npos)
    {
        var key = new byte[len];
        var to = npos.Index;
        var offset = npos.Offset;
        if (offset != 0)
        {
            var lenTail = StrLen(-_array[to].Base);
            if (len > lenTail)
            {
                len -= lenTail;
            }
            else
            {
                lenTail = len;
                len = 0;
            }

            Array.Copy(_tail, (int)(offset - lenTail), key, len, lenTail);
        }

        while (len-- > 0)
        {
            var from = _array[to].Check;
            key[len] = (byte)(_array[from].Base ^ to);
            to = from;
        }

        return key;
    }

    private int StrLen(int offset)
    {
        var i = 0;
        while (_tail[offset + i] != 0)
            i++;
        return i;
    }

    // -------------------------------------------------------------- helpers

    private void ArrayResize(int newLen)
    {
        if (newLen > _array.Length)
        {
            var cap = _array.Length == 0 ? 256 : _array.Length;
            while (cap < newLen)
                cap *= 2;
            Array.Resize(ref _array, cap);
        }

        if (newLen > _arrayLen)
            Array.Clear(_array, _arrayLen, newLen - _arrayLen);
        _arrayLen = newLen;
    }

    private void NinfoResize(int newLen)
    {
        if (newLen > _ninfo.Length)
        {
            var cap = _ninfo.Length == 0 ? 256 : _ninfo.Length;
            while (cap < newLen)
                cap *= 2;
            Array.Resize(ref _ninfo, cap);
        }

        if (newLen > _ninfoLen)
            Array.Clear(_ninfo, _ninfoLen, newLen - _ninfoLen);
        _ninfoLen = newLen;
    }

    private void BlockResize(int newLen)
    {
        if (newLen > _block.Length)
        {
            var cap = _block.Length == 0 ? 1 : _block.Length;
            while (cap < newLen)
                cap *= 2;
            Array.Resize(ref _block, cap);
        }

        for (var i = _blockLen; i < newLen; i++)
            _block[i] = new Block { Num = 256, Reject = 257 };
        _blockLen = newLen;
    }

    private void TailResize(int newLen)
    {
        if (newLen > _tail.Length)
        {
            var cap = _tail.Length == 0 ? 16 : _tail.Length;
            while (cap < newLen)
                cap *= 2;
            Array.Resize(ref _tail, cap);
        }

        if (newLen > _tailLen)
            Array.Clear(_tail, _tailLen, newLen - _tailLen);
        _tailLen = newLen;
    }

    private void StoreNodeValue(int node, float value) => StoreNodeBits(node, BitConverter.SingleToInt32Bits(value));

    private void StoreNodeBits(int node, int bits) => _array[node].Base = bits;

    private void StoreTailValue(int offset, float value) =>
        BinaryPrimitives.WriteInt32LittleEndian(
            _tail.AsSpan(offset, ValueSize),
            BitConverter.SingleToInt32Bits(value));

    private int LoadTailValue(int offset) =>
        BinaryPrimitives.ReadInt32LittleEndian(_tail.AsSpan(offset, ValueSize));

    private static void WriteUInt32BigEndian(Stream output, uint value)
    {
        Span<byte> buffer = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(buffer, value);
        output.Write(buffer);
    }

    private static void WriteInt32BigEndian(Stream output, int value)
    {
        Span<byte> buffer = stackalloc byte[4];
        BinaryPrimitives.WriteInt32BigEndian(buffer, value);
        output.Write(buffer);
    }

    private static void WriteInt16BigEndian(Stream output, short value)
    {
        Span<byte> buffer = stackalloc byte[2];
        BinaryPrimitives.WriteInt16BigEndian(buffer, value);
        output.Write(buffer);
    }

    private static uint ReadUInt32BigEndian(Stream input)
    {
        Span<byte> buffer = stackalloc byte[4];
        ReadExactly(input, buffer);
        return BinaryPrimitives.ReadUInt32BigEndian(buffer);
    }

    private static int ReadInt32BigEndian(Stream input)
    {
        Span<byte> buffer = stackalloc byte[4];
        ReadExactly(input, buffer);
        return BinaryPrimitives.ReadInt32BigEndian(buffer);
    }

    private static void ReadExactly(Stream input, Span<byte> buffer)
    {
        var read = 0;
        while (read < buffer.Length)
        {
            var n = input.Read(buffer[read..]);
            if (n <= 0)
                throw new EndOfStreamException();
            read += n;
        }
    }
}
