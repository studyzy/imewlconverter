namespace ImeWlConverter.Formats.GboardBinary;

/// <summary>
/// 双数组 Trie 构建器 —— Google <c>da_trie_builder.cc</c> 的忠实移植
/// (Gboard <c>user_dict_3_3</c> 的 DA-TRIE 块即由它生成)。
///
/// 算法要点:
/// <list type="bullet">
///   <item>节点 1 = 根; 节点 0 存空闲链表头; 节点 0 的 base = -(最后一个节点号)。</item>
///   <item>空闲节点: <c>base[i] = -(i-1)</c>, <c>check[i] = -(i+1)</c>, 末节点 check = 0。</item>
///   <item>插入冲突时按子树大小决定搬移哪一侧(<c>relocate</c>)。</item>
/// </list>
///
/// ⚠️ 重要:<c>insert</c> 过程中的 <c>relocate</c> 会搬移已有节点,
/// 所以**必须先把所有路径插入, 再重新 lookup 解析终端节点号**,
/// 否则记录的节点号会失效(这是大词典构建失败的经典原因)。
/// </summary>
internal sealed class GboardDaTrie
{
    /// <summary>
    /// 可增长的 uint32 数组容器。
    ///
    /// ⚠️ 为什么不用裸数组: <c>Base[i] = F()</c> 中, 数组引用会在求值 <c>F()</c>
    /// **之前**被捕获; 若 <c>F()</c> 内部触发了扩容(Array.Resize 换掉数组对象),
    /// 赋值就会写进旧数组而丢失 —— 这正是 trie 无限增长的根因。
    /// 用索引器后 <c>Base[i] = x</c> 变成对容器的调用, 内部数组可安全替换。
    /// </summary>
    internal sealed class U32Store
    {
        private uint[] _data;
        public U32Store(int capacity) => _data = new uint[capacity];
        public uint this[int index]
        {
            get => _data[index];
            set => _data[index] = value;
        }
        /// <summary>确保至少能容纳 <paramref name="count"/> 个元素(容量翻倍)。</summary>
        public void Ensure(int count)
        {
            if (count <= _data.Length)
                return;
            var cap = _data.Length;
            while (cap < count)
                cap *= 2;
            Array.Resize(ref _data, cap);
        }
    }

    /// <summary>逻辑节点数(必须是 256 的倍数, 与官方一致)。</summary>
    public int N { get; private set; } = 256;

    private readonly U32Store _base = new(256);
    private readonly U32Store _check = new(256);

    /// <summary>节点 base 表(按索引读写, 可安全扩容)。</summary>
    public U32Store Base { get; }
    /// <summary>节点 check 表(按索引读写, 可安全扩容)。</summary>
    public U32Store Check { get; }

    public GboardDaTrie()
    {
        Base = _base;
        Check = _check;
    }

    private byte[] _alphabet = [];

    /// <summary>字符集大小。</summary>
    public int Alpha => _alphabet.Length;

    /// <summary>设置字符集(路径中实际出现的所有字节, 升序)。</summary>
    public void SetAlphabet(IEnumerable<byte> chars)
        => _alphabet = chars.Distinct().OrderBy(c => c).ToArray();

    private static long U(long x) => x & 0xFFFFFFFFL;

    private static readonly byte[] NoChildren = [];

    // ------------------------------------------------------------------
    // 容量管理: 逻辑大小严格 +256, 但底层数组按倍数扩容以避免 O(n²) 拷贝
    // ------------------------------------------------------------------
    private void EnsureCapacity(int need)
    {
        Base.Ensure(need);
        Check.Ensure(need);
    }

    // ------------------------------------------------------------------
    // sub_1D5158  初始化空闲链表
    // ------------------------------------------------------------------
    public void InitFree()
    {
        for (var i = 0; i < N - 1; i++)
            Check[i] = (uint)U(~(long)i);
        for (var j = 1; j < N; j++)
            Base[j] = (uint)U(1L - j);
        Check[N - 1] = 0;
        Base[0] = (uint)U(1L - N);

        RemoveFree(1);
        Base[1] = 1;
        Check[1] = 1;
    }

    // ------------------------------------------------------------------
    // sub_1D4E5C  扩容 +256
    // ------------------------------------------------------------------
    /// <summary>
    /// 节点数上限。正常词典每字符约 1~3 个节点, 13,955 词约 25 万节点。
    /// 若输入数据异常(路径格式错误), 空闲链表会被破坏并导致 N 无界增长 ——
    /// 这里设一个远高于正常值的上限, 让异常输入快速失败而不是耗尽内存。
    /// </summary>
    private const int MaxNodes = 16 * 1024 * 1024;

    public void Grow()
    {
        if (N >= MaxNodes)
            throw new InvalidOperationException(
                $"DA-trie 节点数超过上限 {MaxNodes} —— 输入路径格式异常(疑似非法字符或空的拼音)。");
        var oldN = N;
        N += 256;
        EnsureCapacity(N);
        for (var i = oldN; i < N; i++)
        {
            Base[i] = 0;
            Check[i] = 0;
        }

        var last = U(-(long)Base[0]);
        Check[(int)last] = (uint)U(-(long)oldN);
        for (var i = oldN; i < N - 1; i++)
            Check[i] = (uint)U(~(long)i);
        Check[N - 1] = 0;

        long prev = last;
        for (var i = oldN; i < N; i++)
        {
            Base[i] = (uint)U(-prev);
            prev = i;
        }
        Base[0] = (uint)U(-prev);
    }

    // ------------------------------------------------------------------
    // sub_1D5128  从空闲链表摘除(标记为占用)
    // ------------------------------------------------------------------
    public void RemoveFree(long idx)
    {
        var b = (long)Base[(int)idx];
        var c = (long)Check[(int)idx];
        var prev = U(-b);
        var next = U(-c);
        Check[(int)prev] = (uint)c;
        Base[(int)next] = (uint)b;
    }

    // ------------------------------------------------------------------
    // sub_1D53E8  把节点 a3 插到 walk(a2) 的结果之前
    // ------------------------------------------------------------------
    private void AddFree(long a2, long a3)
    {
        while (true)
        {
            if (a2 >= a3)
                break;
            var v4 = (long)Check[(int)a2];
            if (v4 == 0)
                break;
            a2 = U(-v4);
        }
        var v8 = (long)Base[(int)a2];
        Base[(int)a3] = (uint)v8;
        Check[(int)a3] = (uint)U(-a2);
        Check[(int)U(-v8)] = (uint)U(-a3);
        Base[(int)a2] = (uint)U(-a3);
    }

    // ------------------------------------------------------------------
    // sub_1D4FE8  寻找能容纳 children 的 base 值
    // ------------------------------------------------------------------
    private long FindBase(IReadOnlyList<byte> children, long a4)
    {
        var v13 = U(-(long)Check[0]);
        var guard = 0;
        while (v13 <= a4)
        {
            if (++guard > N * 5)
                throw new InvalidOperationException("find_base: 空闲链表异常 (阶段 1)");
            v13 = U(-(long)Check[(int)v13]);
        }
        while (v13 + 256 - a4 >= N - 1)
            Grow();

        guard = 0;
        while (true)
        {
            if (++guard > N * 5)
                throw new InvalidOperationException("find_base: 空闲链表异常 (阶段 2)");

            var ok = true;
            foreach (var ch in children)
            {
                var idx = v13 - a4 + ch;
                while (idx >= N)
                    Grow();
                var ck = (long)Check[(int)idx];
                // 空闲 = 高位置 1(~i) 或 0(空闲链尾)
                if ((ck & 0x80000000L) == 0 && ck != 0)
                {
                    ok = false;
                    break;
                }
            }
            if (ok)
                return U(v13 - a4);

            v13 = U(-(long)Check[(int)v13]);
            while (v13 + 256 - a4 >= N - 1)
                Grow();
        }
    }

    // ------------------------------------------------------------------
    // sub_1D559C  收集节点在字符集中的子节点字符
    // ------------------------------------------------------------------
    private List<byte> CollectChildren(long node)
    {
        var result = new List<byte>();
        var nodeBase = (long)Base[(int)node];
        foreach (var c in _alphabet)
        {
            var v = c + nodeBase;
            if (v < N && (long)Check[(int)v] == node)
                result.Add(c);
        }
        return result;
    }

    // ------------------------------------------------------------------
    // sub_1D6410  把 a3 的子节点搬到新 base a4
    // ------------------------------------------------------------------
    private long Relocate(long a2, long a3, long a4, IReadOnlyList<byte> a5, int a6)
    {
        var oldBase = (long)Base[(int)a3];
        var saved = new uint[a6];
        long v20 = 0, v18 = a2;

        for (var v17 = 0; v17 < a6; v17++)
        {
            var v21 = (long)a5[v17];
            var v22 = a4 + v21;
            var v23 = v21 + oldBase;

            if (v22 == v20)
                v20 = U(-(long)Base[(int)v20]);
            while (v22 >= N)
                Grow();
            RemoveFree(v22);
            Check[(int)v22] = (uint)U(a3);
            saved[v17] = v23 < N ? Base[(int)v23] : 0;

            if (v21 > 0)
            {
                var bb = v23 < N ? (long)Base[(int)v23] : 0;
                if ((bb & 0x7FFFFFFFL) > 0)
                {
                    foreach (var c in _alphabet)
                    {
                        var v27 = c + bb;
                        if (v27 < N && (long)Check[(int)v27] == v23)
                            Check[(int)v27] = (uint)U(v22);
                    }
                }
                if (v23 == a2)
                    v18 = v22;
            }

            if (v23 == 1)
            {
                Base[3] = 1;
            }
            else
            {
                var v25 = v20;
                v20 = v23;
                AddFree(v25, v23);
            }
        }

        Base[(int)a3] = (uint)U(a4);
        for (var v30 = 0; v30 < a6; v30++)
            Base[(int)(a4 + a5[v30])] = saved[v30];

        return U(v18);
    }

    // ------------------------------------------------------------------
    // 查找路径(存在返回终端节点号, 否则 null)
    // ------------------------------------------------------------------
    public long? Lookup(ReadOnlySpan<byte> path)
    {
        long n = 1;
        foreach (var b in path)
        {
            var idx = b + (long)Base[(int)n];
            if (idx >= N || idx < 0 || (long)Check[(int)idx] != n)
                return null;
            n = idx;
        }
        return n;
    }

    // ------------------------------------------------------------------
    // sub_1D66CC  插入一条路径, 返回终端节点号
    // ------------------------------------------------------------------
    public long Insert(ReadOnlySpan<byte> path, uint value)
    {
        long n = 1;
        for (var pi = 0; pi < path.Length; pi++)
            n = Step(n, path[pi], pi, path.Length, path);
        Base[(int)n] = value;
        return n;
    }

    private long Step(long a2, byte a3, int pi, int kl, ReadOnlySpan<byte> path)
    {
        var v10 = a3 + (long)Base[(int)a2];
        while (v10 >= N)
            Grow();

        var v11 = (long)Check[(int)v10];

        // 目标槽空闲 → 直接占用
        if ((v11 & 0x80000000L) != 0 || v11 == 0)
        {
            RemoveFree(v10);
            Check[(int)v10] = (uint)U(a2);
            if (pi + 1 < kl)
                Base[(int)v10] = (uint)U(FindBase(NoChildren, path[pi + 1]));
            return v10;
        }

        // 冲突: 需要搬移
        if (v11 != U(a2))
        {
            var s = CollectChildren(a2);
            var s2 = CollectChildren(v11);

            if (v11 != 1 && (a2 == 1 || s.Count + 1 >= s2.Count))
            {
                // 搬移 v11 的子树
                var base2 = FindBase(s2, s2.Count > 0 ? s2[^1] : 0);
                var a2n = Relocate(a2, v11, base2, s2, s2.Count);
                a2 = a2n;                       // a2 被搬走了 → 父指针必须用新节点号
                v10 = a3 + (long)Base[(int)a2n];
            }
            else
            {
                // 搬移 a2 的子树
                var base1 = FindBase(s, a3);
                Relocate(a2, a2, base1, s, s.Count);
                v10 = base1 + a3;
            }

            while (v10 >= N)
                Grow();
            RemoveFree(v10);
            Check[(int)v10] = (uint)U(a2);
            if (pi + 1 < kl)
                Base[(int)v10] = (uint)U(FindBase(NoChildren, path[pi + 1]));
            return v10;
        }

        return v10;
    }
}
