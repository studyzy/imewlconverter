namespace ImeWlConverter.Formats.LibIMEPinyin;

using System;
using System.IO;
using ZstdSharp.Unsafe;

/// <summary>
/// 逐字节复刻 libime 写出二进制词库时的压缩链路
/// （libime <c>core/zstdfilter.h</c> + Boost.Iostreams）：
///
/// <code>
/// std::ostream → boost::iostreams::chain（indirect_streambuf，128B）
///              → ZSTDCompressor（symmetric_filter，4096B 输出缓冲）
///              → libzstd（ZSTD_initCStream(level 0) + ZSTD_c_checksumFlag=1）
/// </code>
///
/// 复刻的原因不是"还原实现细节"，而是 boost <c>symmetric_filter::close()</c>
/// 会产生一个可观察的产物：关闭时它以空输入反复调用 filter(flush=true)，
/// 直到内部 4096B 输出缓冲可以排空。如果关闭时 zstd 内部积压的输出超过一个
/// 缓冲，循环就要多跑一轮；这一轮的 <c>ZSTD_compressStream</c> 正好把当前帧
/// 的剩余输出写完，紧跟的 <c>ZSTD_endStream</c> 于是新开并立即结束了一个空帧，
/// 于是在文件末尾多出 13 字节的空 zstd 帧（<c>28 b5 2f fd 24 00 01 00 00 99 e9 d8 51</c>）。
///
/// 结论：libime_pinyindict / Fcitx5 写出的 <c>*.dict</c> 尾部会带这个空帧，
/// 且是否带它取决于关闭时刻 zstd 内部的积压量（数据相关，不是简单的大小阈值：
/// 随机数据在 128KiB 整数倍时没有积压，就不带空帧）。只有同样复刻这套缓冲与
/// flush 循环，才能做到与 libime 字节级一致。
/// </summary>
internal sealed unsafe class LibimeZstdWriter : Stream
{
    /// <summary>boost::iostreams::default_device_buffer_size，即 ZSTDCompressor 的内层输出缓冲。</summary>
    private const int FilterBufferSize = 4096;

    /// <summary>boost::iostreams::default_filter_buffer_size，即链上过滤器 streambuf 的缓冲。</summary>
    private const int OuterBufferSize = 128;

    private readonly Stream _sink;
    private readonly byte[] _filterBuffer = new byte[FilterBufferSize];
    private readonly byte[] _outerBuffer = new byte[OuterBufferSize];
    private readonly byte[] _scratch = new byte[1];

    private ZSTD_CCtx_s* _cctx;
    private int _filterPtr;
    private int _filterEptr;
    private int _outerCount;
    private bool _writeStarted;
    private bool _closed;
    private bool _eof;

    public LibimeZstdWriter(Stream sink)
    {
        _sink = sink ?? throw new ArgumentNullException(nameof(sink));

        _cctx = Methods.ZSTD_createCStream();
        if (_cctx == null)
            throw new InvalidOperationException("无法创建 zstd 压缩上下文。");

        ResetCStream();
    }

    /// <inheritdoc/>
    public override bool CanRead => false;

    /// <inheritdoc/>
    public override bool CanSeek => false;

    /// <inheritdoc/>
    public override bool CanWrite => true;

    /// <inheritdoc/>
    public override long Length => throw new NotSupportedException();

    /// <inheritdoc/>
    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    /// <inheritdoc/>
    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    /// <inheritdoc/>
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    /// <inheritdoc/>
    public override void SetLength(long value) => throw new NotSupportedException();

    /// <summary>
    /// boost 的 <c>indirect_streambuf::sync()</c> 只把 128B 缓冲推进到过滤器，
    /// 不会排空 symmetric_filter 自己的 4096B 缓冲，这里保持一致。
    /// 因此本方法**不保证字节已经落到 sink**，调用方不要依赖它落盘；
    /// 压缩数据的收尾只能靠 <see cref="Dispose"/>。
    /// </summary>
    public override void Flush() => FlushOuterBuffer();

    /// <inheritdoc/>
    public override void Write(byte[] buffer, int offset, int count)
        => Write(buffer.AsSpan(offset, count));

    /// <inheritdoc/>
    public override void Write(ReadOnlySpan<byte> buffer)
    {
        ObjectDisposedException.ThrowIf(_closed, this);

        while (!buffer.IsEmpty)
        {
            var n = Math.Min(buffer.Length, OuterBufferSize - _outerCount);
            buffer[..n].CopyTo(_outerBuffer.AsSpan(_outerCount));
            _outerCount += n;
            buffer = buffer[n..];

            if (_outerCount == OuterBufferSize)
                FlushOuterBuffer();
        }
    }

    /// <inheritdoc/>
    public override void WriteByte(byte value)
    {
        ObjectDisposedException.ThrowIf(_closed, this);

        _outerBuffer[_outerCount++] = value;
        if (_outerCount == OuterBufferSize)
            FlushOuterBuffer();
    }

    protected override void Dispose(bool disposing)
    {
        if (!_closed)
        {
            _closed = true;

            if (disposing)
            {
                try
                {
                    CloseFilter();
                }
                finally
                {
                    if (_cctx != null)
                    {
                        Methods.ZSTD_freeCStream(_cctx);
                        _cctx = null;
                    }
                }
            }
        }

        base.Dispose(disposing);
    }

    // ---------------------------------------------------------------- boost 链

    /// <summary>boost <c>indirect_streambuf::sync_impl()</c>：把 128B 缓冲交给过滤器。</summary>
    private void FlushOuterBuffer()
    {
        if (_outerCount == 0)
            return;

        var pending = _outerBuffer.AsSpan(0, _outerCount);
        _outerCount = 0;
        SymmetricWrite(pending);
    }

    /// <summary>boost <c>symmetric_filter::write()</c>。</summary>
    private void SymmetricWrite(ReadOnlySpan<byte> input)
    {
        BeginWrite();

        var offset = 0;
        while (offset < input.Length)
        {
            if (_filterPtr == _filterEptr && !FlushFilterBuffer())
                break;

            if (!Filter(input[offset..], flush: false, out var consumed))
            {
                FlushFilterBuffer();
                break;
            }

            offset += consumed;
        }
    }

    /// <summary>boost <c>symmetric_filter::begin_write()</c>。</summary>
    private void BeginWrite()
    {
        if (_writeStarted)
            return;

        _writeStarted = true;
        _filterPtr = 0;
        _filterEptr = _filterBuffer.Length;
    }

    /// <summary>boost <c>symmetric_filter::close(snk, out)</c> 加 <c>close_impl()</c>。</summary>
    private void CloseFilter()
    {
        FlushOuterBuffer();
        BeginWrite();

        var again = true;
        while (again)
        {
            if (_filterPtr != _filterEptr)
                again = Filter(ReadOnlySpan<byte>.Empty, flush: true, out _);

            FlushFilterBuffer();
        }

        // close_impl(): state = 0; buf.set(0, 0); filter().close()
        _writeStarted = false;
        _filterPtr = 0;
        _filterEptr = 0;
        ResetCStream();
    }

    /// <summary>boost <c>symmetric_filter::flush(snk)</c>：把 4096B 缓冲写进 sink。</summary>
    /// <returns>本次是否真的写出了字节。</returns>
    private bool FlushFilterBuffer()
    {
        var count = _filterPtr;
        if (count > 0)
            _sink.Write(_filterBuffer, 0, count);

        _filterPtr = 0;
        _filterEptr = _filterBuffer.Length;
        return count != 0;
    }

    // ------------------------------------------------- libime ZSTDCompressorImpl

    /// <summary>
    /// libime <c>ZSTDFilterBase::before()/after()</c> + <c>ZSTDCompressorImpl::filter()</c>。
    /// </summary>
    /// <returns>与 libime 一致：true 表示"还需要再调用一次"。</returns>
    private bool Filter(ReadOnlySpan<byte> input, bool flush, out int consumed)
    {
        if (input.IsEmpty)
        {
            // 关闭阶段的调用传入空区间，指针仍必须有效（libime 传的是 &dummy）。
            fixed (byte* pEmpty = _scratch)
            fixed (byte* pOut = _filterBuffer)
            {
                return FilterCore(pEmpty, 0, pOut, flush, out consumed);
            }
        }

        fixed (byte* pInput = input)
        fixed (byte* pOut = _filterBuffer)
        {
            return FilterCore(pInput, input.Length, pOut, flush, out consumed);
        }
    }

    private bool FilterCore(byte* pInput, int inputLength, byte* pOut, bool flush, out int consumed)
    {
        var inBuf = new ZSTD_inBuffer_s
        {
            src = pInput,
            size = (nuint)inputLength,
            pos = 0,
        };
        var outBuf = new ZSTD_outBuffer_s
        {
            dst = pOut + _filterPtr,
            size = (nuint)(_filterEptr - _filterPtr),
            pos = 0,
        };

        var result = Deflate(&inBuf, &outBuf, flush);

        consumed = (int)inBuf.pos;
        _filterPtr += (int)outBuf.pos;
        return result != ZstdResult.StreamEnd;
    }

    /// <summary>libime <c>ZSTDCompressorImpl::deflate()</c>。</summary>
    private ZstdResult Deflate(ZSTD_inBuffer_s* input, ZSTD_outBuffer_s* output, bool finish)
    {
        // 忽略多余的调用：已结束且无新输入时直接返回流结束。
        if (_eof && input->size == input->pos)
            return ZstdResult.StreamEnd;

        Check(Methods.ZSTD_compressStream(_cctx, output, input));

        if (!finish)
            return ZstdResult.Okay;

        var result = Methods.ZSTD_endStream(_cctx, output);
        Check(result);
        _eof = result == 0;
        return _eof ? ZstdResult.StreamEnd : ZstdResult.Okay;
    }

    /// <summary>libime <c>ZSTDCompressorImpl::reset()</c>。</summary>
    private void ResetCStream()
    {
        Check(Methods.ZSTD_initCStream(_cctx, 0));
        Check(Methods.ZSTD_CCtx_setParameter(_cctx, ZSTD_cParameter.ZSTD_c_checksumFlag, 1));
        _eof = false;
    }

    private static void Check(nuint code)
    {
        if (Methods.ZSTD_isError(code))
            throw new InvalidDataException($"libime 词库压缩失败: {Methods.ZSTD_getErrorName(code)}。");
    }

    private enum ZstdResult
    {
        StreamEnd,
        Okay,
    }
}
